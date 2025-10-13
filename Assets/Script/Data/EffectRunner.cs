using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class EffectRunner
{
    public static string Apply(string effects, StoryLine line)
    {
        if (string.IsNullOrWhiteSpace(effects)) return null;

        var randCache = new Dictionary<int, bool>();
        string pre = PreprocessRand(effects, randCache);

        string jump = null;
        bool anyMatched = false;

        foreach (var raw in pre.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string s = raw.Trim();
            if (s.Length == 0) continue;

            if (s.StartsWith("else:", StringComparison.OrdinalIgnoreCase))
            {
                if (!anyMatched)
                {
                    string eff = s[(s.IndexOf(':') + 1)..].Trim();
                    jump = ApplyOne(eff, line) ?? jump;
                    anyMatched = true;
                }
                continue;
            }

            var qm = Regex.Match(s, @"^(.*?)\?(.*)$");
            if (qm.Success)
            {
                string cond = qm.Groups[1].Value.Trim();
                string eff = qm.Groups[2].Value.Trim();

                bool pass = EvaluateCondFast(cond, line);
                if (pass)
                {
                    jump = ApplyOne(eff, line) ?? jump;
                    anyMatched = true;
                }
                continue;
            }

            jump = ApplyOne(s, line) ?? jump;
        }

        return jump;
    }

    public static string RunNodeChain(DialogueNode node, StoryLine line)
    {
        if (node == null) return null;

        if (ConditionEvaluator.Evaluate(node.Conditions, line))
            return Apply(node.Effects, line);

        if (ConditionEvaluator.Evaluate(node.ElseIfConditions, line))
            return Apply(node.ElseIfEffects, line);

        return Apply(node.ElseEffects, line);
    }

    // rand() 전처리 (같은 p는 1회만 추첨)
    static string PreprocessRand(string s, Dictionary<int, bool> cache)
    {
        s = Regex.Replace(s, @"!rand\((\d{1,3})\)", m =>
        {
            int p = ClampP(m.Groups[1].Value);
            bool v = SampleRand(p, cache);
            return v ? "FALSE" : "TRUE";
        }, RegexOptions.IgnoreCase);

        s = Regex.Replace(s, @"rand\((\d{1,3})\)", m =>
        {
            int p = ClampP(m.Groups[1].Value);
            bool v = SampleRand(p, cache);
            return v ? "TRUE" : "FALSE";
        }, RegexOptions.IgnoreCase);

        return s;
    }
    static int ClampP(string pText) { int p = 0; int.TryParse(pText, out p); return Mathf.Clamp(p, 0, 100); }
    static bool SampleRand(int p, Dictionary<int, bool> cache)
    {
        if (!cache.TryGetValue(p, out var v))
        {
            int roll = UnityEngine.Random.Range(1, 101);
            v = (roll <= p);
            cache[p] = v;
        }
        return v;
    }
    static bool EvaluateCondFast(string cond, StoryLine line)
    {
        string u = cond.Trim().ToUpperInvariant();
        if (u == "TRUE" || u == "1") return true;
        if (u == "FALSE" || u == "0") return false;
        return ConditionEvaluator.Evaluate(cond, line);
    }

    // 단일 효과 실행 (jump 대상 반환)
    static string ApplyOne(string s, StoryLine line)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        // jump/goto/next:ID
        if (s.StartsWith("jump:", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("goto:", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("next:", StringComparison.OrdinalIgnoreCase))
            return s[(s.IndexOf(':') + 1)..].Trim();

        // flag
        var mFlagSet = Regex.Match(s, @"^flag\.(\w+)\s*=\s*(\d+)$", RegexOptions.IgnoreCase);
        if (mFlagSet.Success)
        {
            string key = mFlagSet.Groups[1].Value;
            bool on = mFlagSet.Groups[2].Value != "0";
            if (on) GameState.I.SetFlag(key);
            else GameState.I.RemoveFlag(key);
            return null;
        }
        var mFlagTrue = Regex.Match(s, @"^flag\.(\w+)$", RegexOptions.IgnoreCase);
        if (mFlagTrue.Success) { GameState.I.SetFlag(mFlagTrue.Groups[1].Value); return null; }
        var mFlagFalse = Regex.Match(s, @"^!flag\.(\w+)$", RegexOptions.IgnoreCase);
        if (mFlagFalse.Success) { GameState.I.RemoveFlag(mFlagFalse.Groups[1].Value); return null; }

        // var.scope.key = / += / -=
        var mVar = Regex.Match(s, @"^var\.(\w+)\.(\w+)\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mVar.Success)
        {
            string scope = mVar.Groups[1].Value;
            string key = mVar.Groups[2].Value;
            string op = mVar.Groups[3].Value;
            int v = int.Parse(mVar.Groups[4].Value);

            int cur = GameState.I.GetVar(scope, key);
            int delta = (op == "=") ? (v - cur) : (op == "+=" ? v : -v);
            if (delta != 0) GameState.I.AddVar(scope, key, delta);
            return null;
        }

        // item[Key] = / += / -=
        var mItem = Regex.Match(s, @"^item\[(.+?)\]\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mItem.Success)
        {
            string key = mItem.Groups[1].Value;
            string op = mItem.Groups[2].Value;
            int v = int.Parse(mItem.Groups[3].Value);

            int cur = GameState.I.GetItem(key);
            int delta = (op == "=") ? (v - cur) : (op == "+=" ? v : -v);
            if (delta != 0) GameState.I.AddItem(key, delta);
            return null;
        }

        // affinity[Name] += / -= / =
        var mAff = Regex.Match(s, @"^affinity\[(.+?)\]\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mAff.Success)
        {
            string who = mAff.Groups[1].Value;
            string op = mAff.Groups[2].Value;
            int v = int.Parse(mAff.Groups[3].Value);

            int cur = GameState.I.GetAffinity(who);
            int delta = (op == "=") ? (v - cur) : (op == "+=" ? v : -v);
            if (delta != 0) GameState.I.AddAffinity(who, delta);
            return null;
        }

        // relation[a,b] += / -= / =
        var mRel = Regex.Match(s, @"^relation\[(.+?)\s*,\s*(.+?)\]\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mRel.Success)
        {
            string a = mRel.Groups[1].Value;
            string b = mRel.Groups[2].Value;
            string op = mRel.Groups[3].Value;
            int v = int.Parse(mRel.Groups[4].Value);

            int cur = GameState.I.GetRelation(a, b);
            int delta = (op == "=") ? (v - cur) : (op == "+=" ? v : -v);
            if (delta != 0) GameState.I.AddRelation(a, b, delta);
            return null;
        }

        // gold/stamina 단축 (var.gold.gold 권장)
        var mGold = Regex.Match(s, @"^gold\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mGold.Success)
        {
            int v = int.Parse(mGold.Groups[2].Value);
            int cur = GameState.I.GetVar("gold", "gold");
            int delta = (mGold.Groups[1].Value == "=") ? (v - cur) : (mGold.Groups[1].Value == "+=" ? v : -v);
            if (delta != 0) GameState.I.AddVar("gold", "gold", delta);
            return null;
        }
        var mSta = Regex.Match(s, @"^stamina\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mSta.Success)
        {
            int v = int.Parse(mSta.Groups[2].Value);
            int cur = GameState.I.GetVar("stamina", "stamina");
            int delta = (mSta.Groups[1].Value == "=") ? (v - cur) : (mSta.Groups[1].Value == "+=" ? v : -v);
            if (delta != 0) GameState.I.AddVar("stamina", "stamina", delta);
            return null;
        }

        return null;
    }
}

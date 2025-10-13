using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class EffectRunner
{
    // (옵션) 외부 UI 갱신용 훅: scope/key/delta 알림
    public static Action<string, string, int> OnStatApplied;

    /// <summary>
    /// 세미콜론(;)으로 구분된 효과 문자열 실행.
    /// 지원:
    ///  - cond ? effect      (삼항식 유사)
    ///  - else: effect       (앞의 어떤 조건도 매치 안되면 실행)
    ///  - rand(p) / !rand(p) (0~100, 같은 p는 한 번만 추첨해 공유 → 70/30 정확 보장)
    ///  - jump/goto/next:NodeId  (점프 반환)
    ///  - flag./var./item[]/affinity[]/relation[a,b] (GameState API 직호출)
    ///  - gold/stamina는 var 스코프로 처리: var.gold.gold, var.stamina.stamina
    /// 반환: 점프할 NodeId (없으면 null)
    /// </summary>
    public static string Apply(string effects, StoryLine line)
    {
        if (string.IsNullOrWhiteSpace(effects)) return null;

        // 1) rand() 전처리: 같은 p는 한 번만 추첨해서 공유
        var randCache = new Dictionary<int, bool>();
        string pre = PreprocessRand(effects, randCache);

        // 2) ; 단위 처리
        string jump = null;
        bool anyMatched = false;

        foreach (var raw in pre.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string s = raw.Trim();
            if (s.Length == 0) continue;

            // else: effect
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

            // cond ? effect
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

            // 단순 effect
            jump = ApplyOne(s, line) ?? jump;
        }

        return jump;
    }

    /// <summary>
    /// DialogueNode의 조건/효과 체인 실행 후 점프 반환
    /// </summary>
    public static string RunNodeChain(DialogueNode node, StoryLine line)
    {
        if (node == null) return null;

        if (ConditionEvaluator.Evaluate(node.Conditions, line))
            return Apply(node.Effects, line);

        if (ConditionEvaluator.Evaluate(node.ElseIfConditions, line))
            return Apply(node.ElseIfEffects, line);

        return Apply(node.ElseEffects, line);
    }

    // ─────────────────────────────────────────────
    // 내부: rand() / 조건 / 단일 효과 적용
    // ─────────────────────────────────────────────

    static string PreprocessRand(string s, Dictionary<int, bool> cache)
    {
        // !rand(p) 먼저 치환
        s = Regex.Replace(s, @"!rand\((\d{1,3})\)", m =>
        {
            int p = ClampP(m.Groups[1].Value);
            bool v = SampleRand(p, cache);
            return v ? "FALSE" : "TRUE";
        }, RegexOptions.IgnoreCase);

        // rand(p)
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
            // Debug.Log($"[EffectRunner] rand({p}) -> {v} (roll={roll})");
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

    // ---------------- 단일 효과 실행 (jump 대상 반환) ----------------
    static string ApplyOne(string s, StoryLine line)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        // jump/goto/next:ID
        if (s.StartsWith("jump:", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("goto:", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("next:", StringComparison.OrdinalIgnoreCase))
        {
            return s[(s.IndexOf(':') + 1)..].Trim();
        }

        // ── 1) flag
        //  - flag.key=1 / flag.key (true)
        //  - flag.key=0 / !flag.key  → 현재 GameState에 해제 API가 없어서 경고만.
        var mFlagSet = Regex.Match(s, @"^flag\.(\w+)\s*=\s*(\d+)$", RegexOptions.IgnoreCase);
        if (mFlagSet.Success)
        {
            string key = mFlagSet.Groups[1].Value;
            bool on = mFlagSet.Groups[2].Value != "0";
            if (on) GameState.I.SetFlag(key);
            else Debug.LogWarning($"[EffectRunner] flag.{key}=0 요청됐지만 GameState에 해제 API가 없어 무시됨.");
            return null;
        }
        var mFlagTrue = Regex.Match(s, @"^flag\.(\w+)$", RegexOptions.IgnoreCase);
        if (mFlagTrue.Success) { GameState.I.SetFlag(mFlagTrue.Groups[1].Value); return null; }
        var mFlagFalse = Regex.Match(s, @"^!flag\.(\w+)$", RegexOptions.IgnoreCase);
        if (mFlagFalse.Success) { Debug.LogWarning($"[EffectRunner] !flag.{mFlagFalse.Groups[1].Value} 요청됐지만 해제 API가 없어 무시됨."); return null; }

        // ── 2) var.scope.key = / += / -=  (AddVar/GetVar 사용)
        // 예) var.affinity.SORA+=1, var.item.Key+=2, var.gold.gold+=100, var.stamina.stamina-=1
        var mVar = Regex.Match(s, @"^var\.(\w+)\.(\w+)\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mVar.Success)
        {
            string scope = mVar.Groups[1].Value; // affinity/item/gold/stamina/...
            string key = mVar.Groups[2].Value;
            string op = mVar.Groups[3].Value;
            int v = int.Parse(mVar.Groups[4].Value);

            int cur = GameState.I.GetVar(scope, key);
            int delta = 0;
            if (op == "=") delta = v - cur;
            if (op == "+=") delta = v;
            if (op == "-=") delta = -v;

            if (delta != 0)
            {
                GameState.I.AddVar(scope, key, delta);
                OnStatApplied?.Invoke(scope, key, delta);
            }
            return null;
        }

        // ── 3) item[Key] = / += / -=  (직접 API)
        var mItem = Regex.Match(s, @"^item\[(.+?)\]\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mItem.Success)
        {
            string key = mItem.Groups[1].Value;
            string op = mItem.Groups[2].Value;
            int v = int.Parse(mItem.Groups[3].Value);

            int cur = GameState.I.GetItem(key);
            int delta = (op == "=") ? (v - cur) : (op == "+=" ? v : -v);
            if (delta != 0)
            {
                GameState.I.AddItem(key, delta);
                OnStatApplied?.Invoke("item", key, delta);
            }
            return null;
        }

        // ── 4) affinity[Name] += / -= / =
        var mAff = Regex.Match(s, @"^affinity\[(.+?)\]\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mAff.Success)
        {
            string who = mAff.Groups[1].Value;
            string op = mAff.Groups[2].Value;
            int v = int.Parse(mAff.Groups[3].Value);

            int cur = GameState.I.GetAffinity(who);
            int delta = (op == "=") ? (v - cur) : (op == "+=" ? v : -v);
            if (delta != 0)
            {
                GameState.I.AddAffinity(who, delta);
                OnStatApplied?.Invoke("affinity", who, delta);
            }
            return null;
        }

        // ── 5) relation[a,b] += / -= / =
        var mRel = Regex.Match(s, @"^relation\[(.+?)\s*,\s*(.+?)\]\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mRel.Success)
        {
            string a = mRel.Groups[1].Value;
            string b = mRel.Groups[2].Value;
            string op = mRel.Groups[3].Value;
            int v = int.Parse(mRel.Groups[4].Value);

            int cur = GameState.I.GetRelation(a, b);
            int delta = (op == "=") ? (v - cur) : (op == "+=" ? v : -v);
            if (delta != 0)
            {
                GameState.I.AddRelation(a, b, delta);
                OnStatApplied?.Invoke("relation", $"{a}|{b}", delta);
            }
            return null;
        }

        // gold/stamina는 var 스코프로 쓰길 권장하지만, 구문 단축도 허용
        var mGold = Regex.Match(s, @"^gold\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mGold.Success)
        {
            int v = int.Parse(mGold.Groups[2].Value);
            int cur = GameState.I.GetVar("gold", "gold");
            int delta = (mGold.Groups[1].Value == "=") ? (v - cur) : (mGold.Groups[1].Value == "+=" ? v : -v);
            if (delta != 0) { GameState.I.AddVar("gold", "gold", delta); OnStatApplied?.Invoke("gold", "gold", delta); }
            return null;
        }
        var mSta = Regex.Match(s, @"^stamina\s*(\+?=|\-?=|=)\s*(\-?\d+)$", RegexOptions.IgnoreCase);
        if (mSta.Success)
        {
            int v = int.Parse(mSta.Groups[2].Value);
            int cur = GameState.I.GetVar("stamina", "stamina");
            int delta = (mSta.Groups[1].Value == "=") ? (v - cur) : (mSta.Groups[1].Value == "+=" ? v : -v);
            if (delta != 0) { GameState.I.AddVar("stamina", "stamina", delta); OnStatApplied?.Invoke("stamina", "stamina", delta); }
            return null;
        }

        // 그 외는 무시(필요 시 확장)
        return null;
    }
}

using System;
using System.Text.RegularExpressions;

public static class ConditionEvaluator
{
    /// <summary>
    /// 조건 문자열을 평가한다. 비어 있으면 true.
    /// 예) "affinity:YUNA>=50; item:Ticket>=1; gold>=100; flag:Key; relation:YUNA/MINJI>=20; Day=2"
    /// </summary>
    public static bool Evaluate(string condition, StoryLine line)
    {
        if (string.IsNullOrWhiteSpace(condition)) return true;

        var parts = condition.Split(';');
        foreach (var raw in parts)
        {
            var s = raw.Trim();
            if (string.IsNullOrEmpty(s)) continue;

            // hasItem:Camera
            if (s.StartsWith("hasItem:", StringComparison.OrdinalIgnoreCase))
            {
                var id = s.Substring("hasItem:".Length).Trim();
                if (GameState.I.GetItem(id) <= 0) return false;
                continue;
            }

            // flag:SomeFlag
            if (s.StartsWith("flag:", StringComparison.OrdinalIgnoreCase))
            {
                var id = s.Substring("flag:".Length).Trim();
                if (!GameState.I.HasFlag(id)) return false;
                continue;
            }

            // Day=2
            if (Regex.IsMatch(s, @"^\s*Day\s*=\s*\d+\s*$"))
            {
                var rhs = Regex.Match(s, @"\d+").Value;
                if (line == null || line.Day != rhs) return false;
                continue;
            }

            // relation:A,B OP N   또는   relation:A/B OP N
            var mRel = Regex.Match(
                s,
                @"^relation:(?<A>[\w-]+)[,\/](?<B>[\w-]+)\s*(?<op>>=|<=|==|!=|>|<)\s*(?<val>-?\d+)$",
                RegexOptions.IgnoreCase
            );
            if (mRel.Success)
            {
                string A = mRel.Groups["A"].Value.Trim();
                string B = mRel.Groups["B"].Value.Trim();
                int lhs = GameState.I.GetRelation(A, B);
                int rhs = int.Parse(mRel.Groups["val"].Value);
                string op = mRel.Groups["op"].Value;
                if (!Compare(lhs, op, rhs)) return false;
                continue;
            }

            // scope:key OP value  (affinity:YUNA>=50, item:Ticket>=1 …)
            var m1 = Regex.Match(s, @"^(?<scope>\w+):(?<key>[\w-]+)\s*(?<op>>=|<=|==|!=|>|<)\s*(?<val>-?\d+)$");
            if (m1.Success)
            {
                string scope = m1.Groups["scope"].Value;
                string key = m1.Groups["key"].Value;
                int lhs = GameState.I.GetVar(scope, key);
                int rhs = int.Parse(m1.Groups["val"].Value);
                string op = m1.Groups["op"].Value;
                if (!Compare(lhs, op, rhs)) return false;
                continue;
            }

            // scope OP value  (gold>=100, stamina>3)
            var m2 = Regex.Match(s, @"^(?<scope>\w+)\s*(?<op>>=|<=|==|!=|>|<)\s*(?<val>-?\d+)$");
            if (m2.Success)
            {
                string scope = m2.Groups["scope"].Value;
                int lhs = GameState.I.GetVar(scope, "");
                int rhs = int.Parse(m2.Groups["val"].Value);
                string op = m2.Groups["op"].Value;
                if (!Compare(lhs, op, rhs)) return false;
                continue;
            }

            // 알 수 없는 토큰 → 안전하게 false
            return false;
        }
        return true;
    }

    private static bool Compare(int lhs, string op, int rhs)
    {
        switch (op)
        {
            case ">=": return lhs >= rhs;
            case "<=": return lhs <= rhs;
            case "==": return lhs == rhs;
            case "!=": return lhs != rhs;
            case ">": return lhs > rhs;
            case "<": return lhs < rhs;
        }
        return false;
    }
}

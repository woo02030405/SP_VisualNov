using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class EffectRunner
{
    /// <summary>
    /// 한 줄 효과를 실행한다. jump 대상이 있으면 그 NodeId를 반환.
    /// </summary>
    public static string Apply(string effect, StoryLine line)
    {
        if (string.IsNullOrWhiteSpace(effect)) return null;

        string jumpTarget = null;
        var parts = effect.Split(';');

        System.Random rng = new System.Random(); // rand() 용

        foreach (var raw in parts)
        {
            var s = raw.Trim();
            if (string.IsNullOrEmpty(s)) continue;

            // 수치 가감: scope:key±N  (affinity:YUNA+5, item:Ticket-1, gold:+50, stamina:-2)
            var add = Regex.Match(s, @"^(?<scope>\w+):(?<key>[\w-]*)\s*(?<num>[+\-]\d+)$");
            if (add.Success)
            {
                var scope = add.Groups["scope"].Value;
                var key = add.Groups["key"].Value;
                int delta = int.Parse(add.Groups["num"].Value);
                GameState.I.AddVar(scope, key, delta);
                continue;
            }

            // 플래그 세팅
            if (s.StartsWith("flag:", StringComparison.OrdinalIgnoreCase))
            {
                GameState.I.SetFlag(s.Substring("flag:".Length).Trim());
                continue;
            }

            // 관계도 가감: relation:A,B±N  또는 relation:A/B±N
            var rel = Regex.Match(
                s,
                @"^relation:(?<A>[\w-]+)[,\/](?<B>[\w-]+)\s*(?<num>[+\-]\d+)$",
                RegexOptions.IgnoreCase
            );
            if (rel.Success)
            {
                string A = rel.Groups["A"].Value.Trim();
                string B = rel.Groups["B"].Value.Trim();
                int delta = int.Parse(rel.Groups["num"].Value);
                GameState.I.AddRelation(A, B, delta);
                continue;
            }

            // 잠금 계열 → 플래그로
            if (s.StartsWith("lock:", StringComparison.OrdinalIgnoreCase))
            {
                GameState.I.SetFlag($"lock:{s.Substring("lock:".Length).Trim()}");
                continue;
            }

            // 즉시 분기
            if (s.StartsWith("jump:", StringComparison.OrdinalIgnoreCase))
            {
                jumpTarget = s.Substring("jump:".Length).Trim();
                continue;
            }

            // rand(p)?jump:ID  (p% 확률로 분기)
            var r = Regex.Match(s, @"^rand\((?<p>\d{1,3})\)\?jump:(?<target>[\w_]+)$", RegexOptions.IgnoreCase);
            if (r.Success)
            {
                int p = Mathf.Clamp(int.Parse(r.Groups["p"].Value), 0, 100);
                int roll = rng.Next(0, 100);
                if (roll < p) jumpTarget = r.Groups["target"].Value;
                continue;
            }

            // !rand(p)?jump:ID  (p% 실패 시 분기)
            var rn = Regex.Match(s, @"^!rand\((?<p>\d{1,3})\)\?jump:(?<target>[\w_]+)$", RegexOptions.IgnoreCase);
            if (rn.Success)
            {
                int p = Mathf.Clamp(int.Parse(rn.Groups["p"].Value), 0, 100);
                int roll = rng.Next(0, 100);
                if (roll >= p) jumpTarget = rn.Groups["target"].Value;
                continue;
            }

            // 메시지 트리거(툴팁/경고 UI와 연동용)
            if (s.StartsWith("show_message:", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("message:", StringComparison.OrdinalIgnoreCase))
            {
                // 훅만 남김
                continue;
            }

            Debug.LogWarning($"[EffectRunner] 알 수 없는 이펙트: {s}");
        }

        return jumpTarget;
    }

    /// <summary>
    /// 하나의 DialogueNode에 대해 Conditions→ElseIf→Else 체인을 실행하고 jump를 반환.
    /// (대화 진입 시 자동 실행에 사용)
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

    /// <summary>
    /// 스킵 페널티 전용 (원할 때 외부에서 호출)
    /// </summary>
    public static void ApplySkipPenalty(DialogueNode node, StoryLine line)
    {
        if (node == null) return;
        Apply(node.SkipPenalty, line);
    }
}

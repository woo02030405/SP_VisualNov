using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class EffectRunner
{
    // 수치 변동 발생 시 UI 등에 통지하고 싶을 때 쓰는 이벤트 (옵션)
    // targetId: 보통 캐릭터ID(예: YUNA), statKey: "affinity:YUNA"/"gold"/"item:Ticket" 등, delta: +/-
    public static Action<string, string, int> OnStatApplied;

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

                string statKey = scope.ToLower() switch
                {
                    "affinity" => $"affinity:{key}",
                    "item" => $"item:{key}",
                    "gold" => "gold",
                    "stamina" => "stamina",
                    _ => scope
                };
                OnStatApplied?.Invoke(string.IsNullOrEmpty(key) ? null : key, statKey, delta);
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
                OnStatApplied?.Invoke(A, "relation", delta);
                continue;
            }

            // 잠금 → 플래그로 저장
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

            // 메시지 트리거(툴팁/경고 UI와 연동용) — 여기서는 실제 표시 X
            if (s.StartsWith("show_message:", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("message:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Debug.LogWarning($"[EffectRunner] 알 수 없는 이펙트: {s}");
        }

        return jumpTarget;
    }

    /// <summary> 노드 체인(Conditions→ElseIf→Else)을 실행하고 jump를 반환. </summary>
    public static string RunNodeChain(DialogueNode node, StoryLine line)
    {
        if (node == null) return null;

        if (ConditionEvaluator.Evaluate(node.Conditions, line))
            return Apply(node.Effects, line);

        if (ConditionEvaluator.Evaluate(node.ElseIfConditions, line))
            return Apply(node.ElseIfEffects, line);

        return Apply(node.ElseEffects, line);
    }

    /// <summary> 스킵 페널티 전용 </summary>
    public static void ApplySkipPenalty(DialogueNode node, StoryLine line)
    {
        if (node == null) return;
        Apply(node.SkipPenalty, line);
    }
}

using UnityEngine;
using VN.Dialogue;

public static class EffectManager
{
    // 조건 체크 (항상 true, 나중에 SaveData 연동)
    public static bool CheckCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return true;

        Debug.Log($"[EffectManager] Checking condition: {condition}");
        return true; // 지금은 항상 true
    }

    // 단일 효과 적용
    public static void ApplyEffect(string effect)
    {
        if (string.IsNullOrEmpty(effect)) return;

        Debug.Log($"[EffectManager] Applying effect: {effect}");
        // TODO: 나중에 SaveData 연동
    }

    // 여러 효과 처리
    public static void ApplyEffects(string effects)
    {
        if (string.IsNullOrEmpty(effects)) return;

        string[] tokens = effects.Split(';');
        foreach (var eff in tokens)
        {
            ApplyEffect(eff.Trim());
        }
    }

    //  노드 단위 효과 적용
    public static void ApplyNodeEffects(DialogueNode node, bool includeSkipPenalty)
    {
        if (node == null) return;

        if (!string.IsNullOrEmpty(node.Effects))
            ApplyEffects(node.Effects);

        if (!string.IsNullOrEmpty(node.ElseIfEffects))
            ApplyEffects(node.ElseIfEffects);

        if (!string.IsNullOrEmpty(node.ElseEffects))
            ApplyEffects(node.ElseEffects);

        if (includeSkipPenalty && !string.IsNullOrEmpty(node.SkipPenalty))
            ApplyEffects(node.SkipPenalty);

        Debug.Log($"[EffectManager] Applied effects for Node={node.NodeId}, includeSkipPenalty={includeSkipPenalty}");
    }

}

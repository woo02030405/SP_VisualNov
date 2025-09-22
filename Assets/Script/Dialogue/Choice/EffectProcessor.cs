using System;
using UnityEngine;

public static class EffectProcessor
{

    public static void ApplyEffects(string effects, SaveData saveData)              // 효과 적용 후 SaveData에 반영
    {
        if (string.IsNullOrEmpty(effects) || saveData == null) return;              // 빈 문자열 또는 null 체크

        string[] effectList = effects.Split(';');
        foreach (var raw in effectList)
        {
            var effect = raw?.Trim();
            if (string.IsNullOrWhiteSpace(effect)) continue;

            try
            {
                if (effect.StartsWith("affinity:", StringComparison.OrdinalIgnoreCase)) // affinity:YUNA+1
                {
                    ProcessAffinity(effect, saveData);
                }
                else if (effect.StartsWith("relation:", StringComparison.OrdinalIgnoreCase)) // relation:YUNA,MINJI+2
                {
                    ProcessRelation(effect, saveData);
                }
                else if (effect.StartsWith("flag:", StringComparison.OrdinalIgnoreCase)) // flag:QuestDone=true
                {
                    ProcessFlag(effect, saveData);
                }
                else if (effect.StartsWith("item:", StringComparison.OrdinalIgnoreCase)) // item:Ticket+1
                {
                    ProcessItem(effect, saveData);
                }
                else if (effect.StartsWith("stat:", StringComparison.OrdinalIgnoreCase)) // stat:Creativity+2
                {
                    ProcessStat(effect, saveData);
                }   
                else if (effect.StartsWith("gold:", StringComparison.OrdinalIgnoreCase)) // gold:+200
                {
                    ProcessGold(effect, saveData);
                }
                else if (effect.StartsWith("jump:", StringComparison.OrdinalIgnoreCase)) // jump:END_A
                {
                    Debug.Log($"[EffectProcessor] Jump requested: {effect}");                     // jump:END_A → 강제 점프 (실제 처리 로직은 DialogueManager에서)

                }
                else if (effect.StartsWith("event:", StringComparison.OrdinalIgnoreCase)) // event:MiniGame_Basketball
                {
                    Debug.Log($"[EffectProcessor] Event triggered: {effect}");                     // event:MiniGame_Basketball → 미니게임 호출

                }
                else if (effect.StartsWith("rand(", StringComparison.OrdinalIgnoreCase)) // rand(30)?affinity:YUNA+1
                {   
                    ProcessRandom(effect, saveData);                                     // 랜덤 확률 효과
                }   
                else if (effect.StartsWith("unlock:", StringComparison.OrdinalIgnoreCase)) // unlock:CG_YUNA1  / unlock:EV_HappyEnd
                {   
                    string id = effect.Substring("unlock:".Length).Trim();            // CG_YUNA1 / EV_HappyEnd
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (id.StartsWith("CG_"))                                       // CG_YUNA1
                        {
                            saveData.UnlockCG(id);
                            Debug.Log($"[EffectProcessor] CG Unlocked: {id}");        // 갤러리에서 CG 잠금 해제
                        }
                        else if (id.StartsWith("EV_"))
                        {
                            saveData.UnlockEvent(id);
                            Debug.Log($"[EffectProcessor] Event Unlocked: {id}");      // 갤러리에서 이벤트 잠금 해제
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[EffectProcessor] Unknown effect: {effect}");       // 알 수 없는 효과
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EffectProcessor] Failed to process effect '{effect}': {ex}");  // 예외 처리
            }
        }
    }

    private static void ProcessAffinity(string effect, SaveData saveData)         // 호감도 증가/감소
    {

        string content = effect.Substring("affinity:".Length).Trim();               // "YUNA+1"
        if (string.IsNullOrEmpty(content)) return;

        int signIndex = Math.Max(content.LastIndexOf('+'), content.LastIndexOf('-')); // 마지막 + 또는 - 위치 찾기
        if (signIndex <= 0 || signIndex >= content.Length - 1) return;

        string key = content.Substring(0, signIndex).Trim();                        // "YUNA"
        string amt = content.Substring(signIndex);                                  // "+1" or "-1"
        if (int.TryParse(amt, out int delta))                                       // "+1" 또는 "-1"을 정수로 변환
        {
            saveData.AddAffinity(key, delta);                                       // SaveData에 반영
        }
    }

    private static void ProcessRelation(string effect, SaveData saveData)         // 관계도 증가/감소
    {
        // relation:YUNA,MINJI+2  / relation:YUNA-1
        string content = effect.Substring("relation:".Length).Trim();               // "YUNA,MINJI+2"
        if (string.IsNullOrEmpty(content)) return;

        int signIndex = Math.Max(content.LastIndexOf('+'), content.LastIndexOf('-')); // 마지막 + 또는 - 위치 찾기
        if (signIndex <= 0 || signIndex >= content.Length - 1) return;

        string keys = content.Substring(0, signIndex).Trim();                           // "YUNA,MINJI"
        string amt = content.Substring(signIndex);                                      // "+2" or "-1"

        if (!int.TryParse(amt, out int delta)) return;                                  // "+2" 또는 "-1"을 정수로 변환

        foreach (var k in keys.Split(','))                                              // "YUNA", "MINJI" 구분
        {
            var key = k.Trim();
            if (!string.IsNullOrEmpty(key))
                saveData.AddRelation(key, delta);
        }
    }

    private static void ProcessFlag(string effect, SaveData saveData)               // 플래그 설정
    {
        // flag:QuestDone=true
        string content = effect.Substring("flag:".Length).Trim();                   // "QuestDone=true"
        var parts = content.Split('=');                                             
        if (parts.Length != 2) return;                                              // '='로 구분된 부분이 2개가 아니면 무시

        string key = parts[0].Trim();
        if (bool.TryParse(parts[1].Trim(), out bool value))                         // "true" 또는 "false"를 bool로 변환
            saveData.SetFlag(key, value);                                          // SaveData에 반영
    }

    private static void ProcessItem(string effect, SaveData saveData)                // 아이템 수량 증가/감소
    {

        string content = effect.Substring("item:".Length).Trim();                   // "Ticket+1"
        if (string.IsNullOrEmpty(content)) return;

        int signIndex = Math.Max(content.LastIndexOf('+'), content.LastIndexOf('-')); // 마지막 + 또는 - 위치 찾기
        if (signIndex <= 0 || signIndex >= content.Length - 1) return;

        string key = content.Substring(0, signIndex).Trim();                    
        string amt = content.Substring(signIndex);                              
        if (int.TryParse(amt, out int delta))
        {
            saveData.AddItem(key, delta);
        }
    }

    private static void ProcessStat(string effect, SaveData saveData)      // 능력치 증가/감소
    {
        // stat:Creativity+2
        string content = effect.Substring("stat:".Length).Trim();
        if (string.IsNullOrEmpty(content)) return;

        int signIndex = Math.Max(content.LastIndexOf('+'), content.LastIndexOf('-'));
        if (signIndex <= 0 || signIndex >= content.Length - 1) return;

        string key = content.Substring(0, signIndex).Trim();
        string amt = content.Substring(signIndex);
        if (int.TryParse(amt, out int delta))
        {
            saveData.AddStat(key, delta);
        }
    }

    private static void ProcessGold(string effect, SaveData saveData)    // 골드 증가/감소
    {
        // gold:+200  / gold:-50
        string content = effect.Substring("gold:".Length).Trim();
        if (int.TryParse(content, out int delta))
            saveData.AddGold(delta);
    }

    private static void ProcessRandom(string effect, SaveData saveData)   // 랜덤 확률 효과
    {

        int open = effect.IndexOf('(');
        int close = effect.IndexOf(')');
        if (open < 0 || close < 0 || close <= open) return; 

        string chanceStr = effect.Substring(open + 1, close - open - 1);
        if (!int.TryParse(chanceStr, out int chance)) return;

        int q = effect.IndexOf('?', close + 1);
        if (q < 0 || q >= effect.Length - 1) return;

        string innerEffect = effect.Substring(q + 1).Trim();
        if (UnityEngine.Random.Range(0, 100) < chance)
        {
            ApplyEffects(innerEffect, saveData);
        }
    }
}

using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    public string Chapter;
    public string Day;
    public string NodeId;
    public string NodeType;     // Dialogue / Choice / END ...
    public string ChoiceGroup;  // ★ 동일 그룹 선택지 묶음 ID
    public string NextNodeId;

    public string TextEffect;   // "color=#66CCFF;shake" 등

    public string Conditions;
    public string Effects;
    public string ElseIfConditions;
    public string ElseIfEffects;
    public string ElseEffects;
    public string SkipPenalty;
    public string FlagTag;
    public string ChoiceStyle;
    public string Skipping;
    public string SavePointFlag;
}

[System.Serializable]
public class StoryLine
{
    public string Chapter;
    public string Day;
    public string NodeId;
    public string SpeakerId;
    public string Text;
    public string ChoiceText;           // 버튼 라벨
    public string ElseEffectsMessage;   // 선택지 옆 안내
    public string SkipPenaltyMessage;   // 선택지 옆 경고
    public string Describe;             // (보존용)
}

[System.Serializable]
public class Speaker
{
    public string SpeakerId;
    public string Name;
    public string Color; // "#RRGGBB"
}

using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    public string Chapter;
    public string Day;
    public string NodeId;
    public string NodeType;     // Dialogue / Choice / END ...
    public string ChoiceGroup;  // Choice 묶음 ID
    public string NextNodeId;

    public string TextEffect;

    public string Conditions;
    public string Effects;
    public string ElseIfConditions;
    public string ElseIfEffects;
    public string ElseEffects;
    public string SkipPenalty;
    public string FlagTag;

    public string ChoiceStyle;   // ← 선택 연출 프리셋
    public string ChoiceArgs;    // ← 선택 연출 파라미터 (key=value;..)
    public string ChoiceFlags;   // ← 선택 기믹 토글 (avoid;shakeOnFail;..)

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
    public string ElseEffectsMessage;   // 실패 메시지(선택지 옆/가까이)
    public string SkipPenaltyMessage;
    public string Describe;
}

[System.Serializable]
public class Speaker
{
    public string SpeakerId;
    public string Name;
    public string Color; // "#RRGGBB"
}

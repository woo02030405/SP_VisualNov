using System;
using System.Collections.Generic;

namespace VN.Dialogue
{
    [Serializable]
    public class DialogueNode
    {
        // ---- Story CSV ----
        public string Chapter;         // 챕터 ID (예: CH1)
        public string Scene;           // 씬 ID (예: SC1)
        public string NodeId;          // 고유 ID (예: N001, Choice_001_1, END_A …)
        public string SpeakerId;       // 화자 (Speakers.csv 매칭)
        public string Text;            // 대사 / 내레이션
        public string ChoiceText;      // 선택지 텍스트 (Story CSV에만 존재)

        // ---- Dialogue CSV ----
        public string Skipping;        // 읽음 여부 기록 (SkipFlag)
        public string TextEffect;      // 텍스트 연출 (예: shake, fade …)
        public string Conditions;      // 조건식
        public string Effects;         // 조건 충족 시 보상
        public string ElseIfConditions;// 대체 조건
        public string ElseIfEffects;   // 대체 보상
        public string ElseEffects;     // 조건 실패 시 패널티
        public string SkipPenalty;     // 선택 안 했을 때 벌칙
        public string FlagTag;         // 플래그/루트 태그
        public string ChoiceStyle;     // 선택지 연출 스타일 (비어있으면 Default 처리)

        // ---- 흐름 ----
        public string NextNodeId;      // Story CSV에서 지정하는 다음 노드 ID
        public string NodeType;       // Dialogue, Choice, End

        // ---- 파서에서 생성 ----
        public ChoiceOption GeneratedOption; // 단일 선택지용으로 임시 생성되는 옵션
        public List<ChoiceOption> Choices { get; set; } = new List<ChoiceOption>();         // 여러 선택지를 담는 리스트. DialogueParser에서 채워 넣음.
    }
}

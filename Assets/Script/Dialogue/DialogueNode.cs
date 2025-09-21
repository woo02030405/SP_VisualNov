using System.Collections.Generic;

namespace VN.Dialogue
{
    [System.Serializable]
    public class DialogueNode
    {
        public string NodeId;          // 노드 ID 
        public string SpeakerId;       // 화자 ID 
        public string Text;            // 실제 대사 텍스트 

        // ---- 캐릭터 관련 (Dialogue.csv) ----
        public string CharacterType;     // standing / bust / full
        public string Expression;        // happy / sad / angry / neutral
        public string Pose;              // stand / sit / battle
        public string CharacterPosition; // left / center / right
        public string CharacterEffect;   // fadein / fadeout / shake / flash

        // ---- 사운드/연출 (Dialogue.csv) ----
        public string Background;      // 배경 이미지
        public string BGM;             // 배경 음악
        public string VoiceId;         // 보이스 파일 ID
        public string SFX;             // 효과음
        public string Transition;      // 화면 전환 (fade, slide, crossfade)

        // ---- 선택지 관련 (Dialogue.csv) ----
        public List<ChoiceOption> Choices = new List<ChoiceOption>();
        public string ChoiceAnimType;  // 선택지 애니메이션 (Default, Slide, Glow…)

        // ---- 텍스트/플레이 제어 (Dialogue.csv) ----
        public string TextEffect;      // 텍스트 출력 효과 (typewriter, shake, color 등)
        public bool SkipFlag;          // 스킵 가능 여부

        // ---- 흐름 제어 (Story.csv/Dialogue.csv 공통) ----
        public string NextNodeId;      // 다음 노드 ID
    }
}

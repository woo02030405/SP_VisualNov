using System.Collections.Generic;

namespace VN.Dialogue
{
    [System.Serializable]
    public class DialogueNode
    {
        public string Id;        // NodeId
        public string Chapter;   // Chapter
        public string Scene;     // Scene
        public string SpeakerId;
        public string Text;
        public string NextNodeId;
        public List<ChoiceOption> Choices = new();

        public string SkipFlag = "False";
        public string TextEffect;

        public bool HasChoices => Choices != null && Choices.Count > 0;

        // 리소스 연동 필드
        public string Background;
        public string BGM;
        public string SFX;
        public string VoiceId;
        public string CharacterId;
        public string Expression;
        public string Pose;
        public string CharacterPosition;
        public string CharacterEffect;
        public string AnimationId;
        public string EventCG;
        public string Transition;
    }
}

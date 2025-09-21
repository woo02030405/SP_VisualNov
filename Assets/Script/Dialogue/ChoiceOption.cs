namespace VN.Dialogue
{
    [System.Serializable]
    public class ChoiceOption
    {
        public string Text;        // 선택지 문구
        public string NextNodeId;  // 선택 시 다음 노드

        public ChoiceOption(string text, string nextNodeId)
        {
            Text = text;
            NextNodeId = nextNodeId;
        }
    }
}

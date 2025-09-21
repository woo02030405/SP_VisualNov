namespace VN.Dialogue
{
    [System.Serializable]
    public class ChoiceOption
    {
        public string Text;        // ������ ����
        public string NextNodeId;  // ���� �� ���� ���

        public ChoiceOption(string text, string nextNodeId)
        {
            Text = text;
            NextNodeId = nextNodeId;
        }
    }
}

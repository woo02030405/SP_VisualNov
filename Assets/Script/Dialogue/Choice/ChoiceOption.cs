[System.Serializable]
public class ChoiceOption
{
    public string Text;         // ������ �ؽ�Ʈ
    public string NextNodeId;   // �̵��� ���
    public string Condition;    // ���� (������ "-")
    public string Effect;       // ���� �� �ߵ� ȿ��
    public string ElseEffect;   // ���� �� ���� �� �ߵ� ȿ��
}

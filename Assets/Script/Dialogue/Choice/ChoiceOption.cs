[System.Serializable]
public class ChoiceOption
{
    public string Text;         // 선택지 텍스트
    public string NextNodeId;   // 이동할 노드
    public string Condition;    // 조건 (없으면 "-")
    public string Effect;       // 선택 시 발동 효과
    public string ElseEffect;   // 선택 안 했을 때 발동 효과
}

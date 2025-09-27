public class ChoiceOption
{
    public string Text { get; private set; }
    public string NextNodeId { get; private set; }
    public string Conditions { get; private set; }
    public string ChoiceStyle { get; private set; }

    public ChoiceOption(string text, string nextNodeId, string conditions = "", string style = "Default")
    {
        Text = text;
        NextNodeId = nextNodeId;
        Conditions = conditions;
        ChoiceStyle = string.IsNullOrEmpty(style) ? "Default" : style;
    }
}

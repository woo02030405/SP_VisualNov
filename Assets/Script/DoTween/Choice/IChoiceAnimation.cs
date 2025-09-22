using UnityEngine;

public interface IChoiceAnimation
{
    void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected);
}

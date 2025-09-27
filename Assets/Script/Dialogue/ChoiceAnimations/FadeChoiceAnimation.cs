using UnityEngine;
using DG.Tweening;

public class FadeChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        var cg = selected.GetComponent<CanvasGroup>() ?? selected.AddComponent<CanvasGroup>();

        cg.DOFade(0f, 0.4f).OnComplete(() =>
        {
            GameObject.Destroy(selected);
            onSelected?.Invoke(next);
        });

        // 다른 버튼은 그냥 사라짐
        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
                GameObject.Destroy(sibling.gameObject);
        }
    }
}

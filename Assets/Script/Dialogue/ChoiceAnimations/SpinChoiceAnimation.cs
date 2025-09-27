using UnityEngine;
using DG.Tweening;

public class SpinChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        selected.transform.DORotate(new Vector3(0, 0, 720f), 0.6f, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad);
        selected.transform.DOScale(0f, 0.6f).OnComplete(() =>
        {
            GameObject.Destroy(selected);
            onSelected?.Invoke(next);
        });

        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
                sibling.transform.DOScale(0f, 0.5f).OnComplete(() => GameObject.Destroy(sibling.gameObject));
        }
    }
}

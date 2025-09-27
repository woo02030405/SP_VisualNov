using UnityEngine;
using DG.Tweening;

public class DropChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        selected.transform.DOLocalMoveY(-600f, 0.6f).SetRelative(true).SetEase(Ease.InBounce)
            .OnComplete(() =>
            {
                GameObject.Destroy(selected);
                onSelected?.Invoke(next);
            });

        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
                sibling.transform.DOLocalMoveY(600f, 0.5f).SetRelative(true).SetEase(Ease.InBack)
                    .OnComplete(() => GameObject.Destroy(sibling.gameObject));
        }
    }
}

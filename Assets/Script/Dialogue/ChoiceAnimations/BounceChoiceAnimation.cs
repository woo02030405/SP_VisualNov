using UnityEngine;
using DG.Tweening;

public class BounceChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        selected.transform.DOScale(1.3f, 0.25f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                GameObject.Destroy(selected);
                onSelected?.Invoke(next);
            });

        // 나머지 버튼은 작게 튀다 사라짐
        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
                sibling.transform.DOScale(0.7f, 0.3f).SetEase(Ease.InBack)
                    .OnComplete(() => GameObject.Destroy(sibling.gameObject));
        }
    }
}

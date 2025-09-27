using UnityEngine;
using DG.Tweening;

public class SlideChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        selected.transform.DOLocalMoveX(800, 0.5f).SetRelative(true).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                GameObject.Destroy(selected);
                onSelected?.Invoke(next);
            });

        // 나머지 버튼은 왼쪽으로 날아감
        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
                sibling.transform.DOLocalMoveX(-800, 0.5f).SetRelative(true).SetEase(Ease.InBack)
                    .OnComplete(() => GameObject.Destroy(sibling.gameObject));
        }
    }
}

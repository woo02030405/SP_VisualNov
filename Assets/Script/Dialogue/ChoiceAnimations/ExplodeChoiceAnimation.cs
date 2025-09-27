using UnityEngine;
using DG.Tweening;

public class ExplodeChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        // 선택된 버튼
        selected.transform.DOScale(2f, 0.3f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                GameObject.Destroy(selected);
                onSelected?.Invoke(next);
            });

        // 나머지 버튼
        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
            {
                Vector3 dir = Random.insideUnitCircle.normalized * 500f;
                sibling.transform.DOLocalMove(dir, 0.5f).SetEase(Ease.OutQuad)
                    .OnComplete(() => GameObject.Destroy(sibling.gameObject));
            }
        }
    }
}

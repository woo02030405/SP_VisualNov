using UnityEngine;
using DG.Tweening;

public class DefaultChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        var selectedCg = selected.GetComponent<CanvasGroup>() ?? selected.AddComponent<CanvasGroup>();

        var seq = DOTween.Sequence();

        // ���õ� ��ư ����
        seq.Append(selected.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));
        seq.Join(selected.transform.DOMove(choicePanel.position, 0.35f).SetEase(Ease.InOutCubic));
        seq.AppendInterval(0.7f); // ���� ���� �ð�
        seq.Append(selectedCg.DOFade(0f, 0.3f));

        // ������ ��ư ó��
        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
            {
                var cg = sibling.GetComponent<CanvasGroup>() ?? sibling.gameObject.AddComponent<CanvasGroup>();
                cg.interactable = false;

                DOTween.Sequence()
                    .Append(sibling.transform.DOScale(0.8f, 0.25f).SetEase(Ease.InCubic))
                    .Join(sibling.transform.DOMoveY(sibling.position.y - 80f, 0.3f).SetEase(Ease.InCubic))
                    .Join(cg.DOFade(0f, 0.3f))
                    .OnComplete(() => GameObject.Destroy(sibling.gameObject));
            }
        }

        // ���õ� ��ư ���� �� �ݹ� ����
        seq.OnComplete(() =>
        {
            GameObject.Destroy(selected);
            onSelected?.Invoke(next);
        });
    }
}

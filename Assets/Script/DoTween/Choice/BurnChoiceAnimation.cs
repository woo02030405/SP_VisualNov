using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class BurnChoiceAnimation : IChoiceAnimation
{
    private float delay;

    public BurnChoiceAnimation(float delay = 0.75f)
    {
        this.delay = delay;
    }

    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        var cg = selected.GetComponent<CanvasGroup>() ?? selected.AddComponent<CanvasGroup>();
        var img = selected.GetComponent<Image>();
        var txt = selected.GetComponentInChildren<TMP_Text>();

        Sequence seq = DOTween.Sequence();

        // 딜레이 후 불타는 연출
        seq.AppendInterval(delay);

        if (img) seq.Join(img.DOColor(new Color(1f, 0.2f, 0.1f, 1f), 0.4f));
        if (txt) seq.Join(txt.DOColor(new Color(1f, 0.8f, 0.2f, 1f), 0.4f));

        seq.Append(selected.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
        seq.Join(cg.DOFade(0f, 0.6f));
        seq.Join(selected.transform.DOMoveY(selected.transform.position.y + 50f, 0.6f).SetEase(Ease.OutCubic));

        // 🔹 끝나면 UI 삭제 (선택 불가)
        seq.OnComplete(() =>
        {
            Object.Destroy(selected);
            // 이건 자동 소멸이니까 onSelected는 호출 안 함
        });
    }
}

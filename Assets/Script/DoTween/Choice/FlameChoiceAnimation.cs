using UnityEngine;
using DG.Tweening;

public class FlameChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        var cg = selected.GetComponent<CanvasGroup>() ?? selected.AddComponent<CanvasGroup>();

        // 선택된 버튼을 강조
        selected.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack);

        // 🔥 붉게 변하면서 점점 사라지는 효과
        var img = selected.GetComponent<UnityEngine.UI.Image>();
        var txt = selected.GetComponentInChildren<TMPro.TMP_Text>();

        Sequence seq = DOTween.Sequence();

        if (img != null)
        {
            seq.Join(img.DOColor(new Color(1f, 0.3f, 0.1f, 1f), 0.3f));
        }
        if (txt != null)
        {
            seq.Join(txt.DOColor(new Color(1f, 0.8f, 0.2f, 1f), 0.3f));
        }

        // 알파값 줄이면서 위로 살짝 이동 후 제거
        seq.Append(cg.DOFade(0f, 0.6f));
        seq.Join(selected.transform.DOMoveY(selected.transform.position.y + 60f, 0.6f).SetEase(Ease.OutCubic));

        // 끝나면 선택된 next 노드 실행
        seq.OnComplete(() =>
        {
            onSelected?.Invoke(next);
        });
    }
}

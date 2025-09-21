using DG.Tweening;
using UnityEngine;

public class ChoicePanelIntro : MonoBehaviour
{
    [SerializeField] CanvasGroup panelCg; // ChoicePanel에 붙일 CanvasGroup

    public Tween PlayIn(float dur = 0.3f)
    {
        if (!panelCg) panelCg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        // 🔹 시작부터 입력 가능하게 (막지 않음)
        panelCg.interactable = true;
        panelCg.blocksRaycasts = true;
        panelCg.alpha = 0f;

        var seq = DOTween.Sequence().Append(panelCg.DOFade(1f, dur));

        // 자식 버튼 순차 등장
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i) as RectTransform;
            var cg = t.GetComponent<CanvasGroup>() ?? t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            t.localScale = Vector3.one * 0.92f;

            seq.Append(cg.DOFade(1f, 0.15f))
               .Join(t.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        }

        return seq; // 🔹 OnComplete로 굳이 interactable 켜줄 필요 없음
    }
}

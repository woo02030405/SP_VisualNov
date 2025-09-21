using DG.Tweening;
using UnityEngine;

public class DialogueBoxIntro : MonoBehaviour
{
    [SerializeField] RectTransform box; // DialogueBox
    [SerializeField] CanvasGroup cg;    // DialogueBoxø° ∫Ÿ¿œ CanvasGroup

    void Awake()
    {
        if (!cg) cg = box.GetComponent<CanvasGroup>();
        if (!cg) cg = box.gameObject.AddComponent<CanvasGroup>();
    }

    public Tween PlayIn(float dur = 0.45f)
    {
        var startY = box.anchoredPosition.y;
        box.anchoredPosition = new Vector2(box.anchoredPosition.x, startY - 140f);
        cg.alpha = 0f;

        return DOTween.Sequence()
            .Join(box.DOAnchorPosY(startY, dur).SetEase(Ease.OutCubic))
            .Join(cg.DOFade(1f, dur * 0.9f));
    }

    public Tween PlayOut(float dur = 0.35f)
    {
        return DOTween.Sequence()
            .Join(box.DOAnchorPosY(box.anchoredPosition.y - 120f, dur).SetEase(Ease.InCubic))
            .Join(cg.DOFade(0f, dur));
    }
}

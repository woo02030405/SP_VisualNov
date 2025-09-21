using DG.Tweening;
using UnityEngine;

public class NextBlink : MonoBehaviour
{
    [SerializeField] CanvasGroup cg; // NextIndicatorø° ∫Ÿ¿œ CanvasGroup

    void OnEnable()
    {
        if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.DOFade(1f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void OnDisable()
    {
        DOTween.Kill(cg);
    }
}

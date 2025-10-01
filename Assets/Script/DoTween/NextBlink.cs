using DG.Tweening;
using UnityEngine;

public class NextBlink : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    private Tween blinkTween;

    [Header("Blink Settings")]
    public float fadeDuration = 0.6f;   // 깜박이는 속도 (Inspector에서 조절)
    public Ease blinkEase = Ease.InOutSine;

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
    }

    public void StartBlink()
    {
        StopBlink();
        blinkTween = cg.DOFade(1f, fadeDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(blinkEase);
    }

    public void StopBlink()
    {
        if (blinkTween != null && blinkTween.IsActive())
        {
            blinkTween.Kill();
            blinkTween = null;
        }
        cg.alpha = 0f;
    }
}

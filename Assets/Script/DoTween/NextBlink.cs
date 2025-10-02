using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NextBlink : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Image indicatorImage;   
    private Tween blinkTween;

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite choiceSprite;
    [SerializeField] private Sprite endSprite;   

    [Header("Blink Settings")]
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private Ease blinkEase = Ease.InOutSine;

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (!indicatorImage) indicatorImage = GetComponent<Image>();
        cg.alpha = 0f;
    }

    // ===== 모드 전환 =====
    public void SetNormal()
    {
        if (indicatorImage) indicatorImage.sprite = normalSprite;
    }

    public void SetChoice()
    {
        if (indicatorImage) indicatorImage.sprite = choiceSprite;
    }

    public void SetEnd()
    {
        if (indicatorImage) indicatorImage.sprite = endSprite;
    }

    // ===== 애니메이션 =====
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

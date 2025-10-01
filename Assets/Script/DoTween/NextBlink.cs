using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NextBlink : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    [SerializeField] Image indicatorImage;   // 🔹 깜빡일 이미지
    private Tween blinkTween;

    [Header("Blink Settings")]
    public float fadeDuration = 0.6f;
    public Ease blinkEase = Ease.InOutSine;
    public Sprite normalSprite;
    public Sprite choiceSprite;

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (!indicatorImage) indicatorImage = GetComponent<Image>();
        cg.alpha = 0f;
    }

    public void SetMode(bool isChoice)
    {
        if (!indicatorImage) return;
        indicatorImage.sprite = isChoice ? choiceSprite : normalSprite;
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

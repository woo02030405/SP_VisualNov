using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// NextIndicator¿ë DOTween ÄÁÆ®·Ñ·¯ (±ôºýÀÓ + µÕµÕ)
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class NextIndicatorDOTween : MonoBehaviour
{
    [Header("Fade (Blink)")]
    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    [Min(0.05f)] public float fadeDuration = 0.6f;

    [Header("Bob (Up/Down)")]
    public float bobOffsetY = 10f;
    [Min(0.05f)] public float bobDuration = 1.2f;

    [Header("Common")]
    public bool playOnEnable = true;
    public bool ignoreTimeScale = true; // ÀÏ½ÃÁ¤Áö Áß¿¡µµ µ¿ÀÛÇÏ·Á¸é true

    private RectTransform rt;
    private CanvasGroup cg;
    private Sequence seq;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (playOnEnable) Play();
    }

    void OnDisable()
    {
        KillSeq();
    }

    public void Play()
    {
        KillSeq();

        var basePos = rt.anchoredPosition;

        // Fade Yoyo
        cg.alpha = maxAlpha;

        seq = DOTween.Sequence();
        seq.SetUpdate(ignoreTimeScale);

        // ±ôºýÀÓ
        seq.Join(cg.DOFade(minAlpha, fadeDuration).SetLoops(-1, LoopType.Yoyo));

        // µÕµÕ
        seq.Join(rt.DOAnchorPosY(basePos.y + bobOffsetY, bobDuration)
                  .SetLoops(-1, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine));

        seq.Play();
    }

    public void Stop()
    {
        KillSeq();
        // ¿ø»óº¹±¸
        cg.alpha = maxAlpha;
    }

    private void KillSeq()
    {
        if (seq != null && seq.IsActive())
        {
            seq.Kill();
            seq = null;
        }
    }

    // ¿ÜºÎ Á¦¾î¿ë (¿¹: Å¸ÀÌÇÎ Áß ¼û±è / ³¡³ª¸é º¸ÀÓ)
    public void Show(bool show)
    {
        gameObject.SetActive(show);
        if (show && playOnEnable) Play();
    }
}

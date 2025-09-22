using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if DOTWEEN
using DG.Tweening;
#endif

/// 씬에 1개 배치: 마우스 근처에 표시되는 UI 툴팁
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("Refs")]
    public TMP_Text text;
    public RectTransform bubble;   // 배경 패널(이미지 붙은)
    public Canvas rootCanvas;      // Overlay Canvas

    [Header("Layout")]
    public Vector2 padding = new(12, 8); // 텍스트 바깥 여백
    public Vector2 cursorOffset = new(18, -18); // 커서 기준 오프셋
    public float maxWidth = 360f;

    [Header("Anim")]
    public float fadeTime = 0.12f;
    public bool useUnscaledTime = true;

    CanvasGroup cg;
    RectTransform rt;
    bool visible;
    string current;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        HideInstant();

        if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            Debug.LogWarning("[TooltipUI] 권장: Screen Space - Overlay");
    }

    void LateUpdate()
    {
        if (!visible) return;

        // 커서 위치를 캔버스 좌표로 변환하고, 화면 밖으로 나가지 않도록 클램프
        Vector2 screen = Input.mousePosition;
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform, screen, rootCanvas.worldCamera, out canvasPos);

        Vector2 desired = canvasPos + cursorOffset;

        // 말풍선 크기 구해서 화면 경계 클램프
        var canvasRect = rootCanvas.transform as RectTransform;
        Vector2 half = bubble.sizeDelta * 0.5f;
        float left = -canvasRect.rect.width * 0.5f + half.x + 6f;
        float right = canvasRect.rect.width * 0.5f - half.x - 6f;
        float bottom = -canvasRect.rect.height * 0.5f + half.y + 6f;
        float top = canvasRect.rect.height * 0.5f - half.y - 6f;

        desired.x = Mathf.Clamp(desired.x, left, right);
        desired.y = Mathf.Clamp(desired.y, bottom, top);
        rt.anchoredPosition = desired;
    }

    public void Show(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;
        current = msg;

        // 텍스트 세팅 및 자동 사이즈
        text.enableWordWrapping = true;
        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        text.text = msg;
        LayoutRebuilder.ForceRebuildLayoutImmediate(text.rectTransform);

        Vector2 content = new(
            Mathf.Min(text.preferredWidth, maxWidth),
            text.preferredHeight);

        bubble.sizeDelta = content + padding * 2f;

        // 페이드 인
#if DOTWEEN
        cg.DOKill();
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;
        cg.DOFade(1f, fadeTime).SetUpdate(useUnscaledTime);
#else
        cg.alpha = 1f;
#endif
        cg.interactable = false; cg.blocksRaycasts = false; // 클릭 막지 않음
        visible = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (!visible) return;
        visible = false;
#if DOTWEEN
        cg.DOKill();
        cg.DOFade(0f, fadeTime).SetUpdate(useUnscaledTime)
          .OnComplete(() => { gameObject.SetActive(false); });
#else
        cg.alpha = 0f;
        gameObject.SetActive(false);
#endif
    }

    public void HideInstant()
    {
        visible = false;
#if DOTWEEN
        cg.DOKill();
#endif
        cg.alpha = 0f;
        gameObject.SetActive(false);
    }
}

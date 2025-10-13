using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class QuickSaveQuickLoadUI : MonoBehaviour
{
    [Header("Settings")]
    public int quickSlotIndex = 0;
    [Tooltip("로딩창 최소 표시 시간(초)")]
    public float minLoadingSeconds = 1f;

    [Header("Style")]
    public int overlaySortingOrder = 100000; // 최상단 확정

    // internal
    Canvas overlayCanvas;
    RectTransform root;
    // Confirm
    CanvasGroup confirmGroup;
    TMP_Text confirmLabel;
    Button confirmYes;
    Button confirmNo;
    Action _pendingYes;
    // Loading
    CanvasGroup loadingGroup;
    TMP_Text loadingLabel;

    void Awake()
    {
        EnsureEventSystem();
        BuildOverlayCanvas();
        BuildConfirm();
        BuildLoading();

        HideConfirm();
        HideLoading();
        Debug.Log("[QS/UI] Minimal overlay ready.");
    }

    // === Public entry points (버튼에 이거만 연결) ===
    public void OnClickQuickSaveButton()
    {
        ShowConfirm("퀵세이브 하시겠습니까?", () => StartCoroutine(SaveFlow()));
    }

    public void OnClickQuickLoadButton()
    {
        ShowConfirm("퀵로드 하시겠습니까?", () => StartCoroutine(LoadFlow()));
    }

    // === Flows ===
    IEnumerator SaveFlow()
    {
        float shownAt = Time.realtimeSinceStartup;
        ShowLoading("Saving...");
        yield return null;

        bool didWork = TryCallSaveManager("Save", quickSlotIndex);
        if (!didWork) Debug.LogWarning("[QS/UI] SaveManager를 찾지 못했습니다. (UI 동작만 확인)");

        float remain = minLoadingSeconds - (Time.realtimeSinceStartup - shownAt);
        if (remain > 0) yield return new WaitForSecondsRealtime(remain);
        HideLoading();
    }

    IEnumerator LoadFlow()
    {
        float shownAt = Time.realtimeSinceStartup;
        ShowLoading("Loading...");
        yield return null;

        bool didWork = TryCallSaveManager("Load", quickSlotIndex);
        if (!didWork) Debug.LogWarning("[QS/UI] SaveManager를 찾지 못했습니다. (UI 동작만 확인)");

        float remain = minLoadingSeconds - (Time.realtimeSinceStartup - shownAt);
        if (remain > 0) yield return new WaitForSecondsRealtime(remain);
        HideLoading();
    }

    // === Confirm ===
    void ShowConfirm(string message, Action onYes)
    {
        _pendingYes = onYes;
        confirmLabel.text = message ?? "Are you sure?";
        SetGroup(confirmGroup, true);
    }
    void HideConfirm() => SetGroup(confirmGroup, false);

    // === Loading ===
    void ShowLoading(string message)
    {
        loadingLabel.text = message ?? "Loading...";
        SetGroup(loadingGroup, true);
    }
    void HideLoading() => SetGroup(loadingGroup, false);

    // === Builders ===
    void EnsureEventSystem()
    {
        if (!FindObjectOfType<EventSystem>())
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

    void BuildOverlayCanvas()
    {
        var go = new GameObject("__QS_Overlay__", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(go);
        overlayCanvas = go.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = overlaySortingOrder;
        go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        root = new GameObject("Root", typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(go.transform, false);
        Stretch(root);
    }

    void BuildConfirm()
    {
        var cg = NewOverlay("Confirm", out var panel);
        confirmGroup = cg;

        // Label
        confirmLabel = NewText(panel, "확인하시겠습니까?", 30, new Vector2(0, 40), new Vector2(540, 120));
        // Buttons
        confirmYes = NewButton(panel, "확인", new Vector2(-110, -60), 180, 56, OnClickConfirmYes);
        confirmNo = NewButton(panel, "취소", new Vector2(110, -60), 180, 56, OnClickConfirmNo);
    }

    void BuildLoading()
    {
        var cg = NewOverlay("Loading", out var panel);
        loadingGroup = cg;

        loadingLabel = NewText(panel, "Loading...", 30, new Vector2(0, 0), new Vector2(540, 80));
    }

    CanvasGroup NewOverlay(string name, out RectTransform panelRT)
    {
        // Dark BG
        var wrap = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        var wrapRT = wrap.GetComponent<RectTransform>();
        wrapRT.SetParent(root, false);
        Stretch(wrapRT);

        var cg = wrap.GetComponent<CanvasGroup>();
        cg.alpha = 0; cg.interactable = false; cg.blocksRaycasts = false;

        var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.SetParent(wrapRT, false);
        Stretch(bgRT);
        var bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.6f);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelRT = panel.GetComponent<RectTransform>();
        panelRT.SetParent(wrapRT, false);
        panelRT.sizeDelta = new Vector2(640, 240);
        var pimg = panel.GetComponent<Image>();
        pimg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        return cg;
    }

    TMP_Text NewText(RectTransform parent, string text, int fontSize, Vector2 anchored, Vector2 size)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchored;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        return tmp;
    }

    Button NewButton(RectTransform parent, string label, Vector2 anchored, float w, float h, Action onClick)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = anchored;

        var img = go.GetComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        var text = NewText(rt, label, 28, Vector2.zero, new Vector2(w - 20, h - 16));
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());
        return btn;
    }

    // === Handlers ===
    void OnClickConfirmYes()
    {
        HideConfirm();
        _pendingYes?.Invoke();
        _pendingYes = null;
    }

    void OnClickConfirmNo()
    {
        HideConfirm();
        _pendingYes = null;
    }

    // === Utils ===
    void SetGroup(CanvasGroup cg, bool on)
    {
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = Vector3.one;
    }

    bool TryCallSaveManager(string methodName, int slot)
    {
        // SaveManager.Instance?.Save/Load(int) 를 리플렉션으로 시도
        var asmTypes = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in asmTypes)
        {
            var t = asm.GetType("VN.SaveSystem.SaveManager") ?? asm.GetType("SaveManager");
            if (t == null) continue;

            var instProp = t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            var inst = instProp?.GetValue(null);
            if (inst == null) continue;

            var m = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
            if (m == null) continue;

            try { m.Invoke(inst, new object[] { slot }); return true; }
            catch (Exception e) { Debug.LogWarning($"[QS/UI] {methodName} 실패: {e.Message}"); return false; }
        }
        return false;
    }
}

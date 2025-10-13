using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using VN.SaveSystem;

[DisallowMultipleComponent]
public class QuickSaveLoadManager : MonoBehaviour
{
    [Header("Slot & Behavior")]
    public int quickSlotIndex = 0;
    public float minLoadingSeconds = 1f;

    [Header("(Optional) Prefabs")]
    public GameObject confirmPopupPrefab;
    public GameObject loadingOverlayPrefab;

    [Header("(Optional) Parent Canvas")]
    public Canvas parentCanvas;

    [Header("Search Keys in Prefabs")]
    public string confirmYesButtonName = "Yes";
    public string confirmNoButtonName = "No";
    public string loadingTextNameContains = "Text";

    // runtime
    bool _inited;
    GameObject _confirmGO, _loadingGO;
    CanvasGroup _confirmGroup, _loadingGroup;
    TMP_Text _confirmLabel, _loadingLabel;
    Button _confirmYesBtn, _confirmNoBtn;
    Action _pendingYes;

    void Awake() => InitOnce();


    void InitOnce()
    {
        if (_inited) return;

        EnsureEventSystem();
        EnsureParentCanvas();

        BuildOrBindConfirm();
        BuildOrBindLoading();

        SetGroup(_confirmGroup, false);
        SetGroup(_loadingGroup, false);

        WireConfirmButtons();

        // SaveManager 콜백 묶기
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnBuildSaveData = BuildSaveData;
            SaveManager.Instance.OnApplySaveData = ApplySaveData;
        }

        // RSBM 자동 생성 (없으면 새로 만든다)
        if (ReadSkipBacklogManager.Instance == null)
            new GameObject("ReadSkipBacklog", typeof(ReadSkipBacklogManager));


        _inited = true;
        Debug.Log("[QSLM] Init complete.");
    }

    // ========== Public OnClick ==========
    public void OnClickQuickSave()
    {
        InitOnce();
        ShowConfirm("퀵세이브 하시겠습니까?", () => StartCoroutine(SaveFlow()));
    }

    public void OnClickQuickLoad()
    {
        InitOnce();
        ShowConfirm("퀵로드 하시겠습니까?", () => StartCoroutine(LoadFlow()));
    }

    // ========== Flows ==========
    IEnumerator SaveFlow()
    {
        float shownAt = Time.realtimeSinceStartup;
        ShowLoading("Saving...");
        yield return null;

        if (SaveManager.Instance != null) SaveManager.Instance.Save(quickSlotIndex);
        else Debug.LogWarning("[QSLM] SaveManager.Instance is null");

        float remain = minLoadingSeconds - (Time.realtimeSinceStartup - shownAt);
        if (remain > 0f) yield return new WaitForSecondsRealtime(remain);
        HideLoading();
    }

    IEnumerator LoadFlow()
    {
        float shownAt = Time.realtimeSinceStartup;
        ShowLoading("Loading...");
        yield return null;

        if (SaveManager.Instance != null) SaveManager.Instance.Load(quickSlotIndex);
        else Debug.LogWarning("[QSLM] SaveManager.Instance is null");

        float remain = minLoadingSeconds - (Time.realtimeSinceStartup - shownAt);
        if (remain > 0f) yield return new WaitForSecondsRealtime(remain);
        HideLoading();
    }

    // ========== SaveData Build / Apply ==========
    SaveData BuildSaveData()
    {
        var d = new SaveData();

        // 스토리 위치 (프로젝트 API에 맞게 자동 탐색)
        d.story.chapter = TryGetString("DialogueManager", "CurrentChapter") ?? d.story.chapter;
        d.story.scene = TryGetString("DialogueManager", "CurrentScene") ?? d.story.scene;
        d.story.nodeId = TryGetString("DialogueManager", "CurrentNodeId") ?? d.story.nodeId;

        // CSV: 경로(Resources 키) 우선 저장, 없으면 라인 덤프
        var csvPath = TryGetCurrentCsvPath();
        if (!string.IsNullOrEmpty(csvPath)) d.story.csvPath = csvPath;
        else
        {
            var rows = TryExportCsvRows();
            if (rows != null && rows.Length > 0) d.story.csvRows = new List<string>(rows);
        }

        // 인벤토리/호감도
        var inv = TryExportDict("InventoryManager", "Export", "ToPairs");
        if (inv != null) d.player.inventory = inv;

        var aff = TryExportDict("AffinityManager", "Export", "ToPairs");
        if (aff != null) d.player.affinity = aff;

        // 월드 상태(있으면)
        var day = TryGetInt("WorldManager", "Day"); if (day.HasValue) d.world.day = day.Value;
        var timeSlot = TryGetString("WorldManager", "Time"); if (!string.IsNullOrEmpty(timeSlot)) d.world.timeSlot = timeSlot;
        var curMap = TryGetString("WorldManager", "Map"); if (!string.IsNullOrEmpty(curMap)) d.world.currentMap = curMap;

        // 메타
        d.title = $"DAY{d.world.day:D2} - {d.world.timeSlot}";
        d.dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        d.system.saveSlot = quickSlotIndex;
        d.system.timestamp = d.dateTime;

        // 1줄: 읽음/백로그를 SaveData.story에 채워넣기
        ReadSkipBacklogManager.Instance?.ExportToSave(d.story);


        return d;
    }

    void ApplySaveData(SaveData d)
    {
        if (d == null) return;

        // ⬇⬇⬇ 여기 1줄: 저장돼 있던 읽음/백로그를 런타임 메모리로 복원
        ReadSkipBacklogManager.Instance?.ImportFromSave(d.story);

        // CSV 복원
        if (!string.IsNullOrEmpty(d.story.csvPath)) TryLoadCsvByPath(d.story.csvPath);
        else if (d.story.csvRows != null && d.story.csvRows.Count > 0) TryLoadCsvByRows(d.story.csvRows);

        // 인벤토리/호감도 복원
        if (d.player.inventory != null) TryImportDict("InventoryManager", "Import", "FromPairs", d.player.inventory);
        if (d.player.affinity != null) TryImportDict("AffinityManager", "Import", "FromPairs", d.player.affinity);

        // 노드 점프(실패 시 후보)
        if (!string.IsNullOrEmpty(d.story.nodeId))
        {
            if (!TryJumpToNode(d.story.nodeId))
            {
                foreach (var cand in BuildNodeIdCandidates(d.story.chapter, d.story.scene, d.story.nodeId))
                    if (TryJumpToNode(cand)) break;
            }
        }

        // 월드 상태
        TrySetInt("WorldManager", "Day", d.world.day);
        TrySetString("WorldManager", "Time", d.world.timeSlot);
        TrySetString("WorldManager", "Map", d.world.currentMap);
    }

    IEnumerable<string> BuildNodeIdCandidates(string chapter, string scene, string nodeId)
    {
        if (!string.IsNullOrEmpty(nodeId)) yield return nodeId;
        if (!string.IsNullOrEmpty(chapter) && !string.IsNullOrEmpty(scene) && !string.IsNullOrEmpty(nodeId))
            yield return $"{chapter}/{scene}/{nodeId}";
        if (!string.IsNullOrEmpty(scene) && !string.IsNullOrEmpty(nodeId))
            yield return $"{scene}/{nodeId}";
    }

    // ========== UI (Confirm / Loading) ==========
    void ShowConfirm(string message, Action onYes)
    {
        _pendingYes = onYes;
        if (_confirmLabel) _confirmLabel.text = string.IsNullOrEmpty(message) ? "Are you sure?" : message;
        ForceActivateHierarchy(_confirmGroup?.transform, true);
        RaiseToTop(_confirmGO, 90000);
        SetGroup(_confirmGroup, true);
    }
    void HideConfirm() => SetGroup(_confirmGroup, false);

    void ShowLoading(string message)
    {
        if (_loadingLabel) _loadingLabel.text = string.IsNullOrEmpty(message) ? "Loading..." : message;
        ForceActivateHierarchy(_loadingGroup?.transform, true);
        RaiseToTop(_loadingGO, 90000);
        SetGroup(_loadingGroup, true);
    }
    void HideLoading() => SetGroup(_loadingGroup, false);

    void BuildOrBindConfirm()
    {
        if (confirmPopupPrefab)
        {
            _confirmGO = Instantiate(confirmPopupPrefab, parentCanvas.transform);
            _confirmGO.SetActive(true);
            ForceRectStretch(_confirmGO.transform as RectTransform);
            EnsureOverlayTopCanvas(_confirmGO, 90000);

            _confirmGroup = FindOrAdd<CanvasGroup>(_confirmGO);
            _confirmLabel = FindFirst<TextMeshProUGUI>(_confirmGO);
            _confirmYesBtn = FindButtonByName(_confirmGO.transform, confirmYesButtonName);
            _confirmNoBtn = FindButtonByName(_confirmGO.transform, confirmNoButtonName);
            if (_confirmYesBtn == null || _confirmNoBtn == null) BuildDefaultConfirmButtons(_confirmGO.transform);
        }
        else
        {
            BuildRuntimeConfirm();
        }
    }

    void BuildOrBindLoading()
    {
        if (loadingOverlayPrefab)
        {
            _loadingGO = Instantiate(loadingOverlayPrefab, parentCanvas.transform);
            _loadingGO.SetActive(true);
            ForceRectStretch(_loadingGO.transform as RectTransform);
            EnsureOverlayTopCanvas(_loadingGO, 90000);

            _loadingGroup = FindOrAdd<CanvasGroup>(_loadingGO);
            _loadingLabel = FindNameContains<TextMeshProUGUI>(_loadingGO, loadingTextNameContains) ?? FindFirst<TextMeshProUGUI>(_loadingGO);
        }
        else
        {
            BuildRuntimeLoading();
        }
    }

    void WireConfirmButtons()
    {
        if (_confirmYesBtn)
        {
            _confirmYesBtn.onClick.RemoveAllListeners();
            _confirmYesBtn.onClick.AddListener(() =>
            {
                HideConfirm();
                _pendingYes?.Invoke();
                _pendingYes = null;
            });
        }
        if (_confirmNoBtn)
        {
            _confirmNoBtn.onClick.RemoveAllListeners();
            _confirmNoBtn.onClick.AddListener(() =>
            {
                HideConfirm();
                _pendingYes = null;
            });
        }
    }

    void BuildRuntimeConfirm()
    {
        _confirmGO = new GameObject("__Confirm__", typeof(RectTransform), typeof(CanvasGroup));
        _confirmGO.transform.SetParent(parentCanvas.transform, false);
        ForceRectStretch(_confirmGO.transform as RectTransform);
        EnsureOverlayTopCanvas(_confirmGO, 90000);

        _confirmGroup = _confirmGO.GetComponent<CanvasGroup>();
        _confirmGroup.alpha = 0; _confirmGroup.interactable = false; _confirmGroup.blocksRaycasts = false;

        var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(_confirmGO.transform, false);
        ForceRectStretch(bg.transform as RectTransform);
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        var prt = panel.GetComponent<RectTransform>();
        panel.transform.SetParent(_confirmGO.transform, false);
        prt.sizeDelta = new Vector2(640, 240);
        panel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        _confirmLabel = NewTMP(panel.transform, "확인하시겠습니까?", 30, new Vector2(0, 40), new Vector2(560, 100));
        _confirmYesBtn = NewBtn(panel.transform, "확인", new Vector2(-110, -60), 180, 56);
        _confirmNoBtn = NewBtn(panel.transform, "취소", new Vector2(110, -60), 180, 56);

        WireConfirmButtons();
    }

    void BuildRuntimeLoading()
    {
        _loadingGO = new GameObject("__Loading__", typeof(RectTransform), typeof(CanvasGroup));
        _loadingGO.transform.SetParent(parentCanvas.transform, false);
        ForceRectStretch(_loadingGO.transform as RectTransform);
        EnsureOverlayTopCanvas(_loadingGO, 90000);

        _loadingGroup = _loadingGO.GetComponent<CanvasGroup>();
        _loadingGroup.alpha = 0; _loadingGroup.interactable = false; _loadingGroup.blocksRaycasts = false;

        var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(_loadingGO.transform, false);
        ForceRectStretch(bg.transform as RectTransform);
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        var prt = panel.GetComponent<RectTransform>();
        panel.transform.SetParent(_loadingGO.transform, false);
        prt.sizeDelta = new Vector2(640, 180);
        panel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        _loadingLabel = NewTMP(panel.transform, "Loading...", 30, new Vector2(0, 0), new Vector2(560, 80));
    }

    void BuildDefaultConfirmButtons(Transform panel)
    {
        if (_confirmLabel == null)
            _confirmLabel = NewTMP(panel, "확인하시겠습니까?", 30, new Vector2(0, 40), new Vector2(560, 100));

        if (_confirmYesBtn == null)
            _confirmYesBtn = NewBtn(panel, "확인", new Vector2(-110, -60), 180, 56);
        if (_confirmNoBtn == null)
            _confirmNoBtn = NewBtn(panel, "취소", new Vector2(110, -60), 180, 56);

        WireConfirmButtons();
    }

    // ========== CSV & Manager access (reflection) ==========
    string TryGetCurrentCsvPath()
    {
        var s = TryGetString("DialogueManager", "CurrentCsvPath");
        if (!string.IsNullOrEmpty(s)) return NormalizeCSVResourcePath(s);

        var m = TryCall<string>("DialogueManager", "GetCurrentCsvPath");
        if (!string.IsNullOrEmpty(m)) return NormalizeCSVResourcePath(m);
        return null;
    }

    string[] TryExportCsvRows() => TryCall<string[]>("DialogueManager", "ExportCsvRows");

    void TryLoadCsvByPath(string csvResKey)
    {
        if (TryCallVoid("DialogueManager", "LoadCsvByPath", csvResKey)) return;

        var ta = Resources.Load<TextAsset>(csvResKey);
        if (ta)
        {
            var rows = ta.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            TryLoadCsvByRows(rows);
        }
    }

    void TryLoadCsvByRows(IList<string> rows)
    {
        if (rows == null || rows.Count == 0) return;
        TryCallVoid("DialogueManager", "LoadCsvRows", rows is string[] sa ? sa : new List<string>(rows).ToArray());
    }

    string NormalizeCSVResourcePath(string anyPath)
    {
        if (string.IsNullOrEmpty(anyPath)) return null;
        anyPath = anyPath.Replace("\\", "/");
        var idx = anyPath.IndexOf("Resources/", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0) anyPath = anyPath[(idx + "Resources/".Length)..];
        if (anyPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            anyPath = anyPath[..^4];
        return anyPath; // ex) "CSV/Ch1_MainStory_kr"
    }

    Dictionary<string, int> TryExportDict(string managerType, string exportMethod, string pairsMethod)
    {
        var dict = TryCall<Dictionary<string, int>>(managerType, exportMethod);
        if (dict != null) return dict;

        var pairs = TryCall<System.Collections.IEnumerable>(managerType, pairsMethod);
        if (pairs != null)
        {
            var d = new Dictionary<string, int>();
            foreach (var it in pairs)
            {
                var t = it.GetType();
                var k = t.GetProperty("Key")?.GetValue(it) as string;
                var v = (int)(t.GetProperty("Value")?.GetValue(it) ?? 0);
                if (!string.IsNullOrEmpty(k)) d[k] = v;
            }
            return d;
        }
        return null;
    }

    void TryImportDict(string managerType, string importMethod, string fromPairsMethod, Dictionary<string, int> dict)
    {
        if (dict == null) return;
        if (TryCallVoid(managerType, importMethod, dict)) return;

        var listType = typeof(List<>).MakeGenericType(typeof(KeyValuePair<string, int>));
        var list = Activator.CreateInstance(listType);
        var add = listType.GetMethod("Add");
        foreach (var kv in dict) add.Invoke(list, new object[] { new KeyValuePair<string, int>(kv.Key, kv.Value) });

        TryCallVoid(managerType, fromPairsMethod, list);
    }

    bool TryJumpToNode(string nodeId)
        => TryCallVoid("DialogueManager", "JumpToNode", nodeId) || TryCallVoid("DialogueManager", "SetNode", nodeId);

    // ========== Reflection helpers ==========
    string TryGetString(string typeName, string propOrMethod)
    {
        var inst = FindSingleton(typeName);
        if (!inst) return null;

        var t = inst.GetType();
        var p = t.GetProperty(propOrMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(string)) return p.GetValue(inst) as string;

        var m = t.GetMethod("Get" + propOrMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (m != null && m.ReturnType == typeof(string)) return (string)m.Invoke(inst, null);
        return null;
    }

    int? TryGetInt(string typeName, string propOrMethod)
    {
        var inst = FindSingleton(typeName);
        if (!inst) return null;

        var t = inst.GetType();
        var p = t.GetProperty(propOrMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(inst);

        var m = t.GetMethod("Get" + propOrMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (m != null && m.ReturnType == typeof(int)) return (int)m.Invoke(inst, null);
        return null;
    }

    void TrySetString(string typeName, string prop, string value)
    {
        var inst = FindSingleton(typeName);
        if (!inst) return;

        var t = inst.GetType();
        var p = t.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite && p.PropertyType == typeof(string)) { p.SetValue(inst, value); return; }

        var m = t.GetMethod("Set" + prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
        if (m != null) m.Invoke(inst, new object[] { value });
    }

    void TrySetInt(string typeName, string prop, int value)
    {
        var inst = FindSingleton(typeName);
        if (!inst) return;

        var t = inst.GetType();
        var p = t.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite && p.PropertyType == typeof(int)) { p.SetValue(inst, value); return; }

        var m = t.GetMethod("Set" + prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
        if (m != null) m.Invoke(inst, new object[] { value });
    }

    object TryCall(string typeName, string method, Type returnType, params object[] args)
    {
        var inst = FindSingleton(typeName);
        if (!inst) return null;
        var t = inst.GetType();
        var argTypes = Array.ConvertAll(args ?? Array.Empty<object>(), a => a?.GetType() ?? typeof(object));
        var m = t.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, argTypes, null);
        if (m == null) return null;
        return m.Invoke(inst, args);
    }

    bool TryCallVoid(string typeName, string method, params object[] args)
        => TryCall(typeName, method, typeof(void), args) != null;

    T TryCall<T>(string typeName, string method)
    {
        var inst = FindSingleton(typeName);
        if (!inst) return default;
        var t = inst.GetType();
        var m = t.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (m != null && typeof(T).IsAssignableFrom(m.ReturnType)) return (T)m.Invoke(inst, null);
        return default;
    }

    Component FindSingleton(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(typeName);
            if (t == null) continue;
            var inst = FindObjectOfType(t, includeInactive: true) as Component;
            if (inst) return inst;
        }
        return null;
    }

    // ========== UI Utils (여기 포함! FindButtonByName 정의됨) ==========
    void EnsureEventSystem()
    {
        if (!FindObjectOfType<EventSystem>())
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

    void EnsureParentCanvas()
    {
        if (parentCanvas) return;
        parentCanvas = FindObjectOfType<Canvas>();
        if (!parentCanvas)
        {
            var go = new GameObject("__QS_Overlay__", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(go);
            parentCanvas = go.GetComponent<Canvas>();
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            parentCanvas.sortingOrder = 80000;
            go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }
    }

    static void SetGroup(CanvasGroup cg, bool on)
    {
        if (!cg) return;
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;
    }

    static void ForceActivateHierarchy(Transform t, bool on)
    {
        if (!t) return;
        for (var p = t; p != null; p = p.parent) p.gameObject.SetActive(on);
    }

    static void ForceRectStretch(RectTransform rt)
    {
        if (!rt) return;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one; rt.anchoredPosition3D = Vector3.zero;
    }

    void EnsureOverlayTopCanvas(GameObject go, int order)
    {
        var canvases = go.GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
        {
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.worldCamera = null;
            c.overrideSorting = true;
            c.sortingOrder = order;
            if (!c.TryGetComponent<GraphicRaycaster>(out _)) c.gameObject.AddComponent<GraphicRaycaster>();
        }
        parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        parentCanvas.overrideSorting = true;
        parentCanvas.sortingOrder = Mathf.Max(parentCanvas.sortingOrder, order - 1);
    }

    void RaiseToTop(GameObject go, int order)
    {
        if (!go) return;

        foreach (var c in go.GetComponentsInParent<Canvas>(true))
        {
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.worldCamera = null;
            c.overrideSorting = true;
            c.sortingOrder = order;
            if (!c.TryGetComponent<GraphicRaycaster>(out _)) c.gameObject.AddComponent<GraphicRaycaster>();
        }
        foreach (var c in go.GetComponentsInChildren<Canvas>(true))
        {
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.worldCamera = null;
            c.overrideSorting = true;
            c.sortingOrder = order + 1;
            if (!c.TryGetComponent<GraphicRaycaster>(out _)) c.gameObject.AddComponent<GraphicRaycaster>();
        }
        parentCanvas.overrideSorting = true;
        parentCanvas.sortingOrder = Mathf.Max(parentCanvas.sortingOrder, order - 1);
    }

    T FindOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponentInChildren<T>(true);
        if (!c) c = go.AddComponent<T>();
        return c;
    }

    T FindFirst<T>(GameObject go) where T : Component
    {
        var arr = go.GetComponentsInChildren<T>(true);
        return (arr != null && arr.Length > 0) ? arr[0] : null;
    }

    T FindNameContains<T>(GameObject go, string contains) where T : Component
    {
        if (string.IsNullOrEmpty(contains)) return FindFirst<T>(go);
        var arr = go.GetComponentsInChildren<T>(true);
        foreach (var a in arr)
            if (a.name.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0)
                return a;
        return FindFirst<T>(go);
    }

    // ★ 여기! 누락돼서 에러가 났던 유틸
    Button FindButtonByName(Transform root, string key)
    {
        if (!root || string.IsNullOrEmpty(key)) return null;
        var buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
            if (b.name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                return b;
        return null;
    }

    TMP_Text NewTMP(Transform parent, string text, int fontSize, Vector2 anchored, Vector2 size)
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

    Button NewBtn(Transform parent, string label, Vector2 anchored, float w, float h)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = anchored;

        var img = go.GetComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        NewTMP(rt, label, 28, Vector2.zero, new Vector2(w - 20, h - 16));
        return go.GetComponent<Button>();
    }
}

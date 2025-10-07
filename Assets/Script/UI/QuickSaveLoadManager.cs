using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using VN.SaveSystem;

[DisallowMultipleComponent]
public class QuickSaveLoadManager : MonoBehaviour
{
    [Header("REQUIRED")]
    [Tooltip("반드시 DialogueManager 컴포넌트를 드래그해서 넣어주세요.")]
    public MonoBehaviour dialogueManager;   // 수동 할당 필수

    [Header("Quick Slot")]
    public int quickSlotIndex = 0;

    // ───────── Confirm Popup (Prefab-based / optional) ─────────
    [Header("Confirm Popup (Prefab or Instance, optional)")]
    [Tooltip("이 프리팹을 넣으면 런타임에 자동으로 인스턴스 생성/바인딩됩니다.")]
    public GameObject confirmPopupPrefab;         // 프리팹(선택)
    [Tooltip("이미 씬에 존재하는 Confirm Popup 루트 CanvasGroup (프리팹 미사용 시)")]
    public CanvasGroup confirmGroup;              // 인스턴스 루트
    public TMP_Text confirmMessage;
    public Button confirmYesButton;
    public Button confirmNoButton;

    // ───────── Loading Overlay (Prefab-based / optional) ─────────
    [Header("Loading Overlay (Prefab or Instance, optional)")]
    [Tooltip("이 프리팹을 넣으면 런타임에 자동으로 인스턴스 생성/바인딩됩니다.")]
    public GameObject loadingOverlayPrefab;       // 프리팹(선택)
    [Tooltip("이미 씬에 존재하는 Loading Overlay 루트 CanvasGroup (프리팹 미사용 시)")]
    public CanvasGroup loadingGroup;              // 인스턴스 루트
    public TMP_Text loadingText;
    public Slider loadingBar;

    // ───────── 사이드카 저장/로드 (씬 이름 자동 전환) ─────────
    [Header("Sidecar Save")]
    public bool saveGameStateSidecar = true;   // _gs.json에 씬 이름 등 저장/로드

    private bool _inited;
    private Action _pendingYes;

    void Awake()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("[QuickSaveLoadManager] dialogueManager가 비었습니다. 인스펙터에 반드시 할당하세요.");
            return;
        }

        // 프리팹 → 자동 인스턴스 & 바인딩
        TrySpawnAndBindConfirmPrefab();
        TrySpawnAndBindLoadingPrefab();

        // Confirm 리스너
        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(() => { HideConfirm(); _pendingYes?.Invoke(); _pendingYes = null; });
        }
        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.RemoveAllListeners();
            confirmNoButton.onClick.AddListener(() => { HideConfirm(); _pendingYes = null; });
        }

        // 시작 시 숨김
        SetGroup(confirmGroup, false);
        SetGroup(loadingGroup, false);

        // Save/Load 콜백: 스토리 위치 저장/적용
        SaveManager.Instance.OnBuildSaveData = BuildSaveData;
        SaveManager.Instance.OnApplySaveData = ApplySaveData;

        _inited = true;
    }

    // ─────────────────────────────────────────────────────────────
    // Prefab instantiation helpers
    // ─────────────────────────────────────────────────────────────
    void TrySpawnAndBindConfirmPrefab()
    {
        // 이미 수동 연결되어 있으면 스킵
        if (confirmGroup != null && confirmYesButton != null) return;

        if (confirmPopupPrefab == null) return;

        var inst = Instantiate(confirmPopupPrefab);
        DontDestroyOnLoad(inst); // 필요시 제거 가능
        // 자동 바인딩: 자식에서 찾아 연결
        confirmGroup = FindInChildren<CanvasGroup>(inst.transform);
        confirmMessage = FindInChildren<TMP_Text>(inst.transform, nameContains: "Message");
        // 버튼 이름 힌트: "Yes", "No" 포함한 오브젝트 우선
        confirmYesButton = FindButtonByName(inst.transform, "Yes") ?? FindInChildren<Button>(inst.transform);
        confirmNoButton = FindButtonByName(inst.transform, "No");
        if (confirmGroup == null)
            Debug.LogWarning("[QuickSaveLoadManager] Confirm prefab에서 CanvasGroup을 찾지 못했습니다. 루트에 CanvasGroup을 넣어주세요.");
    }

    void TrySpawnAndBindLoadingPrefab()
    {
        if (loadingGroup != null) return;

        if (loadingOverlayPrefab == null) return;

        var inst = Instantiate(loadingOverlayPrefab);
        DontDestroyOnLoad(inst);
        loadingGroup = FindInChildren<CanvasGroup>(inst.transform);
        loadingText = FindInChildren<TMP_Text>(inst.transform, nameContains: "Text");
        loadingBar = FindInChildren<Slider>(inst.transform);
        if (loadingGroup == null)
            Debug.LogWarning("[QuickSaveLoadManager] Loading prefab에서 CanvasGroup을 찾지 못했습니다. 루트에 CanvasGroup을 넣어주세요.");
    }

    T FindInChildren<T>(Transform root, string nameContains = null) where T : Component
    {
        var comps = root.GetComponentsInChildren<T>(true);
        if (comps == null || comps.Length == 0) return null;
        if (string.IsNullOrEmpty(nameContains)) return comps[0];
        foreach (var c in comps)
            if (c.name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0)
                return c;
        return comps[0];
    }
    Button FindButtonByName(Transform root, string key)
    {
        var btns = root.GetComponentsInChildren<Button>(true);
        foreach (var b in btns)
            if (b.name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                return b;
        return null;
    }

    // ─────────────────────────────────────────────────────────────
    // 버튼 훅
    // ─────────────────────────────────────────────────────────────
    public void OnClickQuickSave()
    {
        if (!_inited) return;

        if (confirmGroup != null && confirmYesButton != null)
            ShowConfirm("퀵세이브 하시겠습니까?", () => StartCoroutine(SaveFlow()));
        else
            StartCoroutine(SaveFlow());
    }

    public void OnClickQuickLoad()
    {
        if (!_inited) return;

        var mainPath = Path.Combine(Application.persistentDataPath, $"save_slot_{quickSlotIndex}.json");
        if (!File.Exists(mainPath))
        {
            if (confirmGroup && confirmMessage)
                ShowConfirm("퀵세이브 데이터가 없습니다.", null);
            else
                Debug.LogWarning("[QuickSaveLoadManager] QuickLoad: no save file.");
            return;
        }

        if (confirmGroup != null && confirmYesButton != null)
            ShowConfirm("퀵로드 하시겠습니까?\n현재 진행이 사라질 수 있습니다.", () => StartCoroutine(LoadFlow()));
        else
            StartCoroutine(LoadFlow());
    }

    // ─────────────────────────────────────────────────────────────
    // 플로우
    // ─────────────────────────────────────────────────────────────
    private System.Collections.IEnumerator SaveFlow()
    {
        ShowLoading("Saving...", 0f);
        yield return null;

        // 1) 슬롯 저장(스토리 위치)
        SaveManager.Instance.Save(quickSlotIndex);

        // 2) 사이드카 저장(씬 이름 포함)
        if (saveGameStateSidecar) SaveSidecarSceneName(quickSlotIndex);

        UpdateLoading(1f);
        yield return new WaitForEndOfFrame();
        HideLoading();
        Debug.Log($"[QuickSaveLoadManager] QuickSave → slot {quickSlotIndex}");
    }

    private System.Collections.IEnumerator LoadFlow()
    {
        ShowLoading("Loading...", 0f);
        yield return null;

        // 1) 슬롯 로드(스토리 위치 적용 → ApplySaveData 호출됨)
        SaveManager.Instance.Load(quickSlotIndex);

        // 2) 사이드카에 씬 이름이 있으면 자동 전환
        if (saveGameStateSidecar)
        {
            var targetScene = LoadSidecarSceneName(quickSlotIndex);
            var active = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(targetScene) && !string.Equals(active, targetScene, StringComparison.Ordinal))
            {
                yield return LoadSceneAsync(targetScene);
                // 씬이 바뀌면 다시 한 번 저장된 슬롯을 로드하여 점프를 확실히 보장
                SaveManager.Instance.Load(quickSlotIndex);
            }
        }

        UpdateLoading(1f);
        yield return new WaitForEndOfFrame();
        HideLoading();
        Debug.Log($"[QuickSaveLoadManager] QuickLoad ← slot {quickSlotIndex}");
    }

    // ─────────────────────────────────────────────────────────────
    // Save/Load 콜백 (스토리 위치 저장/적용)
    // ─────────────────────────────────────────────────────────────
    private SaveData BuildSaveData()
    {
        var d = new SaveData();

        // 1) DialogueManager가 현재 위치를 직접 제공하면 우선 사용
        if (TryGetTriple(dialogueManager, out var ch, out var sc, out var nid))
        {
            d.story.chapter = ch;
            d.story.scene = sc;
            d.story.nodeId = nid;
        }
        else
        {
            // 2) 없으면 NodeId만 가져와 파싱
            var nodeId = TryGetCurrentNodeId(dialogueManager) ?? "N001";
            d.story.nodeId = nodeId;
            TryParseNodeId(nodeId, out var ch2, out var sc2, out _);
            d.story.chapter = string.IsNullOrEmpty(ch2) ? "CH1" : ch2;
            d.story.scene = string.IsNullOrEmpty(sc2) ? "SC1" : sc2;
        }

        d.title = $"[{d.story.chapter}/{d.story.scene}] {d.story.nodeId}";
        d.dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return d;
    }

    private void ApplySaveData(SaveData d)
    {
        if (d == null) return;
        StartCoroutine(WaitReadyThenJump(d)); // 씬 전환은 LoadFlow에서 처리
    }

    // ─────────────────────────────────────────────────────────────
    // 씬 전환 + 준비 대기 + 점프
    // ─────────────────────────────────────────────────────────────
    private System.Collections.IEnumerator LoadSceneAsync(string scene)
    {
        ShowLoading("Loading Scene...", 0f);
        var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        while (!op.isDone)
        {
            UpdateLoading(op.progress);
            yield return null;
        }
        yield return null; // one more frame
    }

    private System.Collections.IEnumerator WaitReadyThenJump(SaveData d)
    {
        ShowLoading("Preparing...", 0.9f);

        float timeout = 5f, t = 0f;
        while (!IsDialogueReady(dialogueManager))
        {
            t += Time.unscaledDeltaTime;
            if (t > timeout) break;
            yield return null;
        }

        bool done = false;

        if (!string.IsNullOrEmpty(d.story.nodeId))
            done = TryJumpToNode(dialogueManager, d.story.nodeId);

        if (!done)
        {
            foreach (var cand in BuildNodeIdCandidates(d.story.chapter, d.story.scene, d.story.nodeId))
            {
                if (TryJumpToNode(dialogueManager, cand)) { done = true; break; }
            }
        }

        if (!done) Debug.LogWarning("[QuickSaveLoadManager] Jump failed, keeping current node.");
    }

    // ─────────────────────────────────────────────────────────────
    // 사이드카(씬 이름) 저장/로드
    // ─────────────────────────────────────────────────────────────
    private string SidecarPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}_gs.json");

    private void SaveSidecarSceneName(int slot)
    {
        var payload = new GameStatePayload
        {
            gameSceneName = SceneManager.GetActiveScene().name
        };
        var json = JsonUtility.ToJson(payload, prettyPrint: false);
        File.WriteAllText(SidecarPath(slot), json, Encoding.UTF8);
        Debug.Log($"[QuickSaveLoadManager] Saved sidecar(scene) → {SidecarPath(slot)}");
    }

    private string LoadSidecarSceneName(int slot)
    {
        var path = SidecarPath(slot);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path, Encoding.UTF8);
        var payload = JsonUtility.FromJson<GameStatePayload>(json);
        if (payload == null) return null;

        return payload.gameSceneName;
    }

    // ─────────────────────────────────────────────────────────────
    // Confirm & Loading helpers
    // ─────────────────────────────────────────────────────────────
    private void ShowConfirm(string message, Action onYes)
    {
        if (confirmGroup == null || confirmYesButton == null)
        {
            onYes?.Invoke(); // 없으면 바로 진행
            return;
        }
        _pendingYes = onYes;
        if (confirmMessage) confirmMessage.text = message;
        SetGroup(confirmGroup, true);
    }
    private void HideConfirm() => SetGroup(confirmGroup, false);

    private void ShowLoading(string text, float progress = 0f)
    {
        if (loadingText) loadingText.text = text;
        if (loadingBar) loadingBar.value = Mathf.Clamp01(progress);
        SetGroup(loadingGroup, true);
    }
    private void UpdateLoading(float progress)
    {
        if (loadingBar) loadingBar.value = Mathf.Clamp01(progress);
    }
    private void HideLoading() => SetGroup(loadingGroup, false);

    private static void SetGroup(CanvasGroup cg, bool on)
    {
        if (!cg) return;
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;
        if (cg.gameObject) cg.gameObject.SetActive(on);
    }

    // ─────────────────────────────────────────────────────────────
    // 유틸 (DM 접근 & 점프)
    // ─────────────────────────────────────────────────────────────
    private static bool IsDialogueReady(MonoBehaviour dm)
    {
        if (dm == null) return false;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var pi = dm.GetType().GetProperty("IsReady", flags);
        if (pi != null && pi.CanRead) { try { if (pi.GetValue(dm) is bool b && b) return true; } catch { } }
        var fiR = dm.GetType().GetField("isReady", flags);
        if (fiR != null) { try { if (fiR.GetValue(dm) is bool b && b) return true; } catch { } }

        var fi = dm.GetType().GetField("dialogueNodes", flags);
        if (fi != null)
        {
            try
            {
                var obj = fi.GetValue(dm);
                if (obj is System.Collections.ICollection col && col.Count > 0) return true;
            }
            catch { }
        }
        return true; // 신호 없으면 대기 없이 진행
    }

    // DialogueManager가 (string chapter,string scene,string nodeId) 제공하면 사용
    private static bool TryGetTriple(MonoBehaviour dm, out string chapter, out string scene, out string nodeId)
    {
        chapter = null; scene = null; nodeId = null;
        if (dm == null) return false;

        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var mi = dm.GetType().GetMethod("GetCurrentTriple", flags);
        if (mi != null && mi.GetParameters().Length == 0)
        {
            try
            {
                var res = mi.Invoke(dm, null);
                if (res is ValueType || res is object)
                {
                    chapter = GetTupleString(res, "Item1");
                    scene = GetTupleString(res, "Item2");
                    nodeId = GetTupleString(res, "Item3");
                    if (!string.IsNullOrEmpty(nodeId)) return true;
                }
            }
            catch { }
        }
        return false;
    }
    private static string GetTupleString(object tuple, string itemName)
    {
        if (tuple == null) return null;
        var prop = tuple.GetType().GetProperty(itemName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop == null || !prop.CanRead) return null;
        return prop.GetValue(tuple) as string;
    }

    private static string TryGetCurrentNodeId(MonoBehaviour dm)
    {
        if (dm == null) return null;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var mi = dm.GetType().GetMethod("GetCurrentNodeId", flags);
        if (mi != null && mi.GetParameters().Length == 0)
        { try { return mi.Invoke(dm, null) as string; } catch { } }

        var pi = dm.GetType().GetProperty("CurrentNodeId", flags);
        if (pi != null && pi.CanRead)
        { try { return pi.GetValue(dm) as string; } catch { } }

        var fi = dm.GetType().GetField("currentNodeId", flags);
        if (fi != null)
        { try { return fi.GetValue(dm) as string; } catch { } }

        var fi2 = dm.GetType().GetField("nodeId", flags);
        if (fi2 != null)
        { try { return fi2.GetValue(dm) as string; } catch { } }

        return null;
    }

    private static bool TryJumpToNode(MonoBehaviour dm, string nodeId)
    {
        if (dm == null || string.IsNullOrEmpty(nodeId)) return false;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var name in new[] { "JumpToNode", "JumpTo", "GoToNode", "LoadNode", "StartAtNode",
                                     "SetNode", "SetCurrentNode", "ResumeAtNode", "ContinueFromNode" })
        {
            var mi = dm.GetType().GetMethod(name, flags);
            if (mi != null)
            {
                var ps = mi.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(string))
                { try { mi.Invoke(dm, new object[] { nodeId }); return true; } catch { } }
            }
        }

        bool injected = false;
        var pi = dm.GetType().GetProperty("CurrentNodeId", flags);
        if (pi != null && pi.CanWrite) { try { pi.SetValue(dm, nodeId); injected = true; } catch { } }
        if (!injected)
        {
            var fi = dm.GetType().GetField("currentNodeId", flags);
            if (fi != null) { try { fi.SetValue(dm, nodeId); injected = true; } catch { } }
        }
        if (!injected)
        {
            var fi2 = dm.GetType().GetField("nodeId", flags);
            if (fi2 != null) { try { fi2.SetValue(dm, nodeId); injected = true; } catch { } }
        }

        if (injected)
        {
            foreach (var r in new[] { "ShowCurrentNode", "RefreshUI", "RefreshDialogue", "UpdateUI", "ApplyState", "Rebuild" })
            {
                var miR = dm.GetType().GetMethod(r, flags);
                if (miR != null && miR.GetParameters().Length == 0)
                { try { miR.Invoke(dm, null); return true; } catch { } }
            }
            return true;
        }
        return false;
    }

    private static bool TryParseNodeId(string nodeId, out string chapter, out string scene, out string node)
    {
        chapter = null; scene = null; node = null;
        if (string.IsNullOrEmpty(nodeId)) return false;

        var raw = nodeId.Trim();
        char[] seps = new[] { '_', '-', '.', ':' };
        var parts = raw.Split(seps, StringSplitOptions.RemoveEmptyEntries);

        foreach (var p in parts)
        {
            var up = p.ToUpperInvariant();
            if (up.StartsWith("CH")) chapter = up;
            else if (up.StartsWith("SC")) scene = up;
            else if (up.StartsWith("N")) node = up;
            else if (int.TryParse(up, out _)) node = "N" + up;
        }
        if (node == null)
        {
            var last = parts[parts.Length - 1].ToUpperInvariant();
            node = last.StartsWith("N") ? last : "N" + last;
        }
        return !string.IsNullOrEmpty(node);
    }

    private static IEnumerable<string> BuildNodeIdCandidates(string chapter, string scene, string node)
    {
        chapter = string.IsNullOrEmpty(chapter) ? null : chapter.ToUpperInvariant();
        scene = string.IsNullOrEmpty(scene) ? null : scene.ToUpperInvariant();
        node = string.IsNullOrEmpty(node) ? null : node.ToUpperInvariant();

        if (chapter != null && !chapter.StartsWith("CH")) chapter = "CH" + chapter.TrimStart('C', 'H');
        if (scene != null && !scene.StartsWith("SC")) scene = "SC" + scene.TrimStart('S', 'C');
        if (node != null && !node.StartsWith("N")) node = "N" + node.TrimStart('N');

        if (chapter != null && scene != null && node != null)
        {
            yield return $"{chapter}_{scene}_{node}";
            yield return $"{chapter}-{scene}-{node}";
            yield return $"{chapter}.{scene}.{node}";
        }
        if (scene != null && node != null)
        {
            yield return $"{scene}_{node}";
            yield return $"{scene}-{node}";
            yield return $"{scene}.{node}";
        }
        if (node != null)
        {
            yield return node;
        }
    }
}

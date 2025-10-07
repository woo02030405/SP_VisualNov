using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using VN.SaveSystem;

public class QuickSaveLoadManager : MonoBehaviour
{
    [Header("Assign DialogueManager (only this)")]
    public MonoBehaviour dialogueManager;

    [Header("Quick Slot")]
    public int quickSlotIndex = 0;

    [Header("Optional: Chapter → Scene mapping")]
    public bool useChapterSceneMap = false;

    [Serializable]
    public struct ChapterScene { public string chapter; public string sceneName; }
    public List<ChapterScene> chapterScenes = new List<ChapterScene>();

    string GetSceneForChapter(string chapter)
    {
        if (string.IsNullOrEmpty(chapter)) return null;
        foreach (var m in chapterScenes)
            if (string.Equals(m.chapter, chapter, StringComparison.OrdinalIgnoreCase)) return m.sceneName;
        return null;
    }

    void Awake()
    {
        if (!dialogueManager)
        {
            Debug.LogError("[QuickSaveLoadManager] dialogueManager missing.");
            enabled = false; return;
        }

        SaveManager.Instance.OnBuildSaveData = BuildSaveData;
        SaveManager.Instance.OnApplySaveData = ApplySaveData;
    }

    // ───────────── 버튼에서 호출 ─────────────
    public void OnClickQuickSave()
    {
        EnsureUI();
        QuickConfirmPopup.Instance.Show("퀵세이브 하시겠습니까?", () => StartCoroutine(SaveFlow()));
    }

    public void OnClickQuickLoad()
    {
        EnsureUI();
        var path = PathForSlot(quickSlotIndex);
        if (!File.Exists(path))
        {
            QuickConfirmPopup.Instance.Show("퀵세이브 데이터가 없습니다.", () => { });
            return;
        }
        QuickConfirmPopup.Instance.Show("퀵로드 하시겠습니까?\n현재 진행이 사라질 수 있습니다.", () => StartCoroutine(LoadFlow()));
    }

    System.Collections.IEnumerator SaveFlow()
    {
        LoadingOverlay.Instance.Show("Saving...");
        yield return null; // 한 프레임 양보(표시 보장)
        SaveManager.Instance.Save(quickSlotIndex);
        yield return new WaitForSecondsRealtime(0.1f);
        LoadingOverlay.Instance.Hide();
    }

    System.Collections.IEnumerator LoadFlow()
    {
        LoadingOverlay.Instance.Show("Loading...");
        yield return null;
        // 아래 Load() → ApplySaveData() 호출(씬 전환이 필요하면 내부 코루틴에서 계속)
        SaveManager.Instance.Load(quickSlotIndex);
        // 대부분 동기적이므로 한두 프레임 후 닫기, 혹은 Apply 쪽에서 씬 전환 코루틴이 끝나면 닫음
        yield return new WaitForEndOfFrame();
        // 씬 전환/준비 대기가 있으면 ApplySaveData에서 닫아준다.
        if (LoadingOverlay.Instance) LoadingOverlay.Instance.Hide();
    }

    void EnsureUI()
    {
        if (!QuickConfirmPopup.Instance)
        {
            var go = new GameObject("[QuickConfirmPopup]");
            var cg = go.AddComponent<CanvasGroup>();
            var popup = go.AddComponent<QuickConfirmPopup>();
            popup.group = cg;
            // ※ 실제 게임에선 프리팹을 권장. 여기선 안전장치로 빈 팝업 생성.
        }
        if (!LoadingOverlay.Instance)
        {
            var go = new GameObject("[LoadingOverlay]");
            var cg = go.AddComponent<CanvasGroup>();
            var overlay = go.AddComponent<LoadingOverlay>();
            overlay.group = cg;
        }
    }

    // ───────────── Save/Load 콜백 ─────────────
    SaveData BuildSaveData()
    {
        var d = new SaveData();
        var nodeId = TryGetCurrentNodeId(dialogueManager) ?? "N001";
        d.story.nodeId = nodeId;

        TryParseNodeId(nodeId, out var ch, out var sc, out _);
        d.story.chapter = string.IsNullOrEmpty(ch) ? "CH1" : ch;
        d.story.scene = string.IsNullOrEmpty(sc) ? "SC1" : sc;

        d.title = $"[{d.story.chapter}/{d.story.scene}] {d.story.nodeId}";
        d.dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return d;
    }

    void ApplySaveData(SaveData d)
    {
        if (d == null) return;

        if (useChapterSceneMap)
        {
            var targetScene = GetSceneForChapter(d.story.chapter);
            if (!string.IsNullOrEmpty(targetScene))
            {
                var active = SceneManager.GetActiveScene().name;
                if (!string.Equals(active, targetScene, StringComparison.Ordinal))
                {
                    StartCoroutine(LoadSceneThenJump(targetScene, d));
                    return;
                }
            }
        }

        // 같은 씬이면 바로 점프
        StartCoroutine(WaitReadyThenJump(d));
    }

    // ───────────── 씬 전환 + 준비 대기 + 점프 ─────────────
    System.Collections.IEnumerator LoadSceneThenJump(string scene, SaveData d)
    {
        if (!LoadingOverlay.Instance) yield break;
        LoadingOverlay.Instance.Show("Loading Scene...");

        var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        while (!op.isDone)
        {
            LoadingOverlay.Instance.SetProgress(op.progress);
            yield return null;
        }

        // 씬 바뀌었으니 DM 다시 찾기(프로젝트 타입에 맞춰 수정 가능)
        dialogueManager = FindObjectOfType<MonoBehaviour>(true);
        yield return WaitReadyThenJump(d); // 내부에서 Hide()
    }

    System.Collections.IEnumerator WaitReadyThenJump(SaveData d)
    {
        if (LoadingOverlay.Instance) LoadingOverlay.Instance.Show("Preparing...");

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

        if (LoadingOverlay.Instance) LoadingOverlay.Instance.Hide();
    }

    // ───────────── Reflection helpers ─────────────
    static string TryGetCurrentNodeId(MonoBehaviour dm)
    {
        if (dm == null) return null;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var name in new[] { "GetCurrentNodeId" })
        {
            var mi = dm.GetType().GetMethod(name, flags);
            if (mi != null && mi.GetParameters().Length == 0)
            { try { var r = mi.Invoke(dm, null) as string; if (!string.IsNullOrEmpty(r)) return r; } catch { } }
        }
        foreach (var p in new[] { "CurrentNodeId", "NodeId" })
        {
            var pi = dm.GetType().GetProperty(p, flags);
            if (pi != null && pi.CanRead)
            { try { var r = pi.GetValue(dm) as string; if (!string.IsNullOrEmpty(r)) return r; } catch { } }
        }
        foreach (var f in new[] { "currentNodeId", "nodeId" })
        {
            var fi = dm.GetType().GetField(f, flags);
            if (fi != null)
            { try { var r = fi.GetValue(dm) as string; if (!string.IsNullOrEmpty(r)) return r; } catch { } }
        }
        return null;
    }

    static bool TryJumpToNode(MonoBehaviour dm, string nodeId)
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
        foreach (var p in new[] { "CurrentNodeId", "NodeId" })
        {
            var pi = dm.GetType().GetProperty(p, flags);
            if (pi != null && pi.CanWrite) { try { pi.SetValue(dm, nodeId); injected = true; break; } catch { } }
        }
        if (!injected)
        {
            foreach (var f in new[] { "currentNodeId", "nodeId" })
            {
                var fi = dm.GetType().GetField(f, flags);
                if (fi != null) { try { fi.SetValue(dm, nodeId); injected = true; break; } catch { } }
            }
        }

        if (injected)
        {
            foreach (var r in new[] { "ShowCurrentNode", "RefreshUI", "RefreshDialogue", "UpdateUI", "ApplyState", "Rebuild" })
            {
                var mi = dm.GetType().GetMethod(r, flags);
                if (mi != null && mi.GetParameters().Length == 0) { try { mi.Invoke(dm, null); return true; } catch { } }
            }
            return true;
        }
        return false;
    }

    static bool TryParseNodeId(string nodeId, out string chapter, out string scene, out string node)
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

    static IEnumerable<string> BuildNodeIdCandidates(string chapter, string scene, string node)
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

    static string PathForSlot(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    bool IsDialogueReady(MonoBehaviour dm)
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
        return false;
    }
}

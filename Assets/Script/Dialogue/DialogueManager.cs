using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // CSV 테이블
    private Dictionary<string, DialogueNode> dialogueNodes; // from Ch1_MainDialogue
    private Dictionary<string, StoryLine> storyLines;    // from Ch1_MainStory_kr
    private Dictionary<string, Speaker> speakers;      // from Speakers_kr (있으면 사용)

    [Header("Start")]
    [SerializeField] private string startNodeId = "N001";
    private string currentNodeId;

    [Header("UI")]
    public DialogueUI dialogueUI;         // 필수
    public EndingPopup endingPopupPrefab; // (선택) 엔딩 팝업
    public Transform uiLayer;             // (선택) 팝업 부모 Transform

    // 선택 에코 후 1클릭 소모용 큐
    private bool _awaitingAdvance = false;
    private string _queuedNextNode = null;

    // 선택지 표시 코루틴(로드/점프 시 중단용)
    private Coroutine _choicesRoutine;

    // 리소스 키(세이브/로드용)
    public string CurrentDialogueCsvKey { get; private set; } = "Ch1_MainDialogue";
    public string CurrentStoryCsvKey { get; private set; } = "Ch1_MainStory_kr";

    // ======= 세이브가 읽는 공개 프로퍼티 =======
    public string CurrentNodeId => currentNodeId;
    public string CurrentChapter => (dialogueNodes != null && dialogueNodes.TryGetValue(currentNodeId, out var n)) ? n.Chapter : "CH1";
    public string CurrentScene => (dialogueNodes != null && dialogueNodes.TryGetValue(currentNodeId, out var n)) ? n.Day : "SC1";

    private void Start()
    {
        if (!dialogueUI) { Debug.LogError("[DialogueManager] dialogueUI 연결 필요"); enabled = false; return; }
        if (!GameState.I) new GameObject("GameState", typeof(GameState)); // 전역 상태 보장

        // uiLayer 자동 할당(비워두면 대화 UI의 최상위 루트 사용)
        if (uiLayer == null) uiLayer = dialogueUI.transform.root;

        // CSV 로드 (모두 Resources 키)
        dialogueNodes = CSVLoader.LoadTable<DialogueNode>(CurrentDialogueCsvKey, "NodeId");
        storyLines = CSVLoader.LoadTable<StoryLine>(CurrentStoryCsvKey, "NodeId");
        speakers = CSVLoader.LoadTable<Speaker>("Speakers_kr", "SpeakerId"); // 없으면 null이어도 OK

        if (dialogueNodes == null || storyLines == null) { Debug.LogError("[DialogueManager] CSV 로드 실패"); enabled = false; return; }

        // CSV Skipping == "1" → 초기 읽음 처리 (ReadSkipBacklogManager가 있을 때만)
        foreach (var kv in dialogueNodes)
        {
            var node = kv.Value;
            if (!string.IsNullOrEmpty(node.Skipping) && node.Skipping.Trim() == "1")
                RSBM_SetInitialRead(node.NodeId, "1"); // Reflection 호출(없으면 조용히 무시)
        }

        // UI 콜백
        dialogueUI.onClickNext = Next;
        dialogueUI.onChoiceSelected = OnChoiceSelected;

        // 시작 노드
        currentNodeId = string.IsNullOrEmpty(startNodeId) ? FindFirstDialogueNodeId() : startNodeId;
        if (string.IsNullOrEmpty(currentNodeId)) { Debug.LogError("[DialogueManager] 시작 노드 없음"); enabled = false; return; }

        ShowCurrentNode();
    }

    private string FindFirstDialogueNodeId()
    {
        foreach (var kv in dialogueNodes)
        {
            var n = kv.Value;
            var t = (n.NodeType ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(t) || t == "SAY" || t == "DIALOGUE")
                return kv.Key;
        }
        foreach (var kv in storyLines) return kv.Key;
        return null;
    }

    // UI를 강제로 초기화(선택지/코루틴/타이핑/팝업 모두 정리)
    private void ResetUIHard()
    {
        // 선택지 코루틴 정지
        if (_choicesRoutine != null)
        {
            StopCoroutine(_choicesRoutine);
            _choicesRoutine = null;
        }

        // 타이핑/선택지 정리
        if (dialogueUI != null)
        {
            if (dialogueUI.IsTyping())
                dialogueUI.CompleteTyping();
            if (dialogueUI.HasChoices())
                dialogueUI.ClearChoices();
        }

        // EndingPopup 모두 제거(uiLayer 하위만)
        CloseAllPopups();

        _awaitingAdvance = false;
        _queuedNextNode = null;
    }

    private void CloseAllPopups()
    {
        var popups = FindObjectsOfType<EndingPopup>(true);
        foreach (var p in popups)
        {
            if (uiLayer == null || p.transform.IsChildOf(uiLayer))
                Destroy(p.gameObject);
        }
    }

    private void ShowCurrentNode()
    {
        if (string.IsNullOrEmpty(currentNodeId)) return;

        // 새 노드를 그리기 전에 UI를 항상 초기화
        ResetUIHard();

        dialogueNodes.TryGetValue(currentNodeId, out var node);
        storyLines.TryGetValue(currentNodeId, out var line);

        var nodeType = (node?.NodeType ?? "").Trim().ToUpperInvariant();

        // 텍스트 구성
        string spkId = line?.SpeakerId ?? "system";
        string speakerName = ResolveSpeakerName(spkId);
        string rawText = line?.Text ?? "";
        string shownText = DialogueTextEffect.Apply(rawText, node?.TextEffect);

        // 화면 표시
        dialogueUI.ShowDialogue(speakerName, shownText);

        // 읽음/백로그 확정(있으면)
        RSBM_MarkRead(currentNodeId);
        RSBM_AddBacklog(currentNodeId, speakerName, shownText);

        // END 처리
        if (nodeType == "END")
        {
            if (endingPopupPrefab && uiLayer)
            {
                var popup = Instantiate(endingPopupPrefab, uiLayer);
                popup.Show("엔딩", "저장되지 않은 진행은 사라집니다.");
            }
            return;
        }

        // CHOICE 묶음
        if (IsChoiceHeader(node))
        {
            var items = BuildChoicesForGroup(node.ChoiceGroup, node.Chapter, node.Day);
            if (items.Count > 0)
            {
                _choicesRoutine = StartCoroutine(WaitAndShowChoices(items));
                return;
            }
        }
    }

    private IEnumerator WaitAndShowChoices(List<(string nodeId, string label, string style, string args, string flags)> items)
    {
        while (dialogueUI.IsTyping()) yield return null;
        dialogueUI.ShowChoices(items);
    }

    private string ResolveSpeakerName(string speakerId)
    {
        if (speakers != null && speakers.TryGetValue(speakerId, out var s) && !string.IsNullOrEmpty(s.Name))
            return s.Name;
        return speakerId ?? "";
    }

    private bool IsChoiceHeader(DialogueNode node)
    {
        if (node == null) return false;
        var t = (node.NodeType ?? "").Trim().ToUpperInvariant();
        return t == "CHOICE" || t.StartsWith("CHOICE");
    }

    private List<(string nodeId, string label, string style, string args, string flags)>
        BuildChoicesForGroup(string group, string chapter, string day)
    {
        var list = new List<(string nodeId, string label, string style, string args, string flags)>();
        if (string.IsNullOrEmpty(group)) return list;

        foreach (var kv in dialogueNodes)
        {
            var n = kv.Value;
            if (n == null) continue;
            var t = (n.NodeType ?? "").Trim().ToUpperInvariant();

            if ((t == "SAY" || t == "DIALOGUE" || t.StartsWith("CHOICE")) &&
                string.Equals(n.ChoiceGroup?.Trim(), group?.Trim(), StringComparison.Ordinal) &&
                string.Equals(n.Chapter, chapter, StringComparison.Ordinal) &&
                string.Equals(n.Day, day, StringComparison.Ordinal))
            {
                string label = n.NodeId;
                if (storyLines != null && storyLines.TryGetValue(n.NodeId, out var sl) && !string.IsNullOrEmpty(sl.ChoiceText))
                    label = sl.ChoiceText;

                list.Add((n.NodeId, label, n.ChoiceStyle, n.ChoiceArgs, n.ChoiceFlags));
            }
        }
        list.Sort((a, b) => string.CompareOrdinal(a.nodeId, b.nodeId));
        return list;
    }

    // 선택 즉시 이동(랜덤/조건 점프 등)
    private void DirectAdvance(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;
        ResetUIHard();
        currentNodeId = nodeId;
        ShowCurrentNode();
    }

    private void OnChoiceSelected(GameObject btn, string choiceNodeId, string label)
    {
        if (!dialogueNodes.TryGetValue(choiceNodeId, out var n)) { Debug.LogWarning($"선택 노드 없음: {choiceNodeId}"); return; }
        storyLines.TryGetValue(choiceNodeId, out var line);

        // 1) 선택지 자체 조건/효과
        bool cond = ConditionEvaluator.Evaluate(n.Conditions, line);
        if (!cond)
        {
            if (ConditionEvaluator.Evaluate(n.ElseIfConditions, line))
            {
                var j = EffectRunner.Apply(n.ElseIfEffects, line);
                if (!string.IsNullOrEmpty(j)) { DirectAdvance(j); return; }
            }
            else
            {
                var j = EffectRunner.Apply(n.ElseEffects, line);
                if (!string.IsNullOrEmpty(j)) { DirectAdvance(j); return; }
                Debug.Log("[DialogueManager] 선택 조건 불일치. 진행 취소.");
                return;
            }
        }
        else
        {
            var j = EffectRunner.Apply(n.Effects, line);
            if (!string.IsNullOrEmpty(j)) { DirectAdvance(j); return; }
        }

        // 2) 선택 에코 → 백로그
        var hero = ResolveSpeakerName("hero");
        dialogueUI.ShowDialogue(string.IsNullOrEmpty(hero) ? "주인공" : hero, label);
        RSBM_MarkRead(choiceNodeId);
        RSBM_AddBacklog(choiceNodeId, string.IsNullOrEmpty(hero) ? "주인공" : hero, label);

        // 3) NextNodeId로 즉시 이동
        if (!string.IsNullOrEmpty(n.NextNodeId)) { DirectAdvance(n.NextNodeId); return; }
        Debug.LogWarning($"선택 노드 NextNodeId 없음: {choiceNodeId}");
    }

    public void Next()
    {
        if (dialogueUI && dialogueUI.HasChoices()) return;
        if (dialogueUI && dialogueUI.IsTyping()) { dialogueUI.CompleteTyping(); return; }

        // 선택 에코 이후 대기 중이면 우선 처리
        if (_awaitingAdvance && !string.IsNullOrEmpty(_queuedNextNode))
        {
            currentNodeId = _queuedNextNode;
            _queuedNextNode = null;
            _awaitingAdvance = false;
            ShowCurrentNode();
            return;
        }

        // 일반 진행: 조건/효과 체인 → 점프 우선
        if (dialogueNodes.TryGetValue(currentNodeId, out var cur))
        {
            storyLines.TryGetValue(currentNodeId, out var line);

            var j = EffectRunner.RunNodeChain(cur, line);
            if (!string.IsNullOrEmpty(j))
            {
                currentNodeId = j;
                ShowCurrentNode();
                return;
            }

            if (!string.IsNullOrEmpty(cur.NextNodeId))
            {
                currentNodeId = cur.NextNodeId;
                ShowCurrentNode();
                return;
            }

            Debug.LogWarning($"NextNodeId 비어있음: {currentNodeId}");
        }
        else
        {
            Debug.LogWarning($"현재 노드 없음: {currentNodeId}");
        }
    }

    // ===== 스킵 게이트 =====
    public bool CanSkipCurrent() => RSBM_IsRead(currentNodeId);

    // ===== 세이브 연동(QuickSaveLoadManager 호환) =====
    public string GetCurrentCsvPath() => CurrentStoryCsvKey;

    public string[] ExportCsvRows()
    {
        var ta = Resources.Load<TextAsset>(CurrentStoryCsvKey); // CSVLoader 키와 통일
        if (!ta) return null;
        return ta.text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
    }

    public void LoadCsvByPath(string resKey)
    {
        if (string.IsNullOrEmpty(resKey)) return;

        // CSV 재로딩 전 UI 정리(선택지/팝업 포함)
        ResetUIHard();

        var table = CSVLoader.LoadTable<StoryLine>(resKey, "NodeId");
        if (table == null) { Debug.LogWarning($"LoadCsvByPath 실패: {resKey}"); return; }
        storyLines = table;
        CurrentStoryCsvKey = resKey;
        ShowCurrentNode();
    }

    // ===== 퀵로드 점프 API =====
    public bool JumpToNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId) || dialogueNodes == null || !dialogueNodes.ContainsKey(nodeId))
        { Debug.LogWarning($"JumpToNode 실패: {nodeId}"); return false; }

        // 점프 전 UI 정리(선택지/팝업 포함)
        ResetUIHard();

        currentNodeId = nodeId;
        ShowCurrentNode();
        return true;
    }
    public void SetNode(string nodeId) => JumpToNode(nodeId);

    // ================== ReadSkipBacklogManager(옵션, 리플렉션) ==================
    static object RSBM_Instance()
    {
        var t = Type.GetType("ReadSkipBacklogManager");
        if (t == null) return null;
        var prop = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        return prop?.GetValue(null);
    }
    static void RSBM_SetInitialRead(string nodeId, string cell)
    {
        var inst = RSBM_Instance(); if (inst == null) return;
        var m = inst.GetType().GetMethod("SetInitialReadByCsv", BindingFlags.Public | BindingFlags.Instance);
        m?.Invoke(inst, new object[] { nodeId, cell });
    }
    static bool RSBM_IsRead(string nodeId)
    {
        var inst = RSBM_Instance(); if (inst == null) return false;
        var m = inst.GetType().GetMethod("IsRead", BindingFlags.Public | BindingFlags.Instance);
        if (m == null) return false;
        return (bool)m.Invoke(inst, new object[] { nodeId });
    }
    static void RSBM_MarkRead(string nodeId)
    {
        var inst = RSBM_Instance(); if (inst == null) return;
        var m = inst.GetType().GetMethod("MarkRead", BindingFlags.Public | BindingFlags.Instance);
        m?.Invoke(inst, new object[] { nodeId });
    }
    static void RSBM_AddBacklog(string nodeId, string speaker, string text)
    {
        var inst = RSBM_Instance(); if (inst == null) return;
        var m = inst.GetType().GetMethod("AddBacklog", BindingFlags.Public | BindingFlags.Instance);
        m?.Invoke(inst, new object[] { nodeId, speaker, text });
    }
}

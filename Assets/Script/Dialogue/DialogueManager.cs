using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    private Dictionary<string, DialogueNode> dialogueNodes;
    private Dictionary<string, StoryLine> storyLines;
    private Dictionary<string, Speaker> speakers;

    [SerializeField] private string startNodeId = "S1_N001";
    private string currentNodeId;

    // 선택 에코 → 다음 입력에서 이동
    private bool _awaitingAdvance = false;
    private string _queuedNextNodeId = null;

    // END 노드 처리: 대사 먼저 보여주고, 최소 한 번 클릭을 소모한 뒤 종료
    private bool _pendingEnd = false;        // 지금 노드가 END 상태인지
    private bool _endNeedsConfirm = false;   // END 진입 후 첫 클릭을 반드시 소비

    [Header("References")]
    public DialogueUI dialogueUI;

    private void Start()
    {
        if (!dialogueUI) { Debug.LogError("[DialogueManager] dialogueUI 연결 필요"); enabled = false; return; }

        // 파일명은 네가 쓰는 이름으로 고정
        dialogueNodes = CSVLoader.LoadTable<DialogueNode>("Ch1_MainDialogue", "NodeId");
        storyLines = CSVLoader.LoadTable<StoryLine>("Ch1_MainStory_kr", "NodeId");
        speakers = CSVLoader.LoadTable<Speaker>("Speakers_kr", "SpeakerId");

        if (dialogueNodes == null || storyLines == null || speakers == null)
        { Debug.LogError("[DialogueManager] CSV 로드 실패"); enabled = false; return; }

        dialogueUI.onClickNext = Next;
        dialogueUI.onChoiceSelected = OnChoiceSelected;

        currentNodeId = string.IsNullOrEmpty(startNodeId) ? FindFirstNodeId() : startNodeId;
        if (string.IsNullOrEmpty(currentNodeId)) { Debug.LogError("[DialogueManager] 시작 노드 없음"); enabled = false; return; }

        ShowCurrentNode(); // 입장 시 효과/점프 실행하지 않음
    }

    private string FindFirstNodeId()
    {
        // Dialogue로 시작하는 첫 노드 우선
        foreach (var kv in dialogueNodes)
        {
            var node = kv.Value;
            if (string.IsNullOrEmpty(node.NodeType) || node.NodeType == "Dialogue")
                return kv.Key;
        }
        // 없으면 아무거나
        foreach (var kv in storyLines) return kv.Key;
        return null;
    }

    /// 현재 노드의 대사/선택지만 보여준다. (입장 이펙트 실행 X)
    private void ShowCurrentNode()
    {
        dialogueNodes.TryGetValue(currentNodeId, out var node);
        storyLines.TryGetValue(currentNodeId, out var line);

        // END 여부
        _pendingEnd = (node != null && !string.IsNullOrEmpty(node.NodeType) && node.NodeType.Equals("END"));

        // 대사 출력(스토리 라인 없어도 안전)
        var spkId = line?.SpeakerId ?? "system";
        var spk = speakers.ContainsKey(spkId) ? speakers[spkId] : new Speaker { Name = spkId };
        string text = line?.Text ?? "";
        string processed = (node != null) ? DialogueTextEffect.Apply(text, node.TextEffect) : text;
        dialogueUI.ShowDialogue(spk.Name, processed);

        // END면 선택지 없이 표시만 하고, 다음 클릭 한 번은 반드시 소비
        if (_pendingEnd)
        {
            _awaitingAdvance = false;
            _queuedNextNodeId = null;
            _endNeedsConfirm = true;   // ★ 이 플래그가 한 번의 클릭을 먹는다
            return;
        }

        // ChoiceGroup 묶음 → 버튼 생성 (현재 노드가 Choice일 때만)
        var choiceIds = CollectChoiceGroupByDialogue(currentNodeId);
        if (choiceIds.Count > 0)
        {
            var items = new List<(string nodeId, string label, string style, string args, string flags)>();
            foreach (var cid in choiceIds)
            {
                string label = (storyLines.TryGetValue(cid, out var cLine) && !string.IsNullOrEmpty(cLine.ChoiceText)) ? cLine.ChoiceText : cid;
                var n = dialogueNodes[cid];
                items.Add((cid, label, n.ChoiceStyle, n.ChoiceArgs, n.ChoiceFlags));
            }
            dialogueUI.ShowChoices(items);
        }

        _awaitingAdvance = false;
        _queuedNextNodeId = null;
    }

    private List<string> CollectChoiceGroupByDialogue(string currentId)
    {
        var list = new List<string>();
        if (!dialogueNodes.TryGetValue(currentId, out var cur)) return list;

        bool isChoice = !string.IsNullOrEmpty(cur.NodeType) && cur.NodeType.StartsWith("Choice");
        if (!isChoice) return list;

        string group = cur.ChoiceGroup?.Trim();
        if (string.IsNullOrEmpty(group)) { list.Add(currentId); return list; }

        foreach (var kv in dialogueNodes)
        {
            var n = kv.Value;
            if (n.Chapter == cur.Chapter && n.Day == cur.Day &&
                !string.IsNullOrEmpty(n.NodeType) && n.NodeType.StartsWith("Choice") &&
                (n.ChoiceGroup?.Trim() == group))
            {
                list.Add(n.NodeId);
            }
        }
        list.Sort();
        return list;
    }

    private void OnChoiceSelected(GameObject selectedBtn, string choiceNodeId, string label)
    {
        if (!dialogueNodes.TryGetValue(choiceNodeId, out var choiceNode))
        { Debug.LogWarning($"선택 노드 없음: {choiceNodeId}"); return; }

        storyLines.TryGetValue(choiceNodeId, out var choiceLine);

        // 1) 조건 판정
        bool cond = ConditionEvaluator.Evaluate(choiceNode.Conditions, choiceLine);
        bool elif = !cond && ConditionEvaluator.Evaluate(choiceNode.ElseIfConditions, choiceLine);
        string eff = cond ? choiceNode.Effects
                   : elif ? choiceNode.ElseIfEffects
                   : choiceNode.ElseEffects;

        // 2) 효과 실행 (+ jump)
        string jump = EffectRunner.Apply(eff, choiceLine);

        // 3) 실패 처리 (Else 경로 + jump 없음)
        if (!cond && !elif && string.IsNullOrEmpty(jump))
        {
            string msg = (choiceLine != null && !string.IsNullOrEmpty(choiceLine.ElseEffectsMessage))
                       ? choiceLine.ElseEffectsMessage
                       : "조건이 부족합니다.";
            dialogueUI.ShowFloatingHintAtSpeaker(choiceLine?.SpeakerId, msg);

            var t = selectedBtn.transform;
            t.DOShakePosition(0.25f, 12f, 18, 90f, false, true);
            t.DOShakeScale(0.25f, 0.2f);
            return;
        }

        // 4) 성공: FlagTag 기록
        if (!string.IsNullOrEmpty(choiceNode.FlagTag))
        {
            foreach (var tag in choiceNode.FlagTag.Split(';'))
            {
                var t = tag.Trim();
                if (!string.IsNullOrEmpty(t)) GameState.I.SetFlag(t);
            }
        }

        // 5) 다음 노드
        string next = !string.IsNullOrEmpty(jump) ? jump : choiceNode.NextNodeId;
        if (string.IsNullOrEmpty(next)) { Debug.LogWarning($"NextNodeId 비어있음: {choiceNodeId}"); return; }

        // 6) 선택 에코 → 다음 입력에서 이동
        dialogueUI.ClearChoices();
        string heroName = speakers.ContainsKey("hero") ? speakers["hero"].Name : "주인공";
        dialogueUI.ShowDialogue(heroName, label);

        _queuedNextNodeId = next;
        _awaitingAdvance = true;
        _pendingEnd = false; // 선택 후에는 END 대기 해제
        _endNeedsConfirm = false;
    }

    public void Next()
    {
        // 선택 열려 있으면 진행 차단
        if (dialogueUI != null && dialogueUI.HasChoices()) return;

        // (1) 타이핑 중이면 이번 클릭은 타이핑 완료
        if (dialogueUI != null && dialogueUI.IsTyping())
        {
            dialogueUI.CompleteTyping();
            return;
        }

        // (2) END 표시 중이면: 첫 클릭은 반드시 소비, 두 번째 클릭에서 종료
        if (_pendingEnd)
        {
            if (_endNeedsConfirm)
            {
                _endNeedsConfirm = false; // 첫 클릭 소비
                return;
            }
            Debug.Log("스토리 종료(END) → 맵 이동");
            // 여기서 실제 맵 이동 함수를 호출하면 됨.
            _pendingEnd = false;
            return;
        }

        // (3) 선택 에코 대기 → 큐로 이동
        if (_awaitingAdvance && !string.IsNullOrEmpty(_queuedNextNodeId))
        {
            currentNodeId = _queuedNextNodeId;
            _awaitingAdvance = false;
            _queuedNextNodeId = null;
            ShowCurrentNode();
            return;
        }

        // (4) Dialogue면 지금 클릭 시 체인 실행 → 점프 우선
        if (dialogueNodes.TryGetValue(currentNodeId, out var cur) &&
            (string.IsNullOrEmpty(cur.NodeType) || cur.NodeType.Equals("Dialogue")))
        {
            storyLines.TryGetValue(currentNodeId, out var line);

            string jump = EffectRunner.RunNodeChain(cur, line); // 클릭 시 실행
            if (!string.IsNullOrEmpty(jump))
            {
                currentNodeId = jump;
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
            return;
        }

        // (5) 안전망
        if (dialogueNodes.TryGetValue(currentNodeId, out var node) && !string.IsNullOrEmpty(node.NextNodeId))
        {
            currentNodeId = node.NextNodeId;
            ShowCurrentNode();
        }
        else
        {
            Debug.Log("다음 없음 → 맵");
        }
    }
}

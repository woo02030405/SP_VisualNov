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

    [Header("References")]
    public DialogueUI dialogueUI;

    private void Start()
    {
        if (!dialogueUI) { Debug.LogError("[DialogueManager] dialogueUI 연결 필요"); enabled = false; return; }

        // 파일명: 네가 쓰는 이름으로 고정
        dialogueNodes = CSVLoader.LoadTable<DialogueNode>("Ch1_MainDialogue", "NodeId");
        storyLines = CSVLoader.LoadTable<StoryLine>("Ch1_MainStory_kr", "NodeId");
        speakers = CSVLoader.LoadTable<Speaker>("Speakers_kr", "SpeakerId");

        if (dialogueNodes == null || storyLines == null || speakers == null)
        { Debug.LogError("[DialogueManager] CSV 로드 실패"); enabled = false; return; }

        dialogueUI.onClickNext = Next;
        dialogueUI.onChoiceSelected = OnChoiceSelected;

        // (옵션) 수치 팝업 이벤트 — EffectRunner에 이미 포함되어 있지 않다면 주석 처리 가능
        if (EffectRunner.OnStatApplied != null)
        {
            EffectRunner.OnStatApplied += (targetId, statKey, delta) =>
            {
                dialogueUI?.ShowStatPopup(targetId, statKey, delta);
            };
        }

        currentNodeId = string.IsNullOrEmpty(startNodeId) ? FindFirstNodeId() : startNodeId;
        if (string.IsNullOrEmpty(currentNodeId)) { Debug.LogError("[DialogueManager] 시작 노드 없음"); enabled = false; return; }

        ShowCurrentNode(); // 입장 시 효과/점프 실행하지 않음
    }

    private string FindFirstNodeId()
    {
        foreach (var kv in dialogueNodes) // 스토리 대신 노드 테이블 기준으로도 안전
        {
            var node = kv.Value;
            if (string.IsNullOrEmpty(node.NodeType) || node.NodeType == "Dialogue")
                return kv.Key;
        }
        foreach (var kv in storyLines) return kv.Key;
        return null;
    }

    /// 현재 노드의 대사/선택지만 보여준다. (입장 이펙트 실행 X)
    private void ShowCurrentNode()
    {
        // END 안전 처리
        if (dialogueNodes.TryGetValue(currentNodeId, out var node)
            && !string.IsNullOrEmpty(node.NodeType) && node.NodeType.Equals("END"))
        {
            Debug.Log("스토리 종료(END) → 맵 이동");
            return;
        }

        // 스토리 라인 없어도 깨지지 않게 TryGet
        storyLines.TryGetValue(currentNodeId, out var line);

        // 대사 출력 (없으면 system/빈 문자열로 안전 표시)
        var spkId = line?.SpeakerId ?? "system";
        var spk = speakers.ContainsKey(spkId) ? speakers[spkId] : new Speaker { Name = spkId };
        string text = line?.Text ?? "";
        string processed = (node != null) ? DialogueTextEffect.Apply(text, node.TextEffect) : text;
        dialogueUI.ShowDialogue(spk.Name, processed);

        // ChoiceGroup 묶음 수집 → 버튼 생성 (현재 노드가 Choice일 때만)
        var choiceIds = CollectChoiceGroupByDialogue(currentNodeId);
        if (choiceIds.Count > 0)
        {
            var items = new List<(string nodeId, string label, string style, string args, string flags)>();
            foreach (var cid in choiceIds)
            {
                // 버튼 라벨은 스토리 라인이 없어도 cid로 대체
                string label = storyLines.TryGetValue(cid, out var cLine) && !string.IsNullOrEmpty(cLine.ChoiceText)
                               ? cLine.ChoiceText : cid;
                var n = dialogueNodes[cid];
                items.Add((cid, label, n.ChoiceStyle, n.ChoiceArgs, n.ChoiceFlags));
            }
            dialogueUI.ShowChoices(items);
        }

        _awaitingAdvance = false;
        _queuedNextNodeId = null;
    }

    /// 같은 Day/Group의 Choice들을 모은다(현재 노드가 Choice일 때만)
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

        // (2) 선택 에코 대기 → 큐로 이동
        if (_awaitingAdvance && !string.IsNullOrEmpty(_queuedNextNodeId))
        {
            currentNodeId = _queuedNextNodeId;
            _awaitingAdvance = false;
            _queuedNextNodeId = null;
            ShowCurrentNode();
            return;
        }

        // (3) Dialogue면 지금 클릭 시 체인 실행 → 점프 우선
        if (dialogueNodes.TryGetValue(currentNodeId, out var cur) &&
            (string.IsNullOrEmpty(cur.NodeType) || cur.NodeType.Equals("Dialogue")))
        {
            // 스토리 라인이 없어도 진행되게 방어
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

        // (4) 안전망
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

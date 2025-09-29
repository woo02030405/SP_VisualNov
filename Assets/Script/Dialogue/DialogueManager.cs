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

    // 에코/지연 이동
    private bool _awaitingAdvance = false;
    private string _queuedNextNodeId = null;

    [Header("References")]
    public DialogueUI dialogueUI;

    private void Start()
    {
        if (!dialogueUI) { Debug.LogError("[DialogueManager] dialogueUI 연결 필요"); enabled = false; return; }

        dialogueNodes = CSVLoader.LoadTable<DialogueNode>("Ch1_MainDialogue", "NodeId");
        storyLines = CSVLoader.LoadTable<StoryLine>("Ch1_MainStory_kr", "NodeId");
        speakers = CSVLoader.LoadTable<Speaker>("Speakers_kr", "SpeakerId");

        if (dialogueNodes == null || storyLines == null || speakers == null)
        { Debug.LogError("[DialogueManager] CSV 로드 실패"); enabled = false; return; }

        dialogueUI.onClickNext = Next;
        dialogueUI.onChoiceSelected = OnChoiceSelected;

        currentNodeId = string.IsNullOrEmpty(startNodeId) ? FindFirstNodeId() : startNodeId;
        if (string.IsNullOrEmpty(currentNodeId)) { Debug.LogError("[DialogueManager] 시작 노드 없음"); enabled = false; return; }

        ShowCurrentNode(applyEffects: true);
    }

    private string FindFirstNodeId()
    {
        foreach (var kv in storyLines) return kv.Key;
        return null;
    }

    private void ShowCurrentNode(bool applyEffects)
    {
        if (!storyLines.ContainsKey(currentNodeId)) { Debug.Log("스토리 종료 → 맵 이동"); return; }

        var line = storyLines[currentNodeId];
        DialogueNode node = dialogueNodes.ContainsKey(currentNodeId) ? dialogueNodes[currentNodeId] : null;

        // END 처리
        if (node != null && !string.IsNullOrEmpty(node.NodeType) && node.NodeType.Equals("END"))
        { Debug.Log("스토리 종료(END) → 맵 이동"); return; }

        // 진입 이펙트 체인
        if (applyEffects && node != null)
        {
            string jump = EffectRunner.RunNodeChain(node, line);
            if (!string.IsNullOrEmpty(jump))
            {
                currentNodeId = jump;
                ShowCurrentNode(applyEffects: true);
                return;
            }
        }

        // 대사 표시
        var spk = speakers.ContainsKey(line.SpeakerId) ? speakers[line.SpeakerId] : new Speaker { Name = "???" };
        string processed = (node != null) ? DialogueTextEffect.Apply(line.Text, node.TextEffect) : line.Text;
        dialogueUI.ShowDialogue(spk.Name, processed);

        // ChoiceGroup 묶음 수집 → style/args/flags까지 전달
        var choiceIds = CollectChoiceGroupByDialogue(currentNodeId);
        if (choiceIds.Count > 0)
        {
            var items = new List<(string nodeId, string label, string style, string args, string flags)>();
            foreach (var cid in choiceIds)
            {
                string label = storyLines.ContainsKey(cid) ? storyLines[cid].ChoiceText : cid;
                var n = dialogueNodes[cid];
                items.Add((cid, label, n.ChoiceStyle, n.ChoiceArgs, n.ChoiceFlags));
            }
            dialogueUI.ShowChoices(items);
        }

        // 에코 대기 초기화
        _awaitingAdvance = false;
        _queuedNextNodeId = null;
    }

    private List<string> CollectChoiceGroupByDialogue(string currentId)
    {
        var list = new List<string>();
        if (!dialogueNodes.ContainsKey(currentId)) return list;

        var cur = dialogueNodes[currentId];
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

        var choiceLine = storyLines.ContainsKey(choiceNodeId) ? storyLines[choiceNodeId] : null;

        // 1) 조건 판정 (Conditions → ElseIf → Else)
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
            dialogueUI.ShowChoiceHint?.Invoke(msg);

            // 최소 기본 흔들림
            var t = selectedBtn.transform;
            t.DOShakePosition(0.25f, 12f, 18, 90f, false, true);
            t.DOShakeScale(0.25f, 0.2f);
            return;
        }

        // 4) 성공 처리: 선택 에코 → 다음 입력에서 이동
        string next = !string.IsNullOrEmpty(jump) ? jump : choiceNode.NextNodeId;
        if (string.IsNullOrEmpty(next)) { Debug.LogWarning($"NextNodeId 비어있음: {choiceNodeId}"); return; }

        dialogueUI.ClearChoices();

        string heroName = speakers.ContainsKey("hero") ? speakers["hero"].Name : "주인공";
        dialogueUI.ShowDialogue(heroName, label);

        _queuedNextNodeId = next;
        _awaitingAdvance = true;
    }

    public void Next()
    {
        if (dialogueUI != null && dialogueUI.HasChoices()) return;

        if (_awaitingAdvance && !string.IsNullOrEmpty(_queuedNextNodeId))
        {
            currentNodeId = _queuedNextNodeId;
            _awaitingAdvance = false;
            _queuedNextNodeId = null;
            ShowCurrentNode(applyEffects: true);
            return;
        }

        if (!dialogueNodes.ContainsKey(currentNodeId)) { Debug.Log("다음 없음 → 맵"); return; }
        var node = dialogueNodes[currentNodeId];

        if (!string.IsNullOrEmpty(node.NodeType) && node.NodeType.Equals("END"))
        { Debug.Log("스토리 종료(END) → 맵 이동"); return; }

        if (string.IsNullOrEmpty(node.NextNodeId))
        { Debug.LogWarning($"NextNodeId 비어있음: {currentNodeId}"); return; }

        currentNodeId = node.NextNodeId;
        ShowCurrentNode(applyEffects: true);
    }
}

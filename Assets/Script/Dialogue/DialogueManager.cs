using UnityEngine;
using System.Collections;
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
    private bool _pendingEnd = false;
    private bool _endNeedsConfirm = false;

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

        ShowCurrentNode();
    }

    private string FindFirstNodeId()
    {
        foreach (var kv in dialogueNodes)
        {
            var node = kv.Value;
            if (string.IsNullOrEmpty(node.NodeType) || node.NodeType == "Dialogue")
                return kv.Key;
        }
        foreach (var kv in storyLines) return kv.Key;
        return null;
    }

    private void ShowCurrentNode()
    {
        dialogueNodes.TryGetValue(currentNodeId, out var node);
        storyLines.TryGetValue(currentNodeId, out var line);

        _pendingEnd = (node != null && !string.IsNullOrEmpty(node.NodeType) && node.NodeType.Equals("END"));

        var spkId = line?.SpeakerId ?? "system";
        var spk = speakers.ContainsKey(spkId) ? speakers[spkId] : new Speaker { Name = spkId };
        string text = line?.Text ?? "";
        string processed = (node != null) ? DialogueTextEffect.Apply(text, node.TextEffect) : text;
        dialogueUI.ShowDialogue(spk.Name, processed);

        if (_pendingEnd)
        {
            _awaitingAdvance = false;
            _queuedNextNodeId = null;
            _endNeedsConfirm = true;
            return;
        }

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
            StartCoroutine(WaitAndShowChoices(items));
        }

        _awaitingAdvance = false;
        _queuedNextNodeId = null;
    }

    private IEnumerator WaitAndShowChoices(List<(string nodeId, string label, string style, string args, string flags)> items)
    {
        while (dialogueUI.IsTyping())
            yield return null;
        dialogueUI.ShowChoices(items);
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

        bool cond = ConditionEvaluator.Evaluate(choiceNode.Conditions, choiceLine);
        bool elif = !cond && ConditionEvaluator.Evaluate(choiceNode.ElseIfConditions, choiceLine);
        string eff = cond ? choiceNode.Effects
                   : elif ? choiceNode.ElseIfEffects
                   : choiceNode.ElseEffects;

        string jump = EffectRunner.Apply(eff, choiceLine);

        if (!cond && !elif && string.IsNullOrEmpty(jump))
        {
            // 이미 실패한 버튼이면 패널티는 다시 적용하지 않음
            if (!selectedBtn.GetComponent<ChoiceFailMarker>())
            {
                selectedBtn.AddComponent<ChoiceFailMarker>();

                string msg = (choiceLine != null && !string.IsNullOrEmpty(choiceLine.ElseEffectsMessage))
                           ? choiceLine.ElseEffectsMessage
                           : "조건이 부족합니다.";
                dialogueUI.ShowFloatingHintAtSpeaker(choiceLine?.SpeakerId, msg);

                EffectRunner.Apply(choiceNode.ElseEffects, choiceLine); // 패널티 1회만
            }

            var t = selectedBtn.transform;
            t.DOShakePosition(0.25f, 12f, 18, 90f, false, true);
            t.DOShakeScale(0.25f, 0.2f);

            var argMap = ChoiceAnimUtil.ParseArgs(choiceNode.ChoiceArgs);
            var fx = selectedBtn.GetComponent<FailureFX>();
            if (fx) fx.PlayFail(argMap);

            return;
        }

        if (!string.IsNullOrEmpty(choiceNode.FlagTag))
        {
            foreach (var tag in choiceNode.FlagTag.Split(';'))
            {
                var t = tag.Trim();
                if (!string.IsNullOrEmpty(t)) GameState.I.SetFlag(t);
            }
        }

        string next = !string.IsNullOrEmpty(jump) ? jump : choiceNode.NextNodeId;
        if (string.IsNullOrEmpty(next)) { Debug.LogWarning($"NextNodeId 비어있음: {choiceNodeId}"); return; }

        dialogueUI.ClearChoices();
        string heroName = speakers.ContainsKey("hero") ? speakers["hero"].Name : "주인공";
        dialogueUI.ShowDialogue(heroName, label);

        _queuedNextNodeId = next;
        _awaitingAdvance = true;
        _pendingEnd = false;
        _endNeedsConfirm = false;
    }

    public void Next()
    {
        if (dialogueUI != null && dialogueUI.HasChoices()) return;

        if (dialogueUI != null && dialogueUI.IsTyping())
        {
            dialogueUI.CompleteTyping();
            return;
        }

        if (_pendingEnd)
        {
            if (_endNeedsConfirm)
            {
                _endNeedsConfirm = false;
                return;
            }
            Debug.Log("스토리 종료(END) → 맵 이동");
            _pendingEnd = false;
            return;
        }

        if (_awaitingAdvance && !string.IsNullOrEmpty(_queuedNextNodeId))
        {
            currentNodeId = _queuedNextNodeId;
            _awaitingAdvance = false;
            _queuedNextNodeId = null;
            ShowCurrentNode();
            return;
        }

        if (dialogueNodes.TryGetValue(currentNodeId, out var cur) &&
            (string.IsNullOrEmpty(cur.NodeType) || cur.NodeType.Equals("Dialogue")))
        {
            storyLines.TryGetValue(currentNodeId, out var line);

            string jump = EffectRunner.RunNodeChain(cur, line);
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

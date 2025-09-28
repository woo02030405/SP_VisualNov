using UnityEngine;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    private Dictionary<string, DialogueNode> dialogueNodes;
    private Dictionary<string, StoryLine> storyLines;
    private Dictionary<string, Speaker> speakers;

    [SerializeField] private string startNodeId = "S1_N001";
    private string currentNodeId;

    [Header("References")]
    public DialogueUI dialogueUI;

    private void Start()
    {
        if (dialogueUI == null)
        {
            Debug.LogError("[DialogueManager] dialogueUI 참조가 없음");
            enabled = false; return;
        }

        dialogueNodes = CSVLoader.LoadTable<DialogueNode>("Ch1_MainDialogue", "NodeId");
        storyLines = CSVLoader.LoadTable<StoryLine>("Ch1_MainStory_kr", "NodeId");
        speakers = CSVLoader.LoadTable<Speaker>("Speakers_kr", "SpeakerId");

        if (dialogueNodes == null || storyLines == null || speakers == null)
        {
            Debug.LogError("[DialogueManager] CSV 로드 실패");
            enabled = false; return;
        }

        dialogueUI.onClickNext = Next;

        currentNodeId = !string.IsNullOrEmpty(startNodeId) ? startNodeId : FindFirstNodeId();
        if (string.IsNullOrEmpty(currentNodeId))
        {
            Debug.LogError("[DialogueManager] 시작 노드 없음");
            enabled = false; return;
        }

        ShowCurrentNode();
    }

    private string FindFirstNodeId()
    {
        foreach (var kv in storyLines) return kv.Key;
        return null;
    }

    private void ShowCurrentNode()
    {
        if (!storyLines.ContainsKey(currentNodeId))
        {
            Debug.Log("스토리 종료 → 맵 이동");
            return;
        }

        var line = storyLines[currentNodeId];
        DialogueNode node = dialogueNodes.ContainsKey(currentNodeId) ? dialogueNodes[currentNodeId] : null;

        var spk = speakers.ContainsKey(line.SpeakerId) ? speakers[line.SpeakerId] : new Speaker { Name = "???" };
        string processed = (node != null) ? DialogueTextEffect.Apply(line.Text, node.TextEffect) : line.Text;

        dialogueUI.ShowDialogue(spk.Name, processed);

        // ChoiceGroup 기반으로 같은 묶음의 Choice 노드 모두 버튼 생성
        var choiceIds = CollectChoiceGroupByDialogue(currentNodeId);
        if (choiceIds.Count > 0)
        {
            var items = new List<(string, System.Action)>();
            foreach (var cid in choiceIds)
            {
                string label = storyLines.ContainsKey(cid) ? storyLines[cid].ChoiceText : cid;
                items.Add((label, () => SelectChoice(cid)));
            }
            dialogueUI.ShowChoices(items);
        }
    }

    // 현재 노드가 Choice면, 같은 Chapter/Day + ChoiceGroup의 모든 Choice 노드 모음
    private List<string> CollectChoiceGroupByDialogue(string currentId)
    {
        var list = new List<string>();
        if (!dialogueNodes.ContainsKey(currentId)) return list;

        var cur = dialogueNodes[currentId];

        bool isChoice = !string.IsNullOrEmpty(cur.NodeType) && cur.NodeType.StartsWith("Choice");
        if (!isChoice) return list;

        string group = cur.ChoiceGroup?.Trim();
        if (string.IsNullOrEmpty(group))
        {
            // 그룹 미지정 → 단일 선택지
            list.Add(currentId);
            return list;
        }

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

    private void SelectChoice(string choiceNodeId)
    {
        // TODO: Conditions/Effects/ElseIf.../SkipPenalty 처리

        if (!dialogueNodes.TryGetValue(choiceNodeId, out var choiceNode) || string.IsNullOrEmpty(choiceNode.NextNodeId))
        {
            Debug.LogWarning($"[DialogueManager] 선택 노드/Next 없음: {choiceNodeId}");
            return;
        }

        // 먼저 선택지 제거(HasChoices 가드 회피)
        dialogueUI.ClearChoices();

        currentNodeId = choiceNode.NextNodeId;
        ShowCurrentNode();
    }

    public void Next()
    {
        // 선택지가 떠 있으면 화면 클릭/엔터 무시 (버튼으로만)
        if (dialogueUI != null && dialogueUI.HasChoices()) return;

        if (!dialogueNodes.ContainsKey(currentNodeId)) { Debug.Log("다음 없음 → 맵"); return; }
        var node = dialogueNodes[currentNodeId];
        if (string.IsNullOrEmpty(node.NextNodeId)) { Debug.Log("스토리 종료 → 맵"); return; }

        currentNodeId = node.NextNodeId;
        ShowCurrentNode();
    }
}

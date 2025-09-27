using System.Collections.Generic;
using UnityEngine;
using VN.Dialogue;
using VN.IO;

namespace VN
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;       // 대사 UI
        [SerializeField] private ChoiceUIManager choiceUI;    // 선택지 UI

        private Dictionary<string, DialogueNode> nodeMap;     // 모든 노드 맵
        private DialogueNode currentNode;                     // 현재 노드

        /// <summary>
        /// CSV를 로드하고 첫 번째 노드(N001)부터 시작
        /// </summary>
        public void Init(string storyCsvPath, string dialogueCsvPath)
        {
            nodeMap = DialogueParser.LoadCsv(storyCsvPath, dialogueCsvPath);

            if (nodeMap.TryGetValue("N001", out var startNode))
                JumpToNode(startNode);
        }

        /// <summary>
        /// 지정한 노드로 이동
        /// </summary>
        private void JumpToNode(DialogueNode node)
        {
            if (node == null) return;

            Debug.Log($"[DialogueManager] Jump → {node.NodeId} ({node.NodeType})");

            currentNode = node;

            switch (node.NodeType)
            {
                case "Choice":
                    ShowChoices(node);
                    break;
                case "End":
                    ShowEnd(node);
                    break;
                default: // Dialogue가 기본값
                    ShowDialogue(node);
                    break;
            }
        }

        /// <summary>
        /// 일반 대사 노드 출력
        /// </summary>
        private void ShowDialogue(DialogueNode node)
        {
            dialogueUI.ShowLine(node, () =>
            {
                dialogueUI.ShowNextIndicator(() =>
                {
                    ContinueToNext(node);
                });
            });
        }

        /// <summary>
        /// 다음 노드로 진행
        /// </summary>
        private void ContinueToNext(DialogueNode node)
        {
            if (!string.IsNullOrEmpty(node.NextNodeId) && nodeMap.TryGetValue(node.NextNodeId, out var next))
            {
                JumpToNode(next);
            }
            else
            {
                string nextId = FindNextNodeId(node.NodeId);
                if (nextId != null && nodeMap.TryGetValue(nextId, out var nextNode))
                    JumpToNode(nextNode);
            }
        }

        /// <summary>
        /// 선택지 노드 출력 (같은 그룹의 Choice_xxx_1,2,3 …를 모아서 표시)
        /// </summary>
        private void ShowChoices(DialogueNode node)
        {
            var options = new List<ChoiceOption>();

            // Choice 그룹 구분자 찾기 (ex: Choice_001 → prefix: "Choice_001")
            string groupPrefix = node.NodeId;
            int lastUnderscore = groupPrefix.LastIndexOf('_');
            if (lastUnderscore > 0)
                groupPrefix = groupPrefix.Substring(0, lastUnderscore);

            foreach (var kv in nodeMap)
            {
                var n = kv.Value;

                // 같은 그룹에 속한 Choice만 모음
                if (n.NodeType == "Choice" && n.NodeId.StartsWith(groupPrefix))
                {
                    // 조건 확인
                    if (string.IsNullOrEmpty(n.Conditions) || EffectManager.CheckCondition(n.Conditions))
                    {
                        string style = string.IsNullOrEmpty(n.ChoiceStyle) ? "Default" : n.ChoiceStyle;
                        options.Add(new ChoiceOption(n.ChoiceText, n.NextNodeId, n.Conditions, style));
                    }
                }
            }

            if (options.Count > 0)
                choiceUI.ShowChoices(options, OnChoiceSelected);
            else
                Debug.LogWarning($"[DialogueManager] No valid choices for node {node.NodeId}");
        }

        /// <summary>
        /// 엔딩 처리
        /// </summary>
        private void ShowEnd(DialogueNode node)
        {
            Debug.Log($"[DialogueManager] End reached: {node.NodeId}");
            // TODO: 엔딩 팝업 UI 호출
        }

        /// <summary>
        /// 선택지가 클릭되었을 때 실행
        /// </summary>
        private void OnChoiceSelected(string nodeId)
        {
            if (!nodeMap.TryGetValue(nodeId, out var chosen))
            {
                Debug.LogError($"[DialogueManager] Invalid choice nodeId={nodeId}");
                return;
            }

            // 보상/패널티 적용
            EffectManager.ApplyNodeEffects(chosen, true);

            // 다음 노드로 이동
            if (!string.IsNullOrEmpty(chosen.NextNodeId) && nodeMap.TryGetValue(chosen.NextNodeId, out var next))
            {
                JumpToNode(next);
            }
            else
            {
                string nextId = FindNextNodeId(chosen.NodeId);
                if (nextId != null && nodeMap.TryGetValue(nextId, out var nextNode))
                    JumpToNode(nextNode);
            }
        }

        /// <summary>
        /// 현재 노드 이후의 첫 노드 ID를 찾음
        /// </summary>
        private string FindNextNodeId(string currentId)
        {
            bool found = false;
            foreach (var kv in nodeMap)
            {
                if (found) return kv.Key;
                if (kv.Key == currentId) found = true;
            }
            return null;
        }

        /// <summary>
        /// 스킵 시 패널티 적용
        /// </summary>
        public void ApplySkipPenalty(string nodeId)
        {
            if (nodeMap.TryGetValue(nodeId, out var node))
                EffectManager.ApplyNodeEffects(node, false);
        }
    }
}

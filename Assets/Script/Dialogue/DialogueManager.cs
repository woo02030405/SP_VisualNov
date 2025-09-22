using System;
using System.Collections.Generic;
using UnityEngine;
using VN.UI;
using VN.Rendering;

namespace VN.Dialogue
{
    public enum SkipMode
    {
        None,      // 0: 끄기
        ReadOnly,  // 1: 읽은 대사만
        All        // 2: 모든 대사
    }

    public class DialogueManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private ChoiceUIManager choiceUI;

        [Header("Managers")]
        [SerializeField] private StreamingResourceManager resourceManager;
        [SerializeField] private LogManager logManager;

        private Dictionary<string, DialogueNode> nodeMap;
        private DialogueNode current;
        private SaveData saveData;

        [Header("Player Settings")]
        public SkipMode skipMode = SkipMode.ReadOnly;
        public bool autoMode = false;
        public float autoDelay = 2f;

        // ========================
        // Initialization
        // ========================
        public void Initialize(
            List<Dictionary<string, string>> speakers,
            List<Dictionary<string, string>> story,
            List<Dictionary<string, string>> dialogue,
            List<Dictionary<string, string>> resourceCsv = null)
        {
            nodeMap = DialogueParser.Parse(story, dialogue, resourceCsv ?? new List<Dictionary<string, string>>());

            if (saveData == null)
                saveData = new SaveData();

            if (dialogueUI != null)
                dialogueUI.OnNextClicked += OnNextClicked;
        }

        // ========================
        // Navigation
        // ========================
        public void JumpTo(string nodeId)
        {
            GoToNode(nodeId);
        }

        public void GoToNode(string nodeId)
        {
            Debug.Log($"[DialogueManager] GoToNode({nodeId}) 호출");

            if (nodeMap != null && nodeMap.TryGetValue(nodeId, out DialogueNode node))
            {
                current = node;
                Debug.Log($"[DialogueManager] Node found: {node.Id}");
                DisplayNode(current);
            }
            else
            {
                Debug.LogError($"[DialogueManager] Node not found: {nodeId}");
            }
        }

        public void RefreshCurrentNode()
        {
            if (current != null)
                DisplayNode(current);
        }

        // ========================
        // Display
        // ========================
        private void DisplayNode(DialogueNode node)
        {
            if (node == null) return;

            // 로그 기록
            logManager?.AddEntry(node.SpeakerId, node.Text);

            // UI 표시
            if (dialogueUI != null)
            {
                dialogueUI.SetName(node.SpeakerId);
                dialogueUI.SetDialogue(node.Text);
                // 선택지가 없을 때만 NextIndicator 표시
                dialogueUI.ShowNextIndicator(node.Choices == null || node.Choices.Count == 0);
            }

            // 리소스 적용
            resourceManager?.Apply(node);

            // 선택지 처리
            if (node.Choices != null && node.Choices.Count > 0 && choiceUI != null)
            {
                choiceUI.ShowChoices(node.Choices, saveData, OnChoiceSelected);
                return; // 멈춤
            }

            // ❌ 여기서는 자동 이동 금지
            // 반드시 OnNextClicked에서만 다음 노드로 진행한다
        }

        // ========================
        // Input: Next 클릭
        // ========================
        private void OnNextClicked()
        {
            if (current == null) return;

            // 선택지가 열려있으면 무시
            if (current.Choices != null && current.Choices.Count > 0)
            {
                Debug.Log("[DialogueManager] Choice open, ignore next click.");
                return;
            }

            if (!string.IsNullOrEmpty(current.NextNodeId))
            {
                GoToNode(current.NextNodeId);
            }
            else
            {
                string nextId = FindNextNodeId(current.Id);
                if (!string.IsNullOrEmpty(nextId))
                    GoToNode(nextId);
                else
                    Debug.Log("[DialogueManager] 대화 종료");
            }
        }

        // ========================
        // Input: 선택지 클릭
        // ========================
        private void OnChoiceSelected(ChoiceOption option)
        {
            if (!string.IsNullOrEmpty(option.Effect))
                EffectProcessor.ApplyEffects(option.Effect, saveData);

            if (!string.IsNullOrEmpty(option.NextNodeId))
                GoToNode(option.NextNodeId);
        }

        // ========================
        // Helper: 다음 노드 찾기
        // ========================
        private string FindNextNodeId(string currentId)
        {
            if (nodeMap == null) return null;

            if (currentId.StartsWith("N"))
            {
                if (int.TryParse(currentId.Substring(1), out int num))
                {
                    string nextId = "N" + (num + 1).ToString("D3");
                    if (nodeMap.ContainsKey(nextId))
                        return nextId;
                }
            }

            return null;
        }
    }
}

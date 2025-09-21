using UnityEngine;
using System.Collections.Generic;
using VN.UI;
using DG.Tweening;

namespace VN.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private ChoiceUIManager choiceUIManager;
        [SerializeField] private EffectManager effectManager;
        [SerializeField] private DialogueBoxIntro dialogueBoxIntro;
        [SerializeField] private ChoicePanelIntro choicePanelIntro;

        private Dictionary<string, DialogueNode> nodeMap;
        private DialogueNode currentNode;
        private string lastSpeakerId = null;

        public void Load(Dictionary<string, DialogueNode> map, string startNode)
        {
            nodeMap = map;
            if (nodeMap.TryGetValue(startNode, out currentNode))
                ShowNode(currentNode);
            else
                Debug.LogError($"Start node {startNode} not found!");
        }

        private void ShowNode(DialogueNode node)
        {
            currentNode = node;

            // 대사창 연출
            if (dialogueBoxIntro != null && (lastSpeakerId == null || node.SpeakerId != lastSpeakerId))
            {
                dialogueBoxIntro.PlayIn();
            }

            // 이름/색상 + 대사 출력
            dialogueUI.SetName(node.SpeakerId);
            dialogueUI.SetDialogue(node.Text, SettingsManager.TextSpeed);
            lastSpeakerId = node.SpeakerId;

            dialogueUI.ClearNextEvent();
            CancelInvoke(nameof(AutoProceed));

            // 🔹 캐릭터 이미지 교체
            var charMgr = FindObjectOfType<CharacterManager>();
            if (charMgr != null)
            {
                charMgr.ShowCharacter(node.SpeakerId, node.CharacterType, node.Expression, node.Pose, node.CharacterPosition, node.CharacterEffect);
            }

            // 🔹 선택지 처리
            if (node.Choices != null && node.Choices.Count > 0)
            {
                choiceUIManager.ShowChoices(node.Choices, node.ChoiceAnimType, OnChoiceSelected);

                if (choicePanelIntro != null)
                {
                    choicePanelIntro.PlayIn();
                    var cg = choicePanelIntro.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    }
                }
            }
            else
            {
                dialogueUI.OnNextClicked += () =>
                {
                    if (!string.IsNullOrEmpty(node.NextNodeId) &&
                        nodeMap.TryGetValue(node.NextNodeId, out var next))
                    {
                        ShowNode(next);
                    }
                    else
                    {
                        HandleEnding();
                    }
                };

                if (SettingsManager.AutoModeEnabled)
                    Invoke(nameof(AutoProceed), SettingsManager.AutoDelay);
            }
        }

        private void AutoProceed()
        {
            if (!string.IsNullOrEmpty(currentNode.NextNodeId) &&
                nodeMap.TryGetValue(currentNode.NextNodeId, out var next))
            {
                ShowNode(next);
            }
            else
            {
                HandleEnding();
            }
        }

        private void OnChoiceSelected(string nextNodeId)
        {
            if (!string.IsNullOrEmpty(nextNodeId) && nodeMap.TryGetValue(nextNodeId, out var next))
                ShowNode(next);
            else
                HandleEnding();
        }

        private void HandleEnding()
        {
            var popup = FindObjectOfType<EndingPopup>();
            if (popup != null)
                popup.Show("엔딩", "게임이 끝났습니다!");
            else
                Debug.Log("게임 종료: 엔딩 팝업 없음");
        }
    }
}

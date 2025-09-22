using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace VN.UI
{
    public class DialogueUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup uiGroup;      // UI 전체를 묶는 캔버스 그룹
        [SerializeField] private TMP_Text nameText;        // 캐릭터 이름
        [SerializeField] private TMP_Text dialogueText;    // 대사
        [SerializeField] private GameObject nextIndicator; // ▶ 표시 (다음으로 넘길 수 있을 때만 보이게)

        public event Action OnNextClicked;

        private bool isHidden; // UI 숨김 상태
        private string fullText;

        // ========================
        // Speaker/Dialogue 표시
        // ========================
        public void SetName(string speakerId)
        {
            if (nameText != null)
            {
                nameText.text = LocalizationManager.GetSpeakerName(speakerId);
                nameText.color = LocalizationManager.GetSpeakerColor(speakerId);
            }
        }

        public void SetDialogue(string text)
        {
            fullText = text ?? "";
            if (dialogueText != null)
                dialogueText.text = fullText;

            // 기본적으로는 다음 인디케이터 ON
            ShowNextIndicator(true);
        }

        public void ShowNextIndicator(bool show)
        {
            if (nextIndicator != null)
                nextIndicator.SetActive(show);
        }

        // ========================
        // UI 표시/숨김
        // ========================
        public void ToggleUI()
        {
            if (isHidden) ShowUI();
            else HideUI();
        }

        public void HideUI()
        {
            isHidden = true;
            if (uiGroup != null)
            {
                uiGroup.alpha = 0f;
                uiGroup.interactable = false;
                uiGroup.blocksRaycasts = false;
            }
        }

        public void ShowUI()
        {
            isHidden = false;
            if (uiGroup != null)
            {
                uiGroup.alpha = 1f;
                uiGroup.interactable = true;
                uiGroup.blocksRaycasts = true;
            }
        }

        // ========================
        // 입력 처리
        // ========================
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isHidden) return; // 숨겨진 상태에서는 클릭 무효
            OnNextClicked?.Invoke();
        }

        private void Update()
        {
            if (isHidden) return;

            // 엔터키로 넘기기
            if (Input.GetKeyDown(KeyCode.Return))
                OnNextClicked?.Invoke();

            // 좌클릭으로 넘기기
            if (Input.GetMouseButtonDown(0))
                OnNextClicked?.Invoke();

            // 우클릭으로 UI 토글
            if (Input.GetMouseButtonDown(1))
                ToggleUI();
        }
    }
}

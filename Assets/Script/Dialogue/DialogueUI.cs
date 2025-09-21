using TMPro;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;

namespace VN.UI
{
    public class DialogueUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private GameObject nextIndicator;

        public event Action OnNextClicked;

        private Coroutine typingCoroutine;
        private bool isTyping;
        private string fullText;

        public void SetName(string speakerId)
        {
            // 이름 & 색상 동시 적용
            nameText.text = LocalizationManager.GetSpeakerName(speakerId);
            nameText.color = LocalizationManager.GetSpeakerColor(speakerId);
        }

        public void SetDialogue(string text, float speed = 0.05f)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            fullText = text;
            typingCoroutine = StartCoroutine(TypeText(fullText, speed));
        }

        IEnumerator TypeText(string text, float speed)
        {
            isTyping = true;
            dialogueText.text = "";
            if (nextIndicator) nextIndicator.SetActive(false);

            foreach (char c in text)
            {
                dialogueText.text += c;
                if (!SettingsManager.SkipMode)
                    yield return new WaitForSeconds(speed);
            }

            isTyping = false;
            if (nextIndicator) nextIndicator.SetActive(true);
        }

        public void ClearNextEvent()
        {
            OnNextClicked = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isTyping)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                dialogueText.text = fullText;
                isTyping = false;
                if (nextIndicator) nextIndicator.SetActive(true);
            }
            else
            {
                OnNextClicked?.Invoke();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnPointerClick(null);
            }
        }
    }
}

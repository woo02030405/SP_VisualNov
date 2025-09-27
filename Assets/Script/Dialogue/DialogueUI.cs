using TMPro;
using UnityEngine;
using VN.Dialogue;

namespace VN
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private RectTransform nextIndicator; // ▶ 아이콘 (NextBlink 붙어있음)

        private System.Action onContinue;

        void Awake()
        {
            if (nextIndicator != null)
                nextIndicator.gameObject.SetActive(false); // 기본 꺼두기
        }

        /// <summary>
        /// 대사 출력
        /// </summary>
        public void ShowLine(DialogueNode node, System.Action onComplete)
        {
            if (speakerNameText != null)
                speakerNameText.text = LocalizationManager.GetSpeakerName(node.SpeakerId);

            if (dialogueText != null)
                dialogueText.text = node.Text;

            onComplete?.Invoke();
        }

        /// <summary>
        /// ▶ 아이콘을 대사 끝 위치에 표시, 클릭/엔터 입력 대기
        /// </summary>
        public void ShowNextIndicator(System.Action onClick)
        {
            onContinue = onClick;

            if (nextIndicator != null && dialogueText != null)
            {
                nextIndicator.gameObject.SetActive(true);

                // 텍스트 끝 위치 계산
                float textWidth = dialogueText.preferredWidth;
                Vector3 basePos = dialogueText.rectTransform.position;

                // 오른쪽 끝 + 오프셋(15)
                Vector3 newPos = basePos + new Vector3(textWidth / 2f + 15f, -dialogueText.preferredHeight / 2f, 0f);
                nextIndicator.position = newPos;
            }
        }

        void Update()
        {
            if (onContinue != null && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)))
            {
                if (nextIndicator != null)
                    nextIndicator.gameObject.SetActive(false); // 깜빡임 종료

                var cb = onContinue;
                onContinue = null;
                cb?.Invoke(); // 다음 노드 실행
            }
        }
    }
}

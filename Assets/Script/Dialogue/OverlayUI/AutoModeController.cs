using UnityEngine;
using UnityEngine.EventSystems;
using Game.OverlayUI;

namespace Game.OverlayUI
{
    /// 오토 모드: 대사 길이에 비례해서 자동 Next
    public class AutoModeController : MonoBehaviour
    {
        public DialogueManager dialogueManager;
        public DialogueUI dialogueUI;

        [Header("Speed")]
        public float msPerChar = 28f;
        public float minDelay = 0.8f;
        public float maxDelay = 4.5f;

        bool _on;
        float _timer;

        public bool Toggle()
        {
            _on = !_on;
            _timer = 0f;
            Debug.Log($"[AutoMode] Toggle -> {(_on ? "ON" : "OFF")}");
            return _on;
        }

        void Update()
        {
            if (!_on || dialogueManager == null || dialogueUI == null) return;

            //  메뉴/팝업이 열려 있으면 자동 진행도 잠깐 멈춤
            if (UIBlocker.IsBlocked) return;

            //  커서가 UI 위에 있으면 자동 진행도 멈춤(원치 않으면 이 줄은 지워도 됨)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (dialogueUI.HasChoices()) return; // 선택 열려 있으면 대기
            if (dialogueUI.IsTyping()) return; // 타이핑 중이면 대기

            _timer += Time.deltaTime;

            var txt = dialogueUI.dialogueText; // DialogueUI에 TMP_Text 공개라고 가정
            int len = txt ? txt.text.Length : 20;
            float wait = Mathf.Clamp((len * msPerChar) / 1000f, minDelay, maxDelay);

            if (_timer >= wait)
            {
                _timer = 0f;
                dialogueManager.Next();
            }
        }
    }
}

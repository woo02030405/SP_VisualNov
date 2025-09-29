using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.OverlayUI
{
    /// 오토 모드: 대사 길이에 비례해서 자동 Next
    public class AutoModeController : MonoBehaviour
    {
        public DialogueManager dialogueManager;
        public DialogueUI dialogueUI;

        [Header("Speed")]
        [Tooltip("글자당 지연(ms)")]
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

        // 환경설정 UI에서 연결할 수 있는 속도 조절 메서드
        public void SetSpeedMultiplier(float multiplier)
        {
            multiplier = Mathf.Clamp(multiplier, 0.2f, 5f); // 최소 0.2배 ~ 최대 5배
            msPerChar = 28f / multiplier;
            minDelay = Mathf.Max(0.2f, 0.8f / multiplier);
            maxDelay = 4.5f / multiplier;
        }

        void Update()
        {
            if (!_on || dialogueManager == null || dialogueUI == null) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (dialogueUI.HasChoices()) return;
            if (dialogueUI.IsTyping()) return;

            _timer += Time.deltaTime;

            var txt = dialogueUI.dialogueText;
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

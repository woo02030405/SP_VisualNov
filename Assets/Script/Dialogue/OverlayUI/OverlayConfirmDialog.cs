using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Game.OverlayUI
{
    /// 공용 확인 팝업 (메시지 + 확인 + [취소])
    public class OverlayConfirmDialog : MonoBehaviour
    {
        [Header("Refs")]
        public CanvasGroup cg;          // 루트 CanvasGroup (페이드용)
        public RectTransform panel;     // 팝업 본체(스케일 인/아웃)
        public TMP_Text messageText;
        public Button okButton;
        public Button cancelButton;
        public GameObject cancelObj;    // 취소 버튼 오브젝트(옵션)

        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public System.Action onOk;
        [System.Diagnostics.CodeAnalysis.MaybeNull]
        public System.Action onCancel;

        void Awake()
        {
            if (!cg) cg = GetComponent<CanvasGroup>();
            if (cg) cg.alpha = 0f;
            if (panel) panel.localScale = Vector3.one * 0.9f;
        }

        void OnEnable()
        {
            // UIBlocker 활성화 (팝업 열렸을 때 뒤 클릭 방지)
            UIBlocker.Push();

            // 진입 애니메이션 재생
            if (cg) { cg.alpha = 0f; cg.DOFade(1f, 0.15f).SetUpdate(true); }
            if (panel)
            {
                panel.localScale = Vector3.one * 0.9f;
                panel.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        public void Setup(string msg, string okText = "확인", string cancelText = null)
        {
            if (messageText) messageText.text = msg ?? "";

            if (okButton)
            {
                var okLabel = okButton.GetComponentInChildren<TMP_Text>(true);
                if (okLabel) okLabel.text = string.IsNullOrEmpty(okText) ? "확인" : okText;
                okButton.onClick.AddListener(() => { onOk?.Invoke(); Close(); });
                if (!okButton.GetComponent<Game.OverlayUI.ButtonCuteFX>())
                    okButton.gameObject.AddComponent<Game.OverlayUI.ButtonCuteFX>();
            }

            bool hasCancel = !string.IsNullOrEmpty(cancelText);
            if (cancelObj) cancelObj.SetActive(hasCancel);

            if (hasCancel && cancelButton)
            {
                var cLabel = cancelButton.GetComponentInChildren<TMP_Text>(true);
                if (cLabel) cLabel.text = cancelText;
                cancelButton.onClick.AddListener(() => { onCancel?.Invoke(); Close(); });
                if (!cancelButton.GetComponent<Game.OverlayUI.ButtonCuteFX>())
                    cancelButton.gameObject.AddComponent<Game.OverlayUI.ButtonCuteFX>();
            }
        }

        public void Close()
        {
            // 퇴장 애니메이션 후 삭제
            if (cg) cg.DOFade(0f, 0.12f).SetUpdate(true);
            if (panel) panel.DOScale(0.9f, 0.12f).SetEase(Ease.InSine).SetUpdate(true);
            Destroy(gameObject, 0.13f);
        }

        void OnDestroy()
        {
            // UIBlocker 해제 (팝업 닫혔을 때 뒤 클릭 허용)
            UIBlocker.Pop();
        }
    }
}

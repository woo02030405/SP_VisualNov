using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Game.OverlayUI
{
    /// 화면 전체 Overlay + 중앙 다이얼로그
    public class OverlayConfirmDialog : MonoBehaviour
    {
        [Header("Refs")]
        public CanvasGroup cg;              // 루트 CanvasGroup (페이드/차단)
        public Image background;            // 전체 배경(반투명, RaycastTarget=ON)
        public RectTransform panel;         // 중앙 패널
        public TMP_Text messageText;
        public Button okButton;
        public Button cancelButton;
        public GameObject cancelObj;

        public System.Action onOk;
        public System.Action onCancel;

        [Header("Anim")]
        public float fadeIn = 0.15f;
        public float fadeOut = 0.12f;
        public float scaleIn = 0.20f;

        void Reset() { cg = GetComponent<CanvasGroup>(); }

        void Awake()
        {
            if (!cg) cg = GetComponent<CanvasGroup>();
            if (cg) { cg.alpha = 0f; cg.blocksRaycasts = true; }   // 열릴 때 차단 켬
            if (panel) panel.localScale = Vector3.one * 0.9f;
            if (background) background.raycastTarget = true;       // 뒤 클릭 막기

            // 안전망(Stretch 보정)
            var root = GetComponent<RectTransform>();
            if (root) { root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero; }
            if (background)
            {
                var bgRt = background.rectTransform;
                bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            }
            if (panel)
            {
                panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
                panel.anchoredPosition = Vector2.zero;
            }
        }

        void OnEnable()
        {
            // 필요 시 전역 블로커 사용한다면 주석 해제
            // UIBlocker.Push();

            if (cg) { cg.alpha = 0f; cg.DOFade(1f, fadeIn).SetUpdate(true); }
            if (panel)
                panel.DOScale(1f, scaleIn).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void Setup(string msg, string okText = "확인", string cancelText = null)
        {
            if (messageText) messageText.text = msg ?? string.Empty;

            // OK
            if (okButton)
            {
                var t = okButton.GetComponentInChildren<TMP_Text>(true);
                if (t) t.text = string.IsNullOrEmpty(okText) ? "확인" : okText;
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(() => { onOk?.Invoke(); Close(); });
            }

            // Cancel
            bool hasCancel = !string.IsNullOrEmpty(cancelText);
            if (cancelObj) cancelObj.SetActive(hasCancel);
            if (hasCancel && cancelButton)
            {
                var t = cancelButton.GetComponentInChildren<TMP_Text>(true);
                if (t) t.text = cancelText;
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(() => { onCancel?.Invoke(); Close(); });
            }
        }

        public void Close()
        {
            // ★ 즉시 레이캐스트 해제 (투명막 남는 이슈 방지)
            if (cg) cg.blocksRaycasts = false;
            if (background) background.raycastTarget = false;

            // 전역 블로커 사용한다면 안전하게 Pop
            try { UIBlocker.Pop(); } catch { }

            // 퇴장 연출 후 파괴
            if (cg) cg.DOFade(0f, fadeOut).SetUpdate(true);
            if (panel) panel.DOScale(0.9f, fadeOut).SetEase(Ease.InSine).SetUpdate(true);
            Destroy(gameObject, fadeOut + 0.02f);
        }

        void OnDestroy()
        {
            // 혹시 Close 전에 파괴될 수도 있으니 한 번 더
            try { UIBlocker.Pop(); } catch { }
        }
    }
}

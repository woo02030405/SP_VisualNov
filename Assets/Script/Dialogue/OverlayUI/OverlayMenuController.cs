using UnityEngine;
using UnityEngine.UI;

namespace Game.OverlayUI
{
    /// 우상단 메뉴 + 전체화면 Confirm 다이얼로그 호출
    public class OverlayMenuController : MonoBehaviour
    {
        [Header("Panel (in-scene)")]
        public GameObject panel;            // 메뉴 패널(씬 오브젝트)
        public Button resumeBtn;
        public Button settingsBtn;
        public Button toTitleBtn;
        public Button quitBtn;

        [Header("Dialogs")]
        public OverlayConfirmDialog confirmPrefab;
        public Transform dialogLayer;       // 루트 Canvas 하위 풀스크린 레이어

        [Header("Overlay (optional)")]
        public GameObject blocker;          // 메뉴 열렸을 때만 쓰는 차단막

        [Header("Auto-Find")]
        public string panelObjectName = "MenuPanel";

        void Awake()
        {
            EnsurePanel();

            if (panel) panel.SetActive(false);
            if (resumeBtn) resumeBtn.onClick.AddListener(() => Toggle(false));
            if (settingsBtn) settingsBtn.onClick.AddListener(OpenSettings);
            if (toTitleBtn) toTitleBtn.onClick.AddListener(ConfirmToTitle);
            if (quitBtn) quitBtn.onClick.AddListener(ConfirmQuit);

            if (blocker) blocker.SetActive(false);
        }

        void EnsurePanel()
        {
            if (panel) return;

            if (!string.IsNullOrEmpty(panelObjectName))
            {
                var t = transform.Find(panelObjectName);
                if (t) panel = t.gameObject;
            }
            if (!panel && !string.IsNullOrEmpty(panelObjectName))
            {
                foreach (var c in GetComponentsInChildren<RectTransform>(true))
                    if (c.gameObject.name == panelObjectName) { panel = c.gameObject; break; }
                if (!panel)
                {
                    foreach (var p in GetComponentsInParent<RectTransform>(true))
                    {
                        var t = p.Find(panelObjectName);
                        if (t) { panel = t.gameObject; break; }
                    }
                }
            }
            if (!panel)
                Debug.LogWarning("[OverlayMenuController] panel 미지정. 인스펙터에서 MenuPanel을 지정하세요.");
        }

        public void Toggle() => Toggle(panel && !panel.activeSelf);

        public void Toggle(bool on)
        {
            EnsurePanel();
            if (!panel) return;

            panel.SetActive(on);
            Time.timeScale = on ? 0f : 1f;
            if (blocker) blocker.SetActive(on);

            Debug.Log($"[Menu] {(on ? "OPEN" : "CLOSE")}");
        }

        void OnDisable()
        {
            Time.timeScale = 1f;
            if (blocker) blocker.SetActive(false);
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
            if (blocker) blocker.SetActive(false);
        }

        // ====== Actions ======

        void OpenSettings()
        {
            var dlg = Spawn("환경설정은 곧 추가됩니다.", "확인", null);
            if (dlg) dlg.onOk = () => { /* 메뉴는 그대로 열린다 */ };
        }

        void ConfirmToTitle()
        {
            var dlg = Spawn("타이틀로 돌아갈까요?\n저장되지 않은 진행은 사라집니다.", "예", "아니오");
            if (!dlg) return;

            // 메뉴를 닫지 않는다 (요구사항)
            dlg.onOk = () =>
            {
                Time.timeScale = 1f;
                Debug.Log("[Menu] 타이틀로 이동");
                // Scene 이동 로직을 여기에…
            };
            dlg.onCancel = () =>
            {
                // 아무 것도 하지 않음 — 메뉴 유지
            };
        }

        void ConfirmQuit()
        {
            var dlg = Spawn("게임을 종료할까요?\n저장되지 않은 진행은 사라집니다.", "종료", "취소");
            if (!dlg) return;

            dlg.onOk = () =>
            {
                Time.timeScale = 1f;
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            };
            dlg.onCancel = () => { /* 메뉴 유지 */ };
        }

        // ====== Core: Always full-screen on Root Canvas ======

        OverlayConfirmDialog Spawn(string msg, string ok = "확인", string cancel = null)
        {
            if (!confirmPrefab)
            {
                Debug.LogError("[OverlayMenuController] confirmPrefab 미지정");
                return null;
            }

            // 1) 루트 Canvas 확보
            var rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            if (!rootCanvas)
            {
                Debug.LogError("[OverlayMenuController] Root Canvas를 찾을 수 없습니다.");
                return null;
            }

            // 2) 다이얼로그 레이어 선택/생성(풀스크린 Stretch)
            Transform parent = dialogLayer;
            if (!parent)
            {
                var found = rootCanvas.transform.Find("DialogLayer");
                if (found) parent = found;
                else
                {
                    var go = new GameObject("DialogLayer", typeof(RectTransform));
                    parent = go.transform;
                    parent.SetParent(rootCanvas.transform, false);

                    var rt = (RectTransform)parent;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
            }

            // 3) 인스턴스 생성 + 부모 지정
            var d = Instantiate(confirmPrefab);
            d.transform.SetParent(parent, false);

            // 4) 루트 RectTransform을 화면 전체로 고정
            var rect = d.GetComponent<RectTransform>();
            if (rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }

            // 5) 배경은 반드시 차단
            if (d.background) d.background.raycastTarget = true;

            d.gameObject.SetActive(true);
            d.Setup(msg, ok, cancel);
            return d;
        }
    }
}

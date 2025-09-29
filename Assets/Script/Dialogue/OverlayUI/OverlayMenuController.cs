using UnityEngine;
using UnityEngine.UI;

namespace Game.OverlayUI
{
    public class OverlayMenuController : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;              // 씬의 MenuPanel (필수)
        public Button resumeBtn;
        public Button settingsBtn;
        public Button toTitleBtn;
        public Button quitBtn;

        [Header("Dialogs")]
        public OverlayConfirmDialog confirmPrefab;
        public Transform dialogLayer;

        [Header("Overlay")]
        [Tooltip("씬에 있는 Blocker 오브젝트를 드래그해서 연결하세요.")]
        public GameObject blocker;

        [Header("Auto-Find (Optional)")]
        [Tooltip("panel이 비었을 때 이 이름으로 자식/부모 쪽에서 MenuPanel을 탐색합니다.")]
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

        // panel이 비었거나 Missing이면 자동으로 찾아봄
        void EnsurePanel()
        {
            if (panel != null) return;

            if (!string.IsNullOrEmpty(panelObjectName))
            {
                var t = transform.Find(panelObjectName);
                if (t) panel = t.gameObject;
            }

            if (panel == null && !string.IsNullOrEmpty(panelObjectName))
            {
                var candidates = GetComponentsInChildren<RectTransform>(true);
                foreach (var c in candidates)
                {
                    if (c.gameObject.name == panelObjectName) { panel = c.gameObject; break; }
                }
                if (panel == null)
                {
                    var parents = GetComponentsInParent<RectTransform>(true);
                    foreach (var p in parents)
                    {
                        var t = p.Find(panelObjectName);
                        if (t) { panel = t.gameObject; break; }
                    }
                }
            }

            if (panel == null)
                Debug.LogWarning("[OverlayMenuController] panel이 비어있습니다. 인스펙터에서 MenuPanel(씬 오브젝트)을 지정하세요.");
        }

        public void Toggle() => Toggle(panel != null && !panel.activeSelf);

        public void Toggle(bool on)
        {
            EnsurePanel();
            if (!panel)
            {
                Debug.LogWarning("[OverlayMenuController] panel이 없어 메뉴를 열 수 없습니다.");
                return;
            }

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

        void OpenSettings()
        {
            var dlg = Spawn("환경설정은 곧 추가됩니다.", "확인", null);
            dlg.onOk = () => { };
        }

        void ConfirmToTitle()
        {
            var dlg = Spawn("타이틀로 돌아갈까요?\n저장되지 않은 진행은 사라집니다.", "예", "아니오");
            dlg.onOk = () =>
            {
                Time.timeScale = 1f;
                // TODO: SceneNavigator.GoTo("MainMenuScene");
                Debug.Log("[Menu] 타이틀로 이동");
            };
        }

        void ConfirmQuit()
        {
            var dlg = Spawn("게임을 종료할까요?\n저장되지 않은 진행은 사라집니다.", "종료", "취소");
            dlg.onOk = () =>
            {
                Time.timeScale = 1f;
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            };
        }

        OverlayConfirmDialog Spawn(string msg, string ok = "확인", string cancel = null)
        {
            if (!confirmPrefab)
            {
                Debug.LogWarning("[OverlayMenuController] confirmPrefab이 비어 있습니다.");
                return null;
            }

            var parent = dialogLayer ? dialogLayer : transform.parent;
            var d = Instantiate(confirmPrefab);
            d.transform.SetParent(parent, false);

            d.gameObject.SetActive(true);
            d.Setup(msg, ok, cancel);
            return d;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Game.OverlayUI
{
    public class BacklogPanelController : MonoBehaviour
    {
        public static BacklogPanelController I;

        [Header("UI")]
        public CanvasGroup cg;                 // BacklogPanel 루트
        public GameObject root;                // BacklogPanel (this.gameObject)
        public ScrollRect scrollRect;
        public RectTransform content;          // ScrollRect Content
        public GameObject logEntryPrefab;      // LogEntry_Prefab

        [Header("Options")]
        public bool closeOnRightClick = true;
        public KeyCode closeKey = KeyCode.Escape;
        public bool pauseDialogueInputWhileOpen = true;

        readonly List<(string speaker, string line)> logs = new();

        void Awake()
        {
            I = this;
            if (!root) root = gameObject;
            if (!cg) cg = GetComponent<CanvasGroup>();
            HideImmediate();
        }

        void Update()
        {
            if (!root.activeSelf) return;

            if (closeOnRightClick && Input.GetMouseButtonDown(1)) Close();
            if (closeKey != KeyCode.None && Input.GetKeyDown(closeKey)) Close();
        }

        // ==== Public API ====

        public void Open()
        {
            if (pauseDialogueInputWhileOpen) UIBlocker.Push(); // 선택: 대사 입력 막기 (없으면 주석)
            root.SetActive(true);
            if (cg) { cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true; }
            ScrollToBottom();
        }

        public void Close()
        {
            if (pauseDialogueInputWhileOpen) try { UIBlocker.Pop(); } catch { }
            if (cg) { cg.alpha = 0f; cg.blocksRaycasts = false; cg.interactable = false; }
            root.SetActive(false);
        }

        public void Toggle()
        {
            if (root.activeSelf) Close();
            else Open();
        }

        public void AddLog(string speaker, string line)
        {
            logs.Add((speaker, line));

            if (!logEntryPrefab || !content)
            {
                Debug.LogWarning("[Backlog] Prefab/Content 미연결. 데이터만 저장됨.");
                return;
            }

            var go = Instantiate(logEntryPrefab, content);
            var ui = go.GetComponent<LogEntryUI>();
            if (ui) ui.Setup(speaker, line);
            else
            {
                // 프리팹에 스크립트 안 붙였을 때도 방어
                var texts = go.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in texts)
                {
                    if (t.gameObject.name.ToLower().Contains("speaker")) t.text = speaker;
                    else if (t.gameObject.name.ToLower().Contains("line")) t.text = line;
                }
            }

            // 레이아웃 업데이트 후 맨 아래로
            Canvas.ForceUpdateCanvases();
            ScrollToBottom();
        }

        public void Clear()
        {
            foreach (Transform child in content) Destroy(child.gameObject);
            logs.Clear();
            Canvas.ForceUpdateCanvases();
            ScrollToBottom();
        }

        // ==== Helpers ====

        void ScrollToBottom()
        {
            if (!scrollRect) return;
            // Canvases 강제 반영 후 스크롤
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        void HideImmediate()
        {
            if (cg) { cg.alpha = 0f; cg.blocksRaycasts = false; cg.interactable = false; }
            root.SetActive(false);
        }
    }
}

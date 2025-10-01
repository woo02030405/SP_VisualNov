// BacklogController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Game.OverlayUI
{
    public class BacklogController : MonoBehaviour
    {
        public static BacklogController I;

        [Header("UI")]
        [SerializeField] private GameObject logEntryPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private ScrollRect scrollRect;

        private List<string> logs = new List<string>();

        private void Awake() { I = this; }

        public void AddLog(string speaker, string line)
        {
            string fullText = string.IsNullOrEmpty(speaker) ? line : $"{speaker}: {line}";
            logs.Add(fullText);

            var entry = Instantiate(logEntryPrefab, content);
            var ui = entry.GetComponent<LogEntryUI>();
            ui.Setup(speaker, line);

            // 맨 아래로 스크롤
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}

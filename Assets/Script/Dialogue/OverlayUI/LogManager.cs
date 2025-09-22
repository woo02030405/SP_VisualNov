using System.Collections.Generic;
using UnityEngine;

namespace VN.UI
{
    [System.Serializable]
    public class LogEntry
    {
        public string speaker;
        public string text;

        public LogEntry(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }

    public class LogManager : MonoBehaviour
    {
        public static LogManager Instance { get; private set; }

        private const int MaxEntries = 30;
        private List<LogEntry> logEntries = new();

        private bool dirty = false; // 로그 갱신 필요 여부
        public bool IsDirty => dirty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

        }

        public void AddEntry(string speaker, string text)
        {
            logEntries.Add(new LogEntry(speaker, text));

            // 최대 개수 초과 → 오래된 것 삭제
            if (logEntries.Count > MaxEntries)
                logEntries.RemoveAt(0);

            dirty = true;
        }

        public List<LogEntry> GetEntries()
        {
            return logEntries;
        }

        public void Clear()
        {
            logEntries.Clear();
            dirty = true;
        }

        // UI 갱신 후 dirty 해제
        public void MarkClean()
        {
            dirty = false;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogue
{
    public class BacklogManager : MonoBehaviour
    {
        public static BacklogManager Instance { get; private set; }

        private readonly List<BacklogEntry> logs = new List<BacklogEntry>();
        private const int MaxCount = 30;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddLog(string speaker, string text, AudioClip voice = null)
        {
            Debug.Log($"[Backlog/Add] '{speaker}' : '{text}' (voice={(voice ? "Y" : "N")})");

            if (string.IsNullOrWhiteSpace(text)) return;

            logs.Add(new BacklogEntry(speaker, text, voice));

            if (logs.Count > MaxCount)
                logs.RemoveAt(0);
        }

        public IReadOnlyList<BacklogEntry> GetLogs() => logs;
    }

    [System.Serializable]
    public class BacklogEntry
    {
        public string Speaker;
        public string Text;
        public AudioClip VoiceClip;

        public BacklogEntry(string speaker, string text, AudioClip voice)
        {
            Speaker = speaker;
            Text = text;
            VoiceClip = voice;
        }
    }
}

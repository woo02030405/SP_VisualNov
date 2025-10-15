using System.Collections.Generic;
using UnityEngine;

public class BacklogManager : MonoBehaviour
{
    public static BacklogManager Instance;
    private List<BacklogEntry> backlogList = new List<BacklogEntry>();
    private const int MaxLogCount = 30; // 최근 30줄까지만 저장

    private void Awake()
    {
        Instance = this;
    }

    public void AddLog(string speaker, string content, AudioClip voice)
    {
        backlogList.Add(new BacklogEntry(speaker, content, voice));
        if (backlogList.Count > MaxLogCount)
            backlogList.RemoveAt(0);
    }

    public List<BacklogEntry> GetLogs()
    {
        return backlogList;
    }
}

[System.Serializable]
public class BacklogEntry
{
    public string Speaker;
    public string Content;
    public AudioClip VoiceClip;

    public BacklogEntry(string speaker, string content, AudioClip voice)
    {
        Speaker = speaker;
        Content = content;
        VoiceClip = voice;
    }
}

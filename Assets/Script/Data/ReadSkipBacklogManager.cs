using System;
using System.Collections.Generic;
using UnityEngine;
using VN.SaveSystem;

[DisallowMultipleComponent]
public class ReadSkipBacklogManager : MonoBehaviour
{
    public static ReadSkipBacklogManager Instance { get; private set; }

    private readonly HashSet<string> _readNodes = new();
    private readonly List<BacklogEntry> _backlog = new();
    public int backlogLimit = 200;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 읽음 처리
    public bool IsRead(string nodeId) => !string.IsNullOrEmpty(nodeId) && _readNodes.Contains(nodeId);

    public void MarkRead(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;
        _readNodes.Add(nodeId);
    }

    // CSV Skipping 셀(“1”/“0”/빈칸)로 초기 읽음 플래그 세팅
    public void SetInitialReadByCsv(string nodeId, string skippingCell)
    {
        if (string.IsNullOrEmpty(nodeId)) return;
        if (!string.IsNullOrEmpty(skippingCell) && skippingCell.Trim() == "1")
            _readNodes.Add(nodeId);
    }

    // ── 백로그
    public void AddBacklog(string nodeId, string speaker, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        _backlog.Add(new BacklogEntry
        {
            nodeId = nodeId,
            speaker = speaker ?? "",
            text = text,
            time = DateTime.Now.ToString("HH:mm:ss")
        });
        if (_backlog.Count > Math.Max(10, backlogLimit))
            _backlog.RemoveRange(0, _backlog.Count - backlogLimit);
    }

    public IReadOnlyList<BacklogEntry> GetBacklog() => _backlog;

    // ── 세이브 연결
    public void ExportToSave(StoryData s)
    {
        if (s == null) return;
        s.readNodes = new List<string>(_readNodes);
        s.backlog = new List<BacklogEntry>(_backlog);
        s.backlogLimit = backlogLimit;
    }

    public void ImportFromSave(StoryData s)
    {
        _readNodes.Clear();
        _backlog.Clear();
        if (s == null) return;

        if (s.readNodes != null)
            foreach (var id in s.readNodes) if (!string.IsNullOrEmpty(id)) _readNodes.Add(id);

        if (s.backlog != null) _backlog.AddRange(s.backlog);

        backlogLimit = (s.backlogLimit <= 0) ? 200 : s.backlogLimit;
        if (_backlog.Count > backlogLimit)
            _backlog.RemoveRange(0, _backlog.Count - backlogLimit);
    }
}

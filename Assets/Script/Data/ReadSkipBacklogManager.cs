using System;
using System.Collections.Generic;
using UnityEngine;

public class ReadSkipBacklogManager : MonoBehaviour
{
    public static ReadSkipBacklogManager Instance { get; private set; }

    [Serializable]
    public class BacklogLine
    {
        public string nodeId;
        public string speaker;
        public string text;
        public DateTime time;
    }

    [Header("Backlog")]
    [SerializeField] private int maxBacklog = 200;

    private readonly HashSet<string> _read = new(StringComparer.Ordinal);
    private readonly List<BacklogLine> _backlog = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // CSV Skipping 초기 반영
    public void SetInitialReadByCsv(string nodeId, string cell)
    {
        if (string.IsNullOrEmpty(nodeId) || string.IsNullOrEmpty(cell)) return;
        if (cell.Trim() == "1") _read.Add(nodeId);
    }

    // 읽음/백로그
    public bool IsRead(string nodeId) => !string.IsNullOrEmpty(nodeId) && _read.Contains(nodeId);
    public void MarkRead(string nodeId) { if (!string.IsNullOrEmpty(nodeId)) _read.Add(nodeId); }
    public void AddBacklog(string nodeId, string speaker, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        _backlog.Add(new BacklogLine
        {
            nodeId = nodeId ?? "",
            speaker = speaker ?? "",
            text = text,
            time = DateTime.Now
        });
        if (_backlog.Count > maxBacklog)
            _backlog.RemoveRange(0, _backlog.Count - maxBacklog);
    }

    // 저장/로드: VN.SaveSystem.StoryData에 직접 직렬화
    public void ExportToSave(VN.SaveSystem.StoryData story)
    {
        if (story == null) return;

        story.readNodes ??= new List<string>();
        story.backlog ??= new List<VN.SaveSystem.BacklogEntry>();
        story.backlogLimit = Mathf.Max(1, maxBacklog);

        story.readNodes.Clear();
        story.readNodes.AddRange(_read);

        story.backlog.Clear();
        foreach (var b in _backlog)
        {
            story.backlog.Add(new VN.SaveSystem.BacklogEntry
            {
                nodeId = b.nodeId ?? "",
                speaker = b.speaker ?? "",
                text = b.text ?? "",
                time = b.time.ToString("HH:mm:ss")
            });
        }
        if (story.backlog.Count > story.backlogLimit)
            story.backlog.RemoveRange(0, story.backlog.Count - story.backlogLimit);
    }

    public void ImportFromSave(VN.SaveSystem.StoryData story)
    {
        _read.Clear();
        _backlog.Clear();
        if (story == null) return;

        if (story.readNodes != null)
            foreach (var id in story.readNodes)
                if (!string.IsNullOrEmpty(id)) _read.Add(id);

        if (story.backlog != null)
        {
            foreach (var e in story.backlog)
            {
                if (e == null) continue;

                // "HH:mm:ss"만 저장 → 오늘 날짜 기준으로 보정
                DateTime t = DateTime.Now.Date;
                if (!string.IsNullOrEmpty(e.time))
                {
                    var parts = e.time.Split(':');
                    try
                    {
                        if (parts.Length >= 2)
                        {
                            int hh = int.Parse(parts[0]);
                            int mm = int.Parse(parts[1]);
                            int ss = (parts.Length >= 3) ? int.Parse(parts[2]) : 0;
                            t = t.AddHours(hh).AddMinutes(mm).AddSeconds(ss);
                        }
                    }
                    catch { t = DateTime.Now; }
                }

                _backlog.Add(new BacklogLine
                {
                    nodeId = e.nodeId ?? "",
                    speaker = e.speaker ?? "",
                    text = e.text ?? "",
                    time = t
                });
            }
        }

        maxBacklog = (story.backlogLimit > 0) ? story.backlogLimit : maxBacklog;
        if (_backlog.Count > maxBacklog)
            _backlog.RemoveRange(0, _backlog.Count - maxBacklog);
    }

    // 조회 헬퍼
    public IReadOnlyList<BacklogLine> GetBacklog() => _backlog;
    public int GetBacklogCount() => _backlog.Count;
    public void ClearBacklog() => _backlog.Clear();
}

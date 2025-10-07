using System.Collections.Generic;
using UnityEngine;
using VN.SaveSystem;

public class ProgressProxy : MonoBehaviour
{
    [Header("Flags (key → bool)")]
    public List<string> keys = new();
    public List<bool> values = new();

    [Header("World (optional)")]
    public int day;
    public string timeSlot;
    public string currentMapId;

    public void PullFromGame()
    {
        // TODO: Flag/Quest/CGOpen 등에서 현재 값 읽어오기
        // TODO: day/timeSlot/currentMapId 읽어오기
    }

    public void PushToGame()
    {
        // TODO: 시스템에 값 적용
    }

    public void ExportTo(GameStatePayload payload)
    {
        payload.flags.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            if (!string.IsNullOrEmpty(keys[i]))
                payload.flags.Add(new FlagEntry { key = keys[i], value = i < values.Count && values[i] });
        }
        payload.day = day;
        payload.timeSlot = timeSlot;
        payload.currentMapId = currentMapId;
    }

    public void ImportFrom(GameStatePayload payload)
    {
        keys.Clear(); values.Clear();
        if (payload.flags != null)
        {
            foreach (var f in payload.flags) { keys.Add(f.key); values.Add(f.value); }
        }
        day = payload.day;
        timeSlot = payload.timeSlot;
        currentMapId = payload.currentMapId;
    }
}

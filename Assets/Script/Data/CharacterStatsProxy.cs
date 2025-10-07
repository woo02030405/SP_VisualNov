using System.Collections.Generic;
using UnityEngine;
using VN.SaveSystem;

public class CharacterStatsProxy : MonoBehaviour
{
    [Header("Stat keys & values")]
    public List<string> keys = new();
    public List<int> values = new();

    // 실제 Stat 시스템과 동기화
    public void PullFromGame()
    {
        // TODO: StatManager에서 읽어오기
    }

    public void PushToGame()
    {
        // TODO: StatManager로 쓰기
    }

    public void ExportTo(List<StatEntry> outList)
    {
        outList.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            if (!string.IsNullOrEmpty(keys[i]))
                outList.Add(new StatEntry { key = keys[i], value = i < values.Count ? values[i] : 0 });
        }
    }

    public void ImportFrom(List<StatEntry> inList)
    {
        keys.Clear(); values.Clear();
        if (inList == null) return;
        foreach (var s in inList) { keys.Add(s.key); values.Add(s.value); }
    }
}

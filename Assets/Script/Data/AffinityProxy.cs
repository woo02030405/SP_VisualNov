using System.Collections.Generic;
using UnityEngine;
using VN.SaveSystem;

public class AffinityProxy : MonoBehaviour
{
    [Header("NPC id & affinity (to player)")]
    public List<string> npcIds = new();
    public List<int> values = new();

    public void PullFromGame()
    {
        // TODO: Affinity 시스템에서 읽기
    }

    public void PushToGame()
    {
        // TODO: Affinity 시스템에 쓰기
    }

    public void ExportTo(List<AffinityEntry> outList)
    {
        outList.Clear();
        for (int i = 0; i < npcIds.Count; i++)
        {
            if (!string.IsNullOrEmpty(npcIds[i]))
                outList.Add(new AffinityEntry { npc = npcIds[i], value = i < values.Count ? values[i] : 0 });
        }
    }

    public void ImportFrom(List<AffinityEntry> inList)
    {
        npcIds.Clear(); values.Clear();
        if (inList == null) return;
        foreach (var a in inList) { npcIds.Add(a.npc); values.Add(a.value); }
    }
}

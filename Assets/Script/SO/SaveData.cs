using System;
using System.Collections.Generic;

[System.Serializable] public class AffinityEntry { public string key; public int value; }
[System.Serializable] public class RelationEntry { public string key; public int value; }
[System.Serializable] public class FlagEntry { public string key; public bool value; }
[System.Serializable] public class ItemEntry { public string key; public int value; }
[System.Serializable] public class StatEntry { public string key; public int value; }

[System.Serializable]
public class SaveData
{
    // 기본 메타데이터
    public string title;
    public string dateTime;
    public string thumbnailPath;

    // 진행 상황
    public string chapterId;
    public string nodeId;

    // 자원
    public int gold = 0;

    // 데이터 저장소 (직렬화 대상)
    public List<AffinityEntry> affinityList = new();
    public List<RelationEntry> relationList = new();
    public List<FlagEntry> flagList = new();
    public List<ItemEntry> itemList = new();
    public List<StatEntry> statList = new();

    // 런타임 캐시 (빠른 조회용)
    private Dictionary<string, int> affinityCache = new();
    private Dictionary<string, int> relationCache = new();
    private Dictionary<string, bool> flagCache = new();
    private Dictionary<string, int> itemCache = new();
    private Dictionary<string, int> statCache = new();

    // 갤러리
    public HashSet<string> unlockedCG = new();
    public HashSet<string> unlockedEvents = new();

    public bool IsCGUnlocked(string id) => unlockedCG.Contains(id);
    public void UnlockCG(string id) => unlockedCG.Add(id);

    public bool IsEventUnlocked(string id) => unlockedEvents.Contains(id);
    public void UnlockEvent(string id) => unlockedEvents.Add(id);

    // ======================
    // 캐시 동기화
    // ======================
    public void RebuildCache()
    {
        affinityCache.Clear();
        relationCache.Clear();
        flagCache.Clear();
        itemCache.Clear();
        statCache.Clear();

        foreach (var e in affinityList) affinityCache[e.key] = e.value;
        foreach (var e in relationList) relationCache[e.key] = e.value;
        foreach (var e in flagList) flagCache[e.key] = e.value;
        foreach (var e in itemList) itemCache[e.key] = e.value;
        foreach (var e in statList) statCache[e.key] = e.value;
    }

    // ======================
    // 편의 메서드
    // ======================

    // --- Affinity ---
    public int GetAffinity(string key, int defaultValue = 0)
        => affinityCache.TryGetValue(key, out int v) ? v : defaultValue;

    public void AddAffinity(string key, int amount)
    {
        if (affinityCache.TryGetValue(key, out int v))
        {
            v += amount;
            affinityCache[key] = v;
            var entry = affinityList.Find(e => e.key == key);
            if (entry != null) entry.value = v;
        }
        else
        {
            affinityCache[key] = amount;
            affinityList.Add(new AffinityEntry { key = key, value = amount });
        }
    }

    // --- Relation ---
    public int GetRelation(string key, int defaultValue = 0)
        => relationCache.TryGetValue(key, out int v) ? v : defaultValue;

    public void AddRelation(string key, int amount)
    {
        if (relationCache.TryGetValue(key, out int v))
        {
            v += amount;
            relationCache[key] = v;
            var entry = relationList.Find(e => e.key == key);
            if (entry != null) entry.value = v;
        }
        else
        {
            relationCache[key] = amount;
            relationList.Add(new RelationEntry { key = key, value = amount });
        }
    }

    // --- Flag ---
    public bool GetFlag(string key)
        => flagCache.TryGetValue(key, out bool v) && v;

    public void SetFlag(string key, bool value)
    {
        flagCache[key] = value;
        var entry = flagList.Find(e => e.key == key);
        if (entry != null) entry.value = value;
        else flagList.Add(new FlagEntry { key = key, value = value });
    }

    // --- Item ---
    public int GetItem(string key, int defaultValue = 0)
        => itemCache.TryGetValue(key, out int v) ? v : defaultValue;

    public void AddItem(string key, int amount)
    {
        if (itemCache.TryGetValue(key, out int v))
        {
            v += amount;
            itemCache[key] = v;
            var entry = itemList.Find(e => e.key == key);
            if (entry != null) entry.value = v;
        }
        else
        {
            itemCache[key] = amount;
            itemList.Add(new ItemEntry { key = key, value = amount });
        }
    }

    // --- Stat ---
    public int GetStat(string key, int defaultValue = 0)
        => statCache.TryGetValue(key, out int v) ? v : defaultValue;

    public void AddStat(string key, int amount)
    {
        if (statCache.TryGetValue(key, out int v))
        {
            v += amount;
            statCache[key] = v;
            var entry = statList.Find(e => e.key == key);
            if (entry != null) entry.value = v;
        }
        else
        {
            statCache[key] = amount;
            statList.Add(new StatEntry { key = key, value = amount });
        }
    }

    // --- Gold ---
    public int GetGold() => gold;
    public void AddGold(int amount) { gold += amount; }
}

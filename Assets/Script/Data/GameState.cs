using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState I { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool logOps = false;

    // ── 내부 스토리지 ───────────────────────────────────────────────
    // flag -> 존재 여부
    private readonly HashSet<string> flags = new(StringComparer.Ordinal);

    // vars -> scope -> key -> value
    private readonly Dictionary<string, Dictionary<string, int>> vars =
        new(StringComparer.Ordinal);

    // items -> key -> count
    private readonly Dictionary<string, int> items =
        new(StringComparer.Ordinal);

    // affinity -> who -> score
    private readonly Dictionary<string, int> affinity =
        new(StringComparer.Ordinal);

    // relations -> "a|b" (정렬된 쌍키) -> score
    private readonly Dictionary<string, int> relations =
        new(StringComparer.Ordinal);

    // ── 수명 ────────────────────────────────────────────────────────
    private void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Flags ───────────────────────────────────────────────────────
    public void SetFlag(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        flags.Add(id);
        if (logOps) Debug.Log($"[GS] Flag ON: {id}");
    }

    public void RemoveFlag(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        flags.Remove(id);
        if (logOps) Debug.Log($"[GS] Flag OFF: {id}");
    }

    public bool HasFlag(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return flags.Contains(id);
    }

    // ── Vars (scope/key) ───────────────────────────────────────────
    public int GetVar(string scope, string key)
    {
        if (string.IsNullOrEmpty(scope) || string.IsNullOrEmpty(key)) return 0;
        return vars.TryGetValue(scope, out var table) && table.TryGetValue(key, out var v) ? v : 0;
    }

    public void AddVar(string scope, string key, int delta)
    {
        if (string.IsNullOrEmpty(scope) || string.IsNullOrEmpty(key) || delta == 0) return;
        if (!vars.TryGetValue(scope, out var table))
        {
            table = new Dictionary<string, int>(StringComparer.Ordinal);
            vars[scope] = table;
        }
        int cur = 0; table.TryGetValue(key, out cur);
        int next = cur + delta;
        table[key] = next;
        if (logOps) Debug.Log($"[GS] Var {scope}.{key} {cur} -> {next} (Δ {delta})");
    }

    public void SetVar(string scope, string key, int value)
    {
        if (string.IsNullOrEmpty(scope) || string.IsNullOrEmpty(key)) return;
        if (!vars.TryGetValue(scope, out var table))
        {
            table = new Dictionary<string, int>(StringComparer.Ordinal);
            vars[scope] = table;
        }
        int cur = 0; table.TryGetValue(key, out cur);
        table[key] = value;
        if (logOps) Debug.Log($"[GS] Var {scope}.{key} {cur} -> {value} (set)");
    }

    // ── Items ───────────────────────────────────────────────────────
    public int GetItem(string key)
    {
        if (string.IsNullOrEmpty(key)) return 0;
        return items.TryGetValue(key, out var v) ? v : 0;
    }

    public void AddItem(string key, int delta)
    {
        if (string.IsNullOrEmpty(key) || delta == 0) return;
        int cur = GetItem(key);
        int next = cur + delta;
        items[key] = next;
        if (logOps) Debug.Log($"[GS] Item {key} {cur} -> {next} (Δ {delta})");
    }

    // ── Affinity ───────────────────────────────────────────────────
    public int GetAffinity(string who)
    {
        if (string.IsNullOrEmpty(who)) return 0;
        return affinity.TryGetValue(who, out var v) ? v : 0;
    }

    public void AddAffinity(string who, int delta)
    {
        if (string.IsNullOrEmpty(who) || delta == 0) return;
        int cur = GetAffinity(who);
        int next = cur + delta;
        affinity[who] = next;
        if (logOps) Debug.Log($"[GS] Affinity {who} {cur} -> {next} (Δ {delta})");
    }

    // ── Relations (양방향 쌍을 정규화하여 하나의 키로 관리) ─────────
    static string PairKey(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) a = "?";
        if (string.IsNullOrEmpty(b)) b = "?";
        return string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
    }

    public int GetRelation(string a, string b)
    {
        string k = PairKey(a, b);
        return relations.TryGetValue(k, out var v) ? v : 0;
    }

    public void AddRelation(string a, string b, int delta)
    {
        if (delta == 0) return;
        string k = PairKey(a, b);
        int cur = GetRelation(a, b);
        int next = cur + delta;
        relations[k] = next;
        if (logOps) Debug.Log($"[GS] Relation {k} {cur} -> {next} (Δ {delta})");
    }

    // ── Export / Import (세이브 파일 연동) ─────────────────────────
    [Serializable] public class SaveFlags { public List<string> list = new(); }
    [Serializable] public class SaveVars { public List<ScopeBlock> scopes = new(); }
    [Serializable] public class ScopeBlock { public string scope; public List<KV> entries = new(); }
    [Serializable] public class KV { public string key; public int value; }
    [Serializable] public class KV2 { public string key; public int value; } // items, affinity, relations 공용

    public SaveFlags ExportFlags()
    {
        var sf = new SaveFlags();
        foreach (var f in flags) sf.list.Add(f);
        return sf;
    }

    public SaveVars ExportVars()
    {
        var sv = new SaveVars();
        foreach (var (scope, table) in vars)
        {
            var block = new ScopeBlock { scope = scope, entries = new List<KV>() };
            foreach (var (k, v) in table)
                block.entries.Add(new KV { key = k, value = v });
            sv.scopes.Add(block);
        }
        return sv;
    }

    public List<KV2> ExportItems()
    {
        var list = new List<KV2>();
        foreach (var (k, v) in items) list.Add(new KV2 { key = k, value = v });
        return list;
    }

    public List<KV2> ExportAffinity()
    {
        var list = new List<KV2>();
        foreach (var (k, v) in affinity) list.Add(new KV2 { key = k, value = v });
        return list;
    }

    public List<KV2> ExportRelations()
    {
        var list = new List<KV2>();
        foreach (var (k, v) in relations) list.Add(new KV2 { key = k, value = v });
        return list;
    }

    public void ImportFlags(SaveFlags data)
    {
        flags.Clear();
        if (data?.list != null)
            foreach (var f in data.list) if (!string.IsNullOrEmpty(f)) flags.Add(f);
    }

    public void ImportVars(SaveVars data)
    {
        vars.Clear();
        if (data?.scopes == null) return;
        foreach (var block in data.scopes)
        {
            if (string.IsNullOrEmpty(block.scope)) continue;
            var table = new Dictionary<string, int>(StringComparer.Ordinal);
            if (block.entries != null)
                foreach (var kv in block.entries)
                    if (!string.IsNullOrEmpty(kv.key)) table[kv.key] = kv.value;
            vars[block.scope] = table;
        }
    }

    public void ImportItems(List<KV2> data)
    {
        items.Clear();
        if (data == null) return;
        foreach (var kv in data)
            if (!string.IsNullOrEmpty(kv.key)) items[kv.key] = kv.value;
    }

    public void ImportAffinity(List<KV2> data)
    {
        affinity.Clear();
        if (data == null) return;
        foreach (var kv in data)
            if (!string.IsNullOrEmpty(kv.key)) affinity[kv.key] = kv.value;
    }

    public void ImportRelations(List<KV2> data)
    {
        relations.Clear();
        if (data == null) return;
        foreach (var kv in data)
            if (!string.IsNullOrEmpty(kv.key)) relations[kv.key] = kv.value;
    }

    // (선택) 전체 초기화
    public void ResetAll()
    {
        flags.Clear();
        vars.Clear();
        items.Clear();
        affinity.Clear();
        relations.Clear();
        if (logOps) Debug.Log("[GS] ResetAll()");
    }
}

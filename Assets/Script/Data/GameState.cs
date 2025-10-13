using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState I { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool logOps = false;

    // 내부 스토리지
    private readonly HashSet<string> flags = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, int>> vars = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> items = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> affinity = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> relations = new(StringComparer.Ordinal); // key: "a|b"

    private void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        // ★ 루트가 아니면 부모 해제 후 DDOL (경고 방지)
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // Flags
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
    public bool HasFlag(string id) => !string.IsNullOrEmpty(id) && flags.Contains(id);

    // Vars
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

    // Items
    public int GetItem(string key) => string.IsNullOrEmpty(key) ? 0 : (items.TryGetValue(key, out var v) ? v : 0);
    public void AddItem(string key, int delta)
    {
        if (string.IsNullOrEmpty(key) || delta == 0) return;
        int cur = GetItem(key);
        int next = cur + delta;
        items[key] = next;
        if (logOps) Debug.Log($"[GS] Item {key} {cur} -> {next} (Δ {delta})");
    }

    // Affinity
    public int GetAffinity(string who) => string.IsNullOrEmpty(who) ? 0 : (affinity.TryGetValue(who, out var v) ? v : 0);
    public void AddAffinity(string who, int delta)
    {
        if (string.IsNullOrEmpty(who) || delta == 0) return;
        int cur = GetAffinity(who);
        int next = cur + delta;
        affinity[who] = next;
        if (logOps) Debug.Log($"[GS] Affinity {who} {cur} -> {next} (Δ {delta})");
    }

    // Relation (양방향 정규화)
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

    // Export/Import (원하면 SaveManager에서 사용)
    [Serializable] public class KV { public string key; public int value; }
    [Serializable] public class ScopeBlock { public string scope; public List<KV> entries = new(); }

    public List<string> ExportFlags() { return new List<string>(flags); }
    public List<ScopeBlock> ExportVars()
    {
        var ret = new List<ScopeBlock>();
        foreach (var (scope, table) in vars)
        {
            var sb = new ScopeBlock { scope = scope, entries = new List<KV>() };
            foreach (var (k, v) in table) sb.entries.Add(new KV { key = k, value = v });
            ret.Add(sb);
        }
        return ret;
    }
    public List<KV> ExportItems()
    {
        var ret = new List<KV>();
        foreach (var (k, v) in items) ret.Add(new KV { key = k, value = v });
        return ret;
    }
    public List<KV> ExportAffinity()
    {
        var ret = new List<KV>();
        foreach (var (k, v) in affinity) ret.Add(new KV { key = k, value = v });
        return ret;
    }
    public List<KV> ExportRelations()
    {
        var ret = new List<KV>();
        foreach (var (k, v) in relations) ret.Add(new KV { key = k, value = v });
        return ret;
    }

    public void ImportFlags(IEnumerable<string> list)
    {
        flags.Clear();
        if (list == null) return;
        foreach (var f in list) if (!string.IsNullOrEmpty(f)) flags.Add(f);
    }
    public void ImportVars(IEnumerable<ScopeBlock> data)
    {
        vars.Clear();
        if (data == null) return;
        foreach (var sb in data)
        {
            if (string.IsNullOrEmpty(sb.scope)) continue;
            var table = new Dictionary<string, int>(StringComparer.Ordinal);
            if (sb.entries != null) foreach (var kv in sb.entries) if (!string.IsNullOrEmpty(kv.key)) table[kv.key] = kv.value;
            vars[sb.scope] = table;
        }
    }
    public void ImportItems(IEnumerable<KV> data)
    {
        items.Clear();
        if (data == null) return;
        foreach (var kv in data) if (!string.IsNullOrEmpty(kv.key)) items[kv.key] = kv.value;
    }
    public void ImportAffinity(IEnumerable<KV> data)
    {
        affinity.Clear();
        if (data == null) return;
        foreach (var kv in data) if (!string.IsNullOrEmpty(kv.key)) affinity[kv.key] = kv.value;
    }
    public void ImportRelations(IEnumerable<KV> data)
    {
        relations.Clear();
        if (data == null) return;
        foreach (var kv in data) if (!string.IsNullOrEmpty(kv.key)) relations[kv.key] = kv.value;
    }

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

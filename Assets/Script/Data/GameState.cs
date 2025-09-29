using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState I { get; private set; }

    public int gold = 0;
    public int stamina = 10;

    private readonly Dictionary<string, int> affinity = new Dictionary<string, int>();
    private readonly Dictionary<string, int> items = new Dictionary<string, int>();
    private readonly HashSet<string> flags = new HashSet<string>();

    private void Awake()
    {
        // 싱글톤 보장
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    public int GetAffinity(string id) => affinity.TryGetValue(id, out var v) ? v : 0;
    public void AddAffinity(string id, int delta) => affinity[id] = GetAffinity(id) + delta;

    public int GetItem(string id) => items.TryGetValue(id, out var v) ? v : 0;
    public void AddItem(string id, int delta) => items[id] = Mathf.Max(0, GetItem(id) + delta);

    public bool HasFlag(string id) => flags.Contains(id);
    public void SetFlag(string id) { if (!string.IsNullOrEmpty(id)) flags.Add(id); }

    public int GetVar(string scope, string key)
    {
        scope = scope.ToLowerInvariant();
        if (scope == "affinity") return GetAffinity(key);
        if (scope == "item") return GetItem(key);
        if (scope == "gold") return gold;
        if (scope == "stamina") return stamina;
        return 0;
    }
    public void AddVar(string scope, string key, int delta)
    {
        scope = scope.ToLowerInvariant();
        if (scope == "affinity") AddAffinity(key, delta);
        else if (scope == "item") AddItem(key, delta);
        else if (scope == "gold") gold += delta;
        else if (scope == "stamina") stamina += delta;
    }


    // GameState.cs 안에 추가

    // 캐릭터 간 관계치 (양방향 보장)
    private readonly Dictionary<string, int> relations = new Dictionary<string, int>();
    private string RelKey(string a, string b)
    {
        // 사전 순으로 정렬해서 (A|B) 같은 키로 통일
        if (string.Compare(a, b) > 0) { var t = a; a = b; b = t; }
        return $"{a}|{b}";
    }
    public int GetRelation(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || a == b) return 0;
        var key = RelKey(a, b);
        return relations.TryGetValue(key, out var v) ? v : 0;
    }
    public void SetRelation(string a, string b, int value)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || a == b) return;
        var key = RelKey(a, b);
        relations[key] = Mathf.Clamp(value, -100, 100);
    }
    public void AddRelation(string a, string b, int delta)
    {
        int cur = GetRelation(a, b);
        SetRelation(a, b, cur + delta);
    }

}

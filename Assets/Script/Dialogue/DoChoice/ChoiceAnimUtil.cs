using System.Collections.Generic;
using System.Globalization;

public static class ChoiceAnimUtil
{
    public static Dictionary<string, string> ParseArgs(string args)
    {
        var dict = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(args)) return dict;
        foreach (var part in args.Split(';'))
        {
            var s = part.Trim();
            if (string.IsNullOrEmpty(s)) continue;
            var kv = s.Split('=');
            if (kv.Length == 2) dict[kv[0].Trim().ToLowerInvariant()] = kv[1].Trim();
        }
        return dict;
    }

    public static bool HasFlag(string flags, string key)
    {
        if (string.IsNullOrWhiteSpace(flags)) return false;
        key = key.Trim().ToLowerInvariant();
        foreach (var f in flags.Split(';'))
            if (f.Trim().ToLowerInvariant() == key) return true;
        return false;
    }

    public static string GetString(Dictionary<string, string> map, string key, string def = null)
        => map != null && map.TryGetValue(key.ToLowerInvariant(), out var v) ? v : def;

    public static float GetFloat(Dictionary<string, string> map, string key, float def)
    {
        if (map != null && map.TryGetValue(key.ToLowerInvariant(), out var v))
            if (float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return f;
        return def;
    }
}

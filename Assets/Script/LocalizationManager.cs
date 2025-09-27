using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LocalizationManager
{
    private static Dictionary<string, string> speakers = new Dictionary<string, string>();
    private static Dictionary<string, Color> speakerColors = new Dictionary<string, Color>();
    private static string currentLang = "kr";

    public static void LoadLanguage(string lang = "kr")
    {
        currentLang = lang;
        speakers.Clear();
        speakerColors.Clear();

        string filePath = Path.Combine(Application.streamingAssetsPath, $"Speakers_{lang}.csv");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Speakers file not found: {filePath}");
            return;
        }

        var lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return;

        var header = lines[0].Split(',');
        int idxId = System.Array.IndexOf(header, "SpeakerId");
        int idxName = System.Array.IndexOf(header, "Name");
        int idxColor = System.Array.IndexOf(header, "Color");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            string id = Safe(cols, idxId);
            string name = Safe(cols, idxName);
            string colorStr = Safe(cols, idxColor);

            if (!string.IsNullOrEmpty(id))
            {
                speakers[id] = name;

                if (!string.IsNullOrEmpty(colorStr) && ColorUtility.TryParseHtmlString(colorStr, out var c))
                    speakerColors[id] = c;
                else
                    speakerColors[id] = Color.white; // 기본값
            }
        }

        Debug.Log($"[Localization] Loaded {speakers.Count} speakers for lang={lang}");
    }

    public static string GetSpeakerName(string id)
    {
        if (string.IsNullOrEmpty(id)) return "";
        if (speakers.TryGetValue(id, out var name))
            return name;
        return id;
    }

    public static Color GetSpeakerColor(string id)
    {
        if (string.IsNullOrEmpty(id)) return Color.white;
        if (speakerColors.TryGetValue(id, out var color))
            return color;
        return Color.white;
    }

    private static string Safe(string[] cols, int idx)
        => (idx >= 0 && idx < cols.Length) ? cols[idx].Trim() : "";
}

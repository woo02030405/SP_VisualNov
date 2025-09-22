using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class LocalizationManager
{
    public enum Language { Korean, English, Japanese }

    public static Language CurrentLanguage { get; private set; } = Language.Korean;

    private static Dictionary<string, string> speakerNameCache = new();
    private static Dictionary<string, Color> speakerColorCache = new();

    private static string GetLangCode(Language lang)
    {
        return lang switch
        {
            Language.Korean => "kr",
            Language.English => "en",
            Language.Japanese => "jp",
            _ => "kr"
        };
    }

    private static string DialoguePath => Path.Combine(Application.streamingAssetsPath, "CSV/Dialogue");

    //  스피커 로드
    public static List<Dictionary<string, string>> LoadSpeakers()
    {
        string filePath = Path.Combine(DialoguePath, $"Speakers_{GetLangCode(CurrentLanguage)}.csv");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[LocalizationManager] Speakers CSV not found: {filePath}");
            return null;
        }

        string csvText = File.ReadAllText(filePath);
        var table = CSVParser.Parse(csvText);

        InitializeSpeakers(table);
        return table;
    }

    //  스토리 로드
    public static List<Dictionary<string, string>> LoadStory(string chapterId)
    {
        string filePath = Path.Combine(DialoguePath, $"{chapterId}_Story_{GetLangCode(CurrentLanguage)}.csv");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[LocalizationManager] Story CSV not found: {filePath}");
            return null;
        }

        string csvText = File.ReadAllText(filePath);
        return CSVParser.Parse(csvText);
    }

    //  다이얼로그 로드 (언어 공용)
    public static List<Dictionary<string, string>> LoadDialogue(string chapterId)
    {
        string filePath = Path.Combine(DialoguePath, $"{chapterId}_Dialogue.csv");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[LocalizationManager] Dialogue CSV not found: {filePath}");
            return null;
        }

        string csvText = File.ReadAllText(filePath);
        return CSVParser.Parse(csvText);
    }

    //  스피커 캐시 초기화
    private static void InitializeSpeakers(List<Dictionary<string, string>> speakers)
    {
        speakerNameCache.Clear();
        speakerColorCache.Clear();

        foreach (var row in speakers)
        {
            if (row.TryGetValue("SpeakerId", out string id))
            {
                if (row.TryGetValue("Name", out string name))
                    speakerNameCache[id] = name;

                if (row.TryGetValue("Color", out string colorStr) &&
                    ColorUtility.TryParseHtmlString(colorStr, out Color c))
                {
                    speakerColorCache[id] = c;
                }
            }
        }
    }

    //  이름/색상 제공
    public static string GetSpeakerName(string speakerId) =>
        speakerNameCache.TryGetValue(speakerId, out var name) ? name : speakerId;

    public static Color GetSpeakerColor(string speakerId) =>
        speakerColorCache.TryGetValue(speakerId, out var c) ? c : Color.white;

    public static void SetLanguage(Language lang)
    {
        CurrentLanguage = lang;
    }
}

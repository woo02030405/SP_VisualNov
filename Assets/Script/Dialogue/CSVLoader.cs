using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CSVLoader
{
    public static Dictionary<string, T> LoadTable<T>(string fileName, string keyFieldName) where T : new()
    {
        TextAsset csvData = Resources.Load<TextAsset>($"CSV/{fileName}");
        if (csvData == null)
        {
            Debug.LogError($"[CSVLoader] CSV 파일을 찾을 수 없음: {fileName}");
            return null;
        }

        // 줄 통일 (CRLF/CR/LF)
        var raw = csvData.text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = raw.Split('\n');
        if (lines.Length == 0) return null;

        string[] header = ParseCsvLine(lines[0]);
        for (int h = 0; h < header.Length; h++) header[h] = header[h].Trim();

        var table = new Dictionary<string, T>();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = ParseCsvLine(line);
            if (values.Length == 0) continue;

            T entry = new T();

            for (int j = 0; j < header.Length && j < values.Length; j++)
            {
                var field = typeof(T).GetField(header[j]);
                if (field == null) continue;

                string v = values[j] ?? string.Empty;
                v = v.Trim();

                // CSV 규칙: "" -> "
                v = v.Replace("\"\"", "\"");

                field.SetValue(entry, v);
            }

            var keyField = typeof(T).GetField(keyFieldName);
            if (keyField == null)
            {
                Debug.LogError($"[CSVLoader] 키 필드({keyFieldName})를 찾을 수 없음. 타입: {typeof(T).Name}");
                continue;
            }

            string key = keyField.GetValue(entry)?.ToString();
            if (string.IsNullOrEmpty(key)) continue;

            if (!table.ContainsKey(key))
                table.Add(key, entry);
        }

        return table;
    }

    // 따옴표 안 콤마 무시, 양끝 큰따옴표 제거, "" -> " 복원
    private static string[] ParseCsvLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return new string[0];

        var parts = Regex.Split(line, @",(?=(?:[^""]*""[^""]*"")*[^""]*$)");

        for (int i = 0; i < parts.Length; i++)
        {
            var s = parts[i].Trim();

            if (s.Length >= 2 && s.StartsWith("\"") && s.EndsWith("\""))
                s = s.Substring(1, s.Length - 2);

            s = s.Replace("\"\"", "\"");
            parts[i] = s;
        }
        return parts;
    }
}

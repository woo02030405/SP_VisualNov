using System.Collections.Generic;
using System.Text;

public static class CSVParser
{
    /// <summary>
    /// RFC4180 스타일 CSV 파서.
    /// - 따옴표(") 내부의 쉼표/개행 허용
    /// - 따옴표 이스케이프: "" -> "
    /// - 헤더(row0) 기준으로 Dictionary 생성 (열 개수가 안 맞으면 부족분은 빈 문자열)
    /// </summary>
    public static List<Dictionary<string, string>> Parse(string csvText, bool hasHeader = true)
    {
        var rows = ParseRows(csvText);
        var result = new List<Dictionary<string, string>>();
        if (rows.Count == 0) return result;

        List<string> headers;
        int startRow = 0;

        if (hasHeader)
        {
            headers = rows[0];
            startRow = 1;
        }
        else
        {
            // 헤더가 없으면 Col1, Col2... 생성
            int maxCols = 0;
            foreach (var r in rows) if (r.Count > maxCols) maxCols = r.Count;
            headers = new List<string>(maxCols);
            for (int i = 0; i < maxCols; i++) headers.Add($"Col{i + 1}");
        }

        for (int i = startRow; i < rows.Count; i++)
        {
            var row = rows[i];
            var dict = new Dictionary<string, string>(headers.Count);
            for (int c = 0; c < headers.Count; c++)
            {
                string key = headers[c] ?? $"Col{c + 1}";
                string val = (c < row.Count) ? row[c] ?? "" : "";
                dict[key] = val;
            }
            result.Add(dict);
        }

        return result;
    }

    /// <summary>
    /// 텍스트를 행/열 리스트로 파싱. 각 행은 List&lt;string&gt;.
    /// </summary>
    public static List<List<string>> ParseRows(string csvText)
    {
        var rows = new List<List<string>>();
        if (string.IsNullOrEmpty(csvText)) return rows;

        var row = new List<string>();
        var field = new StringBuilder();

        bool inQuotes = false;
        for (int i = 0; i < csvText.Length; i++)
        {
            char ch = csvText[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    // "" -> " (이스케이프)
                    if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        field.Append('"');
                        i++; // 다음 따옴표 스킵
                    }
                    else
                    {
                        inQuotes = false; // 따옴표 닫힘
                    }
                }
                else
                {
                    field.Append(ch);
                }
            }
            else
            {
                if (ch == '"')
                {
                    inQuotes = true; // 따옴표 시작
                }
                else if (ch == ',')
                {
                    row.Add(field.ToString());
                    field.Length = 0;
                }
                else if (ch == '\r' || ch == '\n')
                {
                    // 행 종료 (CR, LF, CRLF 모두 지원)
                    row.Add(field.ToString());
                    field.Length = 0;

                    rows.Add(row);
                    row = new List<string>();

                    // CRLF 처리: \r 뒤에 \n 오면 스킵
                    if (ch == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                        i++;
                }
                else
                {
                    field.Append(ch);
                }
            }
        }

        // 마지막 필드/행
        row.Add(field.ToString());
        rows.Add(row);

        return rows;
    }
}

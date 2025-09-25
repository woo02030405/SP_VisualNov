using System.Collections.Generic;
using System.Text;

public static class CSVParser
{
    /// <summary>
    /// RFC4180 ��Ÿ�� CSV �ļ�.
    /// - ����ǥ(") ������ ��ǥ/���� ���
    /// - ����ǥ �̽�������: "" -> "
    /// - ���(row0) �������� Dictionary ���� (�� ������ �� ������ �������� �� ���ڿ�)
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
            // ����� ������ Col1, Col2... ����
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
    /// �ؽ�Ʈ�� ��/�� ����Ʈ�� �Ľ�. �� ���� List&lt;string&gt;.
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
                    // "" -> " (�̽�������)
                    if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        field.Append('"');
                        i++; // ���� ����ǥ ��ŵ
                    }
                    else
                    {
                        inQuotes = false; // ����ǥ ����
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
                    inQuotes = true; // ����ǥ ����
                }
                else if (ch == ',')
                {
                    row.Add(field.ToString());
                    field.Length = 0;
                }
                else if (ch == '\r' || ch == '\n')
                {
                    // �� ���� (CR, LF, CRLF ��� ����)
                    row.Add(field.ToString());
                    field.Length = 0;

                    rows.Add(row);
                    row = new List<string>();

                    // CRLF ó��: \r �ڿ� \n ���� ��ŵ
                    if (ch == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                        i++;
                }
                else
                {
                    field.Append(ch);
                }
            }
        }

        // ������ �ʵ�/��
        row.Add(field.ToString());
        rows.Add(row);

        return rows;
    }
}

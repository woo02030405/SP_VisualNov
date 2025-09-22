using System.Collections.Generic;
using UnityEngine;

public static class CsvLoader
{
    /// <summary>
    /// TextAsset에서 CSV 파싱. 기본은 헤더 사용(true).
    /// </summary>
    public static List<Dictionary<string, string>> Load(TextAsset asset, bool hasHeader = true)
    {
        if (asset == null || string.IsNullOrEmpty(asset.text))
            return new List<Dictionary<string, string>>();

        // UTF-8 with/without BOM 모두 Unity TextAsset.text가 처리함
        return CSVParser.Parse(asset.text, hasHeader);
    }
}

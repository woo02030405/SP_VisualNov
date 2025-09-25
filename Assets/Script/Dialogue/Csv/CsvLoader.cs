using System.Collections.Generic;
using UnityEngine;

public static class CsvLoader
{
    /// <summary>
    /// TextAsset���� CSV �Ľ�. �⺻�� ��� ���(true).
    /// </summary>
    public static List<Dictionary<string, string>> Load(TextAsset asset, bool hasHeader = true)
    {
        if (asset == null || string.IsNullOrEmpty(asset.text))
            return new List<Dictionary<string, string>>();

        // UTF-8 with/without BOM ��� Unity TextAsset.text�� ó����
        return CSVParser.Parse(asset.text, hasHeader);
    }
}

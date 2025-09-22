using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static SaveData currentSave;

    // ✅ 현재 세이브 데이터
    public static SaveData CurrentSave => currentSave;

    private static string SavePath => Path.Combine(Application.persistentDataPath, "saves");

    /// <summary>
    /// 현재 세이브 데이터를 교체
    /// </summary>
    public static void SetCurrentSave(SaveData data)
    {
        currentSave = data;
    }

    /// <summary>
    /// 현재 세이브를 슬롯에 저장
    /// </summary>
    public static void Save(string slotName)
    {
        if (currentSave != null)
            SaveGame(currentSave, slotName);
    }

    /// <summary>
    /// 지정 슬롯에서 불러오고 CurrentSave 갱신
    /// </summary>
    public static void Load(string slotName)
    {
        currentSave = LoadGame(slotName);
    }

    /// <summary>
    /// 저장
    /// </summary>
    public static void SaveGame(SaveData saveData, string slotName)
    {
        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);

        string json = JsonUtility.ToJson(saveData, true);
        string filePath = Path.Combine(SavePath, slotName + ".json");

        File.WriteAllText(filePath, json);

        Debug.Log($"[SaveManager] Saved {slotName} at {filePath}");
    }

    /// <summary>
    /// 로드
    /// </summary>
    public static SaveData LoadGame(string slotName)
    {
        string filePath = Path.Combine(SavePath, slotName + ".json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveManager] No save found for {slotName}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[SaveManager] Loaded {slotName} from {filePath}");
        return saveData;
    }

    /// <summary>
    /// 슬롯 존재 여부
    /// </summary>
    public static bool SaveExists(string slotName)
    {
        string filePath = Path.Combine(SavePath, slotName + ".json");
        return File.Exists(filePath);
    }

    /// <summary>
    /// 세이브 삭제
    /// </summary>
    public static void DeleteSave(string slotName)
    {
        string filePath = Path.Combine(SavePath, slotName + ".json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"[SaveManager] Deleted save {slotName}");
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace VN.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string saveFilePath => Path.Combine(Application.persistentDataPath, "SaveData.json");
        private string thumbnailFolder => Path.Combine(Application.persistentDataPath, "Thumbnails");

        private List<SaveData> saveSlots = new List<SaveData>();
        private const int MaxSlots = 50;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                if (!Directory.Exists(thumbnailFolder))
                    Directory.CreateDirectory(thumbnailFolder);
                LoadAll();
            }
            else Destroy(gameObject);
        }

        public int GetTotalSlotCount() => MaxSlots;

        public List<SaveData> GetSlotsForPage(int pageIndex, int slotsPerPage)
        {
            int start = pageIndex * slotsPerPage;
            int end = Mathf.Min(start + slotsPerPage, saveSlots.Count);
            var result = new List<SaveData>();
            for (int i = start; i < end; i++) result.Add(saveSlots[i]);
            while (result.Count < slotsPerPage) result.Add(null);
            return result;
        }

        public void Save(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots) return;
            var data = new SaveData
            {
                title = $"Chapter X / Node Y",
                dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                thumbnailPath = CaptureThumbnail(slotIndex)
            };
            while (saveSlots.Count < MaxSlots) saveSlots.Add(null);
            saveSlots[slotIndex] = data;
            SaveAll();
            Debug.Log($"Saved slot {slotIndex}");
        }

        public void Load(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= saveSlots.Count) return;
            var data = saveSlots[slotIndex];
            if (data == null) { Debug.Log("ºó ½½·ÔÀÔ´Ï´Ù."); return; }
            Debug.Log($"Load slot {slotIndex} ¡æ {data.title}");
            // TODO: GameScene »óÅÂ º¹¿ø
        }

        private void SaveAll()
        {
            string json = JsonUtility.ToJson(new SaveDatabase(saveSlots), true);
            File.WriteAllText(saveFilePath, json);
        }

        private void LoadAll()
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                var db = JsonUtility.FromJson<SaveDatabase>(json);
                saveSlots = db.slots ?? new List<SaveData>();
            }
            else saveSlots = new List<SaveData>();
        }

        private string CaptureThumbnail(int slotIndex)
        {
            string fileName = $"slot_{slotIndex:D2}.png";
            string path = Path.Combine(thumbnailFolder, fileName);
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(path, png);
            Destroy(tex);
            return path;
        }
    }

    [System.Serializable]
    public class SaveDatabase
    {
        public List<SaveData> slots;
        public SaveDatabase(List<SaveData> slots) { this.slots = slots; }
    }
}

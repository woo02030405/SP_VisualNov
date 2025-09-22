using UnityEngine;
using System.Collections.Generic;
using System.IO;
using VN.Dialogue;

public class SaveLoadUIManager : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotParent;

    [SerializeField] private DialogueManager dialogueManager; 
    private List<SaveSlotUI> slots = new List<SaveSlotUI>();
    private SaveData currentSaveData;
    private bool isSaving = true;

    private string SavePath => Path.Combine(Application.persistentDataPath, "saves");

    void Start()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        slots.Clear();

        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);

        string[] files = Directory.GetFiles(SavePath, "*.json");
        int index = 1;

        // 기존 세이브 슬롯
        foreach (string file in files)
        {
            string slotName = Path.GetFileNameWithoutExtension(file);
            SaveData data = SaveManager.LoadGame(slotName);

            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
            slotUI.Init(slotName, data, OnSlotClicked);
            slots.Add(slotUI);

            index++;
        }

        // 새 슬롯 추가
        GameObject newSlotObj = Instantiate(slotPrefab, slotParent);
        SaveSlotUI newSlotUI = newSlotObj.GetComponent<SaveSlotUI>();
        newSlotUI.Init($"slot{index}", null, OnSlotClicked);
        slots.Add(newSlotUI);
    }

    private void OnSlotClicked(string slotName)
    {
        if (isSaving)
        {
            currentSaveData.title = $"세이브 {slotName}";
            currentSaveData.dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 썸네일 캡처
            string thumbnailPath = Path.Combine(SavePath, slotName + "_thumb.png");
            ScreenCapture.CaptureScreenshot(thumbnailPath);
            currentSaveData.thumbnailPath = thumbnailPath;

            SaveManager.SaveGame(currentSaveData, slotName);
            Debug.Log($"[SaveLoadUIManager] {slotName} 저장 완료");
        }
        else
        {
            SaveData loaded = SaveManager.LoadGame(slotName);
            if (loaded != null)
            {
                currentSaveData = loaded;
                Debug.Log($"[SaveLoadUIManager] {slotName} 불러오기 완료");

                //  DialogueManager로 이어가기
                if (dialogueManager != null && !string.IsNullOrEmpty(loaded.nodeId))
                {
                    dialogueManager.GoToNode(loaded.nodeId); 
                }
            }
        }

        RefreshSlots();
    }

    public void SetCurrentSaveData(SaveData data)
    {
        currentSaveData = data;
    }

    public void SetMode(bool saveMode)
    {
        isSaving = saveMode;
        RefreshSlots();
    }
}

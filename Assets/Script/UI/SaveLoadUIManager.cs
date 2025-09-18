using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using VN.SaveSystem; // SaveManager, SaveData 참조

public class SaveLoadUIManager : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text titleText;   // SAVE / LOAD

    [Header("NavigationBar")]
    public Button prevButton;
    public TMP_Text pageLabel;
    public Button nextButton;

    [Header("Footer")]
    public Button backButton;
    public TMP_Text infoText;

    [Header("Slots")]
    public Transform slotGrid;     // GridLayoutGroup
    public GameObject slotPrefab;  // SlotPrefab
    private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();

    private int currentPage = 0;
    private const int slotsPerPage = 10;

    // Save/Load 모드 (true = Save, false = Load)
    public bool isSaveMode = false;

    void Start()
    {
        // 페이지 버튼 이벤트
        prevButton.onClick.AddListener(() =>
        {
            currentPage = Mathf.Max(0, currentPage - 1);
            RefreshPage();
        });

        nextButton.onClick.AddListener(() =>
        {
            currentPage = Mathf.Min(TotalPages - 1, currentPage + 1);
            RefreshPage();
        });

        backButton.onClick.AddListener(() =>
        {
            var nav = FindObjectOfType<SceneNavigator>();
            if (nav != null) nav.Back();
        });

        BuildSlots();
        RefreshPage();
    }

    private int TotalPages
    {
        get
        {
            if (SaveManager.Instance == null) return 1;
            int totalSlots = SaveManager.Instance.GetTotalSlotCount();
            return Mathf.Max(1, Mathf.CeilToInt(totalSlots / (float)slotsPerPage));
        }
    }

    private void BuildSlots()
    {
        foreach (Transform child in slotGrid)
        {
            Destroy(child.gameObject);
        }
        slotUIs.Clear();

        for (int i = 0; i < slotsPerPage; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotGrid);
            var ui = slotObj.GetComponent<SaveSlotUI>();
            slotUIs.Add(ui);

            ui.Init(i, isSaveMode, OnClickSlot);
        }
    }

    private void RefreshPage()
    {
        // Header
        titleText.text = isSaveMode ? "SAVE" : "LOAD";

        // Footer
        infoText.text = isSaveMode ? "게임 데이터를 저장합니다." : "게임 데이터를 불러옵니다.";

        // NavigationBar
        pageLabel.text = $"{currentPage + 1} / {TotalPages}";

        // 슬롯 채우기
        var slots = SaveManager.Instance.GetSlotsForPage(currentPage, slotsPerPage);

        for (int i = 0; i < slotsPerPage; i++)
        {
            var data = (i < slots.Count) ? slots[i] : null;
            slotUIs[i].SetData(data, isNewest: (i == 0));
        }

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < (TotalPages - 1);
    }

    private void OnClickSlot(int slotIndexInPage)
    {
        int realIndex = currentPage * slotsPerPage + slotIndexInPage;

        if (isSaveMode)
        {
            SaveManager.Instance.Save(realIndex);
            RefreshPage();
        }
        else
        {
            SaveManager.Instance.Load(realIndex);
        }
    }
}

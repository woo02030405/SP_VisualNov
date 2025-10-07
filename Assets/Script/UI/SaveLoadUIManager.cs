using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VN.SaveSystem;

/// <summary>
/// Save/Load 패널 전체 컨트롤러
/// - OpenSave / OpenLoad / Close
/// - 페이지 이동 / 슬롯 갱신 / 슬롯 클릭 처리
/// </summary>
public class SaveLoadUIManager : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text titleText;
    public TMP_Text pageLabel;

    [Header("Paging")]
    public Button prevButton;
    public Button nextButton;
    public int slotsPerPage = 8;

    [Header("Slots")]
    public List<SaveSlotUI> slots = new List<SaveSlotUI>();

    [Header("Mode")]
    public bool isSaveMode = true;

    // 내부 상태
    private int currentPage = 0;
    private int totalPages = 1;

    void Awake()
    {
        if (prevButton) prevButton.onClick.AddListener(OnClickPrevPage);
        if (nextButton) nextButton.onClick.AddListener(OnClickNextPage);
    }

    // ── 외부에서 호출 ───────────────────────────────────────────────────────────
    public void OpenSave()
    {
        isSaveMode = true;
        if (titleText) titleText.text = "SAVE";
        gameObject.SetActive(true);
        currentPage = 0;
        RefreshPage();
    }

    public void OpenLoad()
    {
        isSaveMode = false;
        if (titleText) titleText.text = "LOAD";
        gameObject.SetActive(true);
        currentPage = 0;
        RefreshPage();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ── 페이지 갱신 ────────────────────────────────────────────────────────────
    public void RefreshPage()
    {
        int totalSlots = SaveManager.Instance.GetTotalSlotCount();
        totalPages = Mathf.Max(1, Mathf.CeilToInt(totalSlots / (float)slotsPerPage));
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        // 현재 페이지의 데이터 읽기
        List<SaveData> pageData = SaveManager.Instance.GetSlotsForPage(currentPage, slotsPerPage);

        // 슬롯 바인딩/표시
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            // pageData가 부족하면 null 처리
            SaveData data = (i < pageData.Count) ? pageData[i] : null;

            // 최신 저장 배지 판단(간단히: 현재 페이지 첫 항목이면 newest)
            bool isNewest = (currentPage == 0 && i == 0 && data != null);

            // 기존 프로젝트가 Init(this, i, data, isNewest)를 기대하므로 그 시그니처로 세팅
            slot.Init(this, i, data, isNewest);
        }

        // 페이지 라벨/버튼
        if (pageLabel) pageLabel.text = $"{currentPage + 1} / {totalPages}";
        if (prevButton) prevButton.interactable = (currentPage > 0);
        if (nextButton) nextButton.interactable = (currentPage < totalPages - 1);
    }

    // ── 페이지 이동 ─────────────────────────────────────────────────────────────
    private void OnClickPrevPage()
    {
        if (currentPage <= 0) return;
        currentPage--;
        RefreshPage();
    }

    private void OnClickNextPage()
    {
        if (currentPage >= totalPages - 1) return;
        currentPage++;
        RefreshPage();
    }

    // ── 슬롯 클릭 처리 (SaveSlotUI → 여기로 콜백) ───────────────────────────────
    public void OnSlotClicked(int slotIndexInPage)
    {
        int realIndex = currentPage * slotsPerPage + slotIndexInPage;

        if (isSaveMode)
        {
            SaveManager.Instance.Save(realIndex);
            RefreshPage();               // 저장 직후 목록 갱신
        }
        else
        {
            SaveManager.Instance.Load(realIndex);
            Close();                     // 필요 없으면 이 줄 제거
        }
    }
}

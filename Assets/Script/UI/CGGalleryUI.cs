using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using VN.Data; // CGRecord, CGCatalog 사용


public class CGGalleryUI : MonoBehaviour
{
    [Header("Data")]
    public CGCatalog catalog;   // ScriptableObject (CG 데이터 모음)

    [Header("Slots (12 per page)")]
    public List<Button> cgButtons;         // 버튼 12개
    public List<Image> cgThumbs;           // 썸네일
    public List<GameObject> lockOverlays;  // 잠금 오버레이

    [Header("Preview (확대 보기)")]
    public GameObject previewPanel;   // 확대 뷰 전체 Panel
    public Image fullImage;           // 큰 이미지
    public TMP_Text titleText;        // 제목
    public Button closeButton;        // 닫기 버튼 (X)

    [Header("Navigation")]
    public Button prevButton;
    public Button nextButton;
    public TMP_Text pageLabel;

    [Header("Canvas")]
    public Button backButton;         // GalleryHubScene로 돌아가는 버튼

    [Header("Progress")]
    public TMP_Text progressText;

    private int currentPage = 0;
    private const int perPage = 12;
    private int TotalPages => Mathf.Max(1, Mathf.CeilToInt(catalog.items.Count / (float)perPage));

    private Color defaultLabelColor;

    void Start()
    {
        // 원래 PageLabel 색 저장
        defaultLabelColor = pageLabel.color;

        // 페이지 이동 버튼
        prevButton.onClick.AddListener(() =>
        {
            currentPage = Mathf.Max(0, currentPage - 1);
            RefreshAll();
        });

        nextButton.onClick.AddListener(() =>
        {
            currentPage = Mathf.Min(TotalPages - 1, currentPage + 1);
            RefreshAll();
        });

        // 닫기 버튼 → Preview 닫을 때 네비게이션 복원
        closeButton.onClick.AddListener(() =>
        {
            previewPanel.SetActive(false);

            prevButton.interactable = true;
            nextButton.interactable = true;
            backButton.interactable = true;
            pageLabel.color = defaultLabelColor;
        });

        // 뒤로가기 버튼 → 메인메뉴 씬 로드
        backButton.onClick.AddListener(() =>
        {
            SceneNavigator.Load("GalleryHubScene");
            // 또는 SceneNavigator.Load("MainMenuScene");
        });

        RefreshAll();
    }

    void RefreshAll()
    {
        var save = SaveManager.CurrentSave; 

        if (catalog == null || catalog.items.Count == 0)
        {
            pageLabel.text = "0 / 0";
            progressText.text = "Unlocked 0 / 0";
            return;
        }

        int unlocked = 0;
        foreach (var c in catalog.items)
        {
            bool isUnlocked = save != null && save.IsCGUnlocked(c.id);
            if (isUnlocked) unlocked++;
        }
        progressText.text = $"Unlocked {unlocked} / {catalog.items.Count}";

        int start = currentPage * perPage;
        for (int i = 0; i < cgButtons.Count; i++)
        {
            int dataIndex = start + i;
            bool active = dataIndex < catalog.items.Count;
            cgButtons[i].gameObject.SetActive(active);
            if (!active) continue;

            var data = catalog.items[dataIndex];
            bool isUnlocked = save != null && save.IsCGUnlocked(data.id);

            cgThumbs[i].sprite = isUnlocked ? data.thumbnail : null;
            lockOverlays[i].SetActive(!isUnlocked);

            cgButtons[i].interactable = isUnlocked;
            cgButtons[i].onClick.RemoveAllListeners();
            if (isUnlocked)
            {
                cgButtons[i].onClick.AddListener(() => ShowPreview(data));
            }
        }

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < (TotalPages - 1);
    }


    void ShowPreview(CGRecord data)
    {
        previewPanel.SetActive(true);
        fullImage.sprite = data.fullImage;
        titleText.text = data.title;

        // 네비게이션 잠금
        prevButton.interactable = false;
        nextButton.interactable = false;
        backButton.interactable = false;
        pageLabel.color = Color.gray;
    }
}

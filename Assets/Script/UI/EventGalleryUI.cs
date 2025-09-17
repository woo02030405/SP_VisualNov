using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using VN.Data;

public class EventGalleryUI : MonoBehaviour
{
    [Header("Data")]
    public EventCatalog catalog;

    [Header("Slots (6 buttons)")]
    public List<Button> eventButtons;        // EventItem 6개 버튼
    public List<Image> eventThumbs;         // 각 버튼 안의 Thumb
    public List<TMP_Text> eventTitles;       // 각 버튼 안의 Title
    public List<GameObject> lockOverlays;    // 각 버튼 안의 LockOverlay

    [Header("Preview")]
    public Image previewImage;
    public TMP_Text previewTitle;
    public TMP_Text previewDesc;
    public Button replayButton;

    [Header("Navigation")]
    public Button prevButton;
    public Button nextButton;
    public TMP_Text pageLabel;

    [Header("Progress")]
    public TMP_Text progressText;

    private int currentPage = 0;
    private const int perPage = 6;
    private int TotalPages => Mathf.Max(1, Mathf.CeilToInt(catalog.events.Count / (float)perPage));

    void Start()
    {
        // 네비게이션 버튼 이벤트 연결
        prevButton.onClick.AddListener(() => { currentPage = Mathf.Max(0, currentPage - 1); RefreshAll(); });
        nextButton.onClick.AddListener(() => { currentPage = Mathf.Min(TotalPages - 1, currentPage + 1); RefreshAll(); });

        RefreshAll();
        SelectFirstUnlockedForPreview();
    }

    void RefreshAll()
    {
        // 페이지 라벨
        pageLabel.text = $"{currentPage + 1} / {TotalPages}";

        // 진행도 (해제된 개수 / 전체)
        int unlocked = 0;
        foreach (var e in catalog.events) if (e.unlocked) unlocked++;
        progressText.text = $"Unlocked {unlocked} / {catalog.events.Count}";

        // 슬롯 6개 데이터 채우기
        int start = currentPage * perPage;
        for (int i = 0; i < eventButtons.Count; i++)
        {
            int dataIndex = start + i;
            bool active = (dataIndex < catalog.events.Count);
            eventButtons[i].gameObject.SetActive(active);
            if (!active) continue;

            var data = catalog.events[dataIndex];
            // 비주얼
            if (eventThumbs[i]) eventThumbs[i].sprite = data.thumbnail;
            if (eventTitles[i]) eventTitles[i].text = data.unlocked ? data.title : "???";
            if (lockOverlays[i]) lockOverlays[i].SetActive(!data.unlocked);

            // 인터랙션
            eventButtons[i].interactable = data.unlocked;

            // 클릭 핸들러 갱신
            eventButtons[i].onClick.RemoveAllListeners();
            int capturedIndex = dataIndex;
            eventButtons[i].onClick.AddListener(() => OnClickEvent(capturedIndex));
        }

        // Prev/Next 상태
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < (TotalPages - 1);
    }

    void OnClickEvent(int dataIndex)
    {
        var data = catalog.events[dataIndex];
        UpdatePreview(data);
    }

    void UpdatePreview(EventRecord data)
    {
        if (previewImage) previewImage.sprite = data.thumbnail;
        if (previewTitle) previewTitle.text = data.unlocked ? data.title : "???";
        if (previewDesc) previewDesc.text = data.unlocked ? data.desc : "잠금 해제 후 확인 가능";

        if (replayButton)
        {
            replayButton.interactable = data.unlocked && !string.IsNullOrEmpty(data.replayScene);
            replayButton.onClick.RemoveAllListeners();
            if (replayButton.interactable)
            {
                replayButton.onClick.AddListener(() =>
                {
                    var nav = FindObjectOfType<SceneNavigator>();
                    if (nav != null) nav.Load(data.replayScene);
                });
            }
        }
    }

    void SelectFirstUnlockedForPreview()
    {
        foreach (var e in catalog.events)
        {
            if (e.unlocked) { UpdatePreview(e); return; }
        }
        // 모두 잠금이면 첫 항목으로
        if (catalog.events.Count > 0) UpdatePreview(catalog.events[0]);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VN.Data;

public class EventGalleryUI : MonoBehaviour
{
    [SerializeField] private EventCatalog catalog;
    [SerializeField] private List<Button> eventButtons;
    [SerializeField] private List<Image> eventThumbs;
    [SerializeField] private List<GameObject> lockOverlays;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Image previewImage;

    private int currentIndex = -1;

    private void Start()
    {
        RefreshAll();
        replayButton.onClick.AddListener(OnReplay);
    }

    private void RefreshAll()
    {
        var save = SaveManager.CurrentSave;

        for (int i = 0; i < eventButtons.Count; i++)
        {
            bool active = i < catalog.events.Count;
            eventButtons[i].gameObject.SetActive(active);
            if (!active) continue;

            var data = catalog.events[i];

            bool isUnlocked = save != null && save.IsEventUnlocked(data.id);

            eventThumbs[i].sprite = isUnlocked ? data.thumbnail : null;
            lockOverlays[i].SetActive(!isUnlocked);

            eventButtons[i].interactable = isUnlocked;
            int index = i;
            eventButtons[i].onClick.RemoveAllListeners();
            if (isUnlocked)
            {
                eventButtons[i].onClick.AddListener(() => ShowPreview(index));
            }
        }

        // 초기화
        titleText.text = "";
        descText.text = "";
        previewImage.sprite = null;
        replayButton.gameObject.SetActive(false);
    }

    private void ShowPreview(int index)
    {
        currentIndex = index;
        var data = catalog.events[index];

        titleText.text = data.title;
        descText.text = data.desc;
        previewImage.sprite = data.thumbnail;

        replayButton.gameObject.SetActive(true);
    }

    private void OnReplay()
    {
        if (currentIndex < 0 || currentIndex >= catalog.events.Count) return;
        var data = catalog.events[currentIndex];

        if (!string.IsNullOrEmpty(data.replayScene))
        {
            SceneNavigator.Load(data.replayScene); 
        }
    }
}

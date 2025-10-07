using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using VN.SaveSystem;

/// <summary>
/// 하나의 슬롯 카드 UI
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image thumbnail;      // 썸네일
    public TMP_Text titleText;   // 제목
    public TMP_Text dateText;    // 날짜
    public GameObject newIcon;   // 최신 저장 배지(옵션)
    public Button slotButton;    // 클릭 버튼

    [HideInInspector] public int boundSlotIndex = -1;
    [HideInInspector] public SaveLoadUIManager saveLoadUIManager;

    // 패널과 슬롯 인덱스 바인딩 + 버튼 연결
    public void Bind(SaveLoadUIManager manager, int slotIndexInPage)
    {
        saveLoadUIManager = manager;
        boundSlotIndex = slotIndexInPage;

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(OnClickSlot);
        }
    }

    // 슬롯 데이터 표시 (null이면 빈 슬롯 처리)
    public void SetData(SaveData data, bool isNewest)
    {
        if (data != null)
        {
            // 썸네일
            if (thumbnail != null)
            {
                var sp = LoadSpriteFromFile(data.thumbnailPath);
                thumbnail.sprite = sp;
                thumbnail.enabled = (sp != null);
            }

            if (titleText != null)
                titleText.text = string.IsNullOrEmpty(data.title) ? "No Title" : data.title;

            if (dateText != null)
                dateText.text = string.IsNullOrEmpty(data.dateTime) ? "--/--/--" : data.dateTime;

            if (newIcon != null)
                newIcon.SetActive(isNewest);
        }
        else
        {
            if (thumbnail != null) { thumbnail.sprite = null; thumbnail.enabled = false; }
            if (titleText != null) titleText.text = "빈 슬롯";
            if (dateText != null) dateText.text = "";
            if (newIcon != null) newIcon.SetActive(false);
        }
    }

    // SaveLoadUIManager가 기대하던 Init 시그니처 대응
    public void Init(SaveLoadUIManager manager, int slotIndexInPage, SaveData data, bool isNewest)
    {
        Bind(manager, slotIndexInPage);
        SetData(data, isNewest);
    }

    private void OnClickSlot()
    {
        if (saveLoadUIManager == null)
        {
            Debug.LogWarning("[SaveSlotUI] saveLoadUIManager is null. Bind/Init 확인");
            return;
        }
        if (boundSlotIndex < 0)
        {
            Debug.LogWarning("[SaveSlotUI] boundSlotIndex < 0. Bind/Init 확인");
            return;
        }

        saveLoadUIManager.OnSlotClicked(boundSlotIndex);
    }

    // 파일→Sprite 유틸(선택)
    private static Sprite LoadSpriteFromFile(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (!File.Exists(path)) return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                Object.Destroy(tex);
                return null;
            }
            tex.Apply(false, true);
            var rect = new Rect(0, 0, tex.width, tex.height);
            var pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(tex, rect, pivot, 100f);
        }
        catch { return null; }
    }
}

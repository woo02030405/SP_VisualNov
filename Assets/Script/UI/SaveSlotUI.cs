using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using VN.SaveSystem;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image thumbnail;
    public TMP_Text titleText;
    public TMP_Text dateText;
    public GameObject newIcon;
    public Button slotButton;

    private int slotIndex;

    public void Init(int index, bool saveMode, System.Action<int> onClick)
    {
        slotIndex = index;
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => onClick?.Invoke(slotIndex));
    }

    public void SetData(SaveData data, bool isNewest = false)
    {
        if (data != null)
        {
            // ½æ³×ÀÏ ·Îµå
            if (!string.IsNullOrEmpty(data.thumbnailPath) && File.Exists(data.thumbnailPath))
            {
                byte[] png = File.ReadAllBytes(data.thumbnailPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(png);
                thumbnail.sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f));
            }
            else
            {
                thumbnail.sprite = null;
            }

            // ÅØ½ºÆ®
            titleText.text = string.IsNullOrEmpty(data.title) ? "No Title" : data.title;
            dateText.text = string.IsNullOrEmpty(data.dateTime) ? "--/--/--" : data.dateTime;
            newIcon.SetActive(isNewest);
        }
        else
        {
            // ºó ½½·Ô Ç¥½Ã
            thumbnail.sprite = null;
            titleText.text = "ºó ½½·Ô";
            dateText.text = "";
            newIcon.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Image thumbnailImage;

    private string slotName;
    private System.Action<string> onClick;

    public void Init(string slotName, SaveData data, System.Action<string> onClick)
    {
        this.slotName = slotName;
        this.onClick = onClick;

        if (data != null)
        {
            titleText.text = string.IsNullOrEmpty(data.title) ? "¼¼ÀÌºê µ¥ÀÌÅÍ" : data.title;
            dateText.text = data.dateTime ?? "";

            if (!string.IsNullOrEmpty(data.thumbnailPath) && File.Exists(data.thumbnailPath))
            {
                byte[] bytes = File.ReadAllBytes(data.thumbnailPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                thumbnailImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                thumbnailImage.sprite = null;
            }
        }
        else
        {
            titleText.text = "ºó ½½·Ô";
            dateText.text = "";
            thumbnailImage.sprite = null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(slotName));
    }
}

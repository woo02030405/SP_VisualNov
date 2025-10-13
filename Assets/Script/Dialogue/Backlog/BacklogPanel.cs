using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BacklogPanel : MonoBehaviour
{
    public RectTransform content; // ScrollView/Viewport/Content
    public GameObject rowPrefab;  // Speaker/Text 2개의 TMP_Text 포함
    public Button closeButton;

    void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Show()
    {
        Refresh();
        gameObject.SetActive(true);
    }

    public void Refresh()
    {
        foreach (Transform c in content) Destroy(c.gameObject);

        var list = ReadSkipBacklogManager.Instance?.GetBacklog();
        if (list == null) return;

        foreach (var e in list)
        {
            var go = Instantiate(rowPrefab, content);
            var tmps = go.GetComponentsInChildren<TextMeshProUGUI>();
            // [0] = Speaker, [1] = Text 라고 가정
            if (tmps.Length >= 2)
            {
                tmps[0].text = string.IsNullOrEmpty(e.speaker) ? "" : e.speaker;
                tmps[1].text = e.text;
            }
        }

        // 스크롤을 맨 아래로
        var scroll = GetComponentInChildren<ScrollRect>();
        if (scroll) Canvas.ForceUpdateCanvases(); // 레이아웃 갱신
        if (scroll) scroll.verticalNormalizedPosition = 0f;
    }
}

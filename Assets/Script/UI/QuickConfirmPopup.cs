using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickConfirmPopup : MonoBehaviour
{
    public static QuickConfirmPopup Instance { get; private set; }

    [Header("Refs")]
    public CanvasGroup group;
    public TMP_Text messageText;
    public Button yesButton;
    public Button noButton;

    System.Action _onYes;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        HideImmediate();

        if (yesButton) yesButton.onClick.AddListener(() => { Hide(); _onYes?.Invoke(); _onYes = null; });
        if (noButton) noButton.onClick.AddListener(() => { Hide(); _onYes = null; });
    }

    public void Show(string message, System.Action onYes)
    {
        _onYes = onYes;
        if (messageText) messageText.text = message;
        if (group)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }

    void HideImmediate()
    {
        if (group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingOverlay : MonoBehaviour
{
    public static LoadingOverlay Instance { get; private set; }

    [Header("Refs")]
    public CanvasGroup group;
    public TMP_Text statusText;   // 선택
    public Slider progressBar;    // 선택

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        HideImmediate();
    }

    public void Show(string status = "Loading...")
    {
        if (statusText) statusText.text = status;
        if (progressBar) progressBar.value = 0f;

        if (group)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        gameObject.SetActive(true);
    }

    public void SetProgress(float p)
    {
        if (progressBar) progressBar.value = Mathf.Clamp01(p);
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

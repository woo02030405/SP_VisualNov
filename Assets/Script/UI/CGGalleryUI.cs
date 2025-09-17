using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using VN.Data; // CGRecord, CGCatalog ���

public class CGGalleryUI : MonoBehaviour
{
    [Header("Data")]
    public CGCatalog catalog;   // ScriptableObject (CG ������ ����)

    [Header("Slots (12 per page)")]
    public List<Button> cgButtons;         // ��ư 12��
    public List<Image> cgThumbs;           // �����
    public List<GameObject> lockOverlays;  // ��� ��������

    [Header("Preview (Ȯ�� ����)")]
    public GameObject previewPanel;   // Ȯ�� �� ��ü Panel
    public Image fullImage;           // ū �̹���
    public TMP_Text titleText;        // ����
    public Button closeButton;        // �ݱ� ��ư (X)

    [Header("Navigation")]
    public Button prevButton;
    public Button nextButton;
    public TMP_Text pageLabel;

    [Header("Canvas")]
    public Button backButton;         // GalleryHubScene�� ���ư��� ��ư

    [Header("Progress")]
    public TMP_Text progressText;

    private int currentPage = 0;
    private const int perPage = 12;
    private int TotalPages => Mathf.Max(1, Mathf.CeilToInt(catalog.items.Count / (float)perPage));

    private Color defaultLabelColor;

    void Start()
    {
        // ���� PageLabel �� ����
        defaultLabelColor = pageLabel.color;

        // ������ �̵� ��ư
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

        // �ݱ� ��ư �� Preview ���� �� �׺���̼� ����
        closeButton.onClick.AddListener(() =>
        {
            previewPanel.SetActive(false);

            prevButton.interactable = true;
            nextButton.interactable = true;
            backButton.interactable = true;
            pageLabel.color = defaultLabelColor;
        });

        // �ڷΰ��� ��ư �� ���θ޴� �� �ε�
        backButton.onClick.AddListener(() =>
        {
            var nav = FindObjectOfType<SceneNavigator>();
            if (nav != null) nav.Load("GalleryHubScene");
            // �Ǵ� UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        });

        RefreshAll();
    }

    void RefreshAll()
    {
        if (catalog == null || catalog.items.Count == 0)
        {
            pageLabel.text = "0 / 0";
            progressText.text = "Unlocked 0 / 0";
            return;
        }

        // ������ ��
        pageLabel.text = $"{currentPage + 1} / {TotalPages}";

        // ���൵
        int unlocked = 0;
        foreach (var c in catalog.items) if (c.unlocked) unlocked++;
        progressText.text = $"Unlocked {unlocked} / {catalog.items.Count}";

        // ���� ä���
        int start = currentPage * perPage;
        for (int i = 0; i < cgButtons.Count; i++)
        {
            int dataIndex = start + i;
            bool active = dataIndex < catalog.items.Count;
            cgButtons[i].gameObject.SetActive(active);
            if (!active) continue;

            var data = catalog.items[dataIndex];
            cgThumbs[i].sprite = data.thumbnail;
            lockOverlays[i].SetActive(!data.unlocked);

            cgButtons[i].interactable = data.unlocked;
            cgButtons[i].onClick.RemoveAllListeners();
            if (data.unlocked)
            {
                cgButtons[i].onClick.AddListener(() => ShowPreview(data));
            }
        }

        // ��ư ����
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < (TotalPages - 1);
    }

    void ShowPreview(CGRecord data)
    {
        previewPanel.SetActive(true);
        fullImage.sprite = data.fullImage;
        titleText.text = data.title;

        // �׺���̼� ���
        prevButton.interactable = false;
        nextButton.interactable = false;
        backButton.interactable = false;
        pageLabel.color = Color.gray;
    }
}

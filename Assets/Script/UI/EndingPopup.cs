using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class EndingPopup : MonoBehaviour
{
    [SerializeField] CanvasGroup panelCg;     // EndingPopup의 CanvasGroup
    [SerializeField] GameObject backdrop;     // Backdrop 오브젝트
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] Button confirmButton;
    [SerializeField] string mainMenuSceneName = "MainMenuScene";

    void Awake()
    {
        if (!panelCg) panelCg = GetComponent<CanvasGroup>();
        HideInstant();
    }

    public void Show(string title, string message)
    {
        titleText.text = title;
        messageText.text = message;

        // Backdrop 켜기(클릭 차단)
        if (backdrop) backdrop.SetActive(true);

        panelCg.alpha = 0f;
        panelCg.interactable = false;
        panelCg.blocksRaycasts = false;

        // 팝업 페이드/살짝 팝업
        transform.localScale = Vector3.one * 0.92f;
        DOTween.Sequence()
            .Join(panelCg.DOFade(1f, 0.35f))
            .Join(transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .OnComplete(() =>
            {
                panelCg.interactable = true;
                panelCg.blocksRaycasts = true;
            });

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            // 닫기 연출 후 메인으로
            DOTween.Sequence()
                .Join(panelCg.DOFade(0f, 0.25f))
                .Join(transform.DOScale(0.96f, 0.25f))
                .OnComplete(() =>
                {
                    HideInstant();
                    SceneManager.LoadScene(mainMenuSceneName);
                });
        });
    }

    void HideInstant()
    {
        if (backdrop) backdrop.SetActive(false);
        panelCg.alpha = 0f;
        panelCg.interactable = false;
        panelCg.blocksRaycasts = false;
    }
}

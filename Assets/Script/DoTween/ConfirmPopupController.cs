using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ConfirmPopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform dialog;
    [SerializeField] private TMP_Text message;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Animation Settings")]
    public float showDuration = 0.5f;
    public float hideDuration = 0.4f;
    public Ease showEase = Ease.OutCubic;
    public Ease hideEase = Ease.InCubic;
    public float slideOffset = 250f; // 얼마나 아래에서 올라올지 (픽셀 단위)

    private Vector2 defaultPos;
    private Tween currentTween;

    void Awake()
    {
        defaultPos = dialog.anchoredPosition;
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void Show(string msg, System.Action onYes = null, System.Action onNo = null)
    {
        // 초기화
        message.text = msg;
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;

        // 살짝 아래에 배치 후 올라오게
        dialog.anchoredPosition = new Vector2(defaultPos.x, defaultPos.y - slideOffset);
        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(group.DOFade(1, showDuration * 0.8f));
        seq.Join(dialog.DOAnchorPos(defaultPos, showDuration).SetEase(showEase));
        seq.OnComplete(() => {
            group.interactable = true;
            group.blocksRaycasts = true;
        });

        currentTween?.Kill();
        currentTween = seq;

        // 버튼 연결
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() => {
            onYes?.Invoke();
            HideToMouse();
        });

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() => {
            onNo?.Invoke();
            HideToMouse();
        });
    }

    public void HideToMouse()
    {
        if (!gameObject.activeSelf) return;
        group.interactable = false;
        group.blocksRaycasts = false;

        // 클릭 지점 좌표 변환
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, Input.mousePosition, null, out localMouse);

        Sequence seq = DOTween.Sequence();
        seq.Append(dialog.DOAnchorPos(localMouse, hideDuration).SetEase(hideEase));
        seq.Join(group.DOFade(0, hideDuration * 0.8f));
        seq.OnComplete(() => {
            gameObject.SetActive(false);
            dialog.anchoredPosition = defaultPos;
        });

        currentTween?.Kill();
        currentTween = seq;
    }
}

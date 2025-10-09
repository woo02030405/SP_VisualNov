using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[DisallowMultipleComponent]
public class ConfirmPopupAdapter : MonoBehaviour
{
    [Header("References (assign in prefab)")]
    [SerializeField] private CanvasGroup group;        // Root CanvasGroup
    [SerializeField] private RectTransform dialog;     // 팝업 패널 RectTransform
    [SerializeField] private TMP_Text message;         // (옵션) 메시지 텍스트
    [SerializeField] private Button yesButton;         // Yes 버튼
    [SerializeField] private Button noButton;          // No 버튼 (없으면 비워도 됨)

    [Header("Tween Settings")]
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.35f;
    [SerializeField] private float slideOffset = 300f; // 아래에서 얼마만큼 올라오게 할지(px)
    [SerializeField] private Ease showEase = Ease.OutCubic;
    [SerializeField] private Ease hideEase = Ease.InCubic;

    private Vector2 defaultPos;
    private Tween running;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        dialog = GetComponentInChildren<RectTransform>();
        yesButton = GetComponentInChildren<Button>();
    }

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (!dialog)
        {
            Debug.LogError("[ConfirmPopupAdapter] dialog RectTransform 참조가 없습니다.");
            enabled = false; return;
        }

        defaultPos = dialog.anchoredPosition;
        PrepareHidden();
    }

    void OnEnable()
    {
        // 매니저가 켜주면 자동으로 등장 트윈 실행
        PlayShow();
        // 버튼 리스너를 '우리 쪽'으로 갈아끼운 뒤, 트윈 끝에 매니저 로직 실행
        HookButtons();
    }

    void OnDisable()
    {
        running?.Kill();
        // 원상복귀
        dialog.anchoredPosition = defaultPos;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void PrepareHidden()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        dialog.anchoredPosition = new Vector2(defaultPos.x, defaultPos.y - slideOffset);
    }

    private void PlayShow()
    {
        running?.Kill();
        PrepareHidden();

        running = DOTween.Sequence()
            .Append(group.DOFade(1f, showDuration * 0.8f))
            .Join(dialog.DOAnchorPos(defaultPos, showDuration).SetEase(showEase))
            .OnComplete(() =>
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            });
    }

    private void HookButtons()
    {
        if (yesButton)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() => StartCoroutine(CloseThenConfirm(true)));
        }
        if (noButton)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() => StartCoroutine(CloseThenConfirm(false)));
        }
    }

    private IEnumerator CloseThenConfirm(bool isYes)
    {
        // 입력 잠금
        group.interactable = false;
        group.blocksRaycasts = false;

        // 클릭 지점으로 수축하며 사라짐
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, Input.mousePosition, null, out localMouse);

        running?.Kill();
        running = DOTween.Sequence()
            .Append(dialog.DOAnchorPos(localMouse, hideDuration).SetEase(hideEase))
            .Join(group.DOFade(0f, hideDuration * 0.9f));

        yield return running.WaitForCompletion();

        // 트윈 끝난 뒤, 매니저의 Hide + (Yes면) pendingYes 호출
        var mgr = FindObjectOfType<QuickSaveLoadManager>();
        if (mgr != null)
        {
            // private Action _pendingYes 에 접근
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var fi = typeof(QuickSaveLoadManager).GetField("_pendingYes", flags);
            var miHide = typeof(QuickSaveLoadManager).GetMethod("HideConfirm", flags);

            Action pending = null;
            if (fi != null) pending = fi.GetValue(mgr) as Action;

            // 먼저 HideConfirm()로 팝업 비활성화
            if (miHide != null) miHide.Invoke(mgr, null);

            // YES 선택이면 pendingYes 실행
            if (isYes && pending != null) pending.Invoke();
        }
        else
        {
            // 매니저가 없으면 그냥 닫기만
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using Game.OverlayUI;

public class BottomBarController : MonoBehaviour
{
    [Header("Buttons (RectTransform)")]
    [SerializeField] private RectTransform skipButton;
    [SerializeField] private RectTransform backlogButton;
    [SerializeField] private RectTransform saveButton;
    [SerializeField] private RectTransform loadButton;

    bool isSkipping;

    // 버튼 클릭 바인딩은 에디터에서 OnClick에 이 메서드들 연결해도 됨.
    public void OnClickSkip()
    {
        isSkipping = !isSkipping;
        var msg = isSkipping ? "스킵 중입니다…" : "스킵이 해제되었습니다";
        if (TooltipController.I) TooltipController.I.ShowAboveButton(skipButton, msg, 10f);

        // 실제 스킵 토글 로직은 별도 컨트롤러에 위임 권장
        // SkipModeController.I.Toggle();
    }

    public void OnHoverSkipEnter()
    {
        if (TooltipController.I) TooltipController.I.ShowAboveButton(skipButton, "이미 본 대사만 빠르게 넘깁니다", 10f);
    }

    public void OnHoverBacklogEnter()
    {
        if (TooltipController.I) TooltipController.I.ShowAboveButton(backlogButton, "대사 백로그", 10f);
    }

    public void OnHoverSaveEnter()
    {
        if (TooltipController.I) TooltipController.I.ShowAboveButton(saveButton, "세이브", 10f);
    }

    public void OnHoverLoadEnter()
    {
        if (TooltipController.I) TooltipController.I.ShowAboveButton(loadButton, "로드", 10f);
    }
}

using UnityEngine;
using DG.Tweening;

public class WarpStarChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        // 오버레이 패널 생성
        var overlay = new GameObject("WarpOverlay");
        var rt = overlay.AddComponent<RectTransform>();
        rt.SetParent(choicePanel.parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = overlay.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(1, 1, 1, 0);

        // 화면 덮으면서 warp 효과
        img.DOFade(1f, 0.5f).OnComplete(() =>
        {
            img.DOFade(0f, 1.5f).SetDelay(1f).OnComplete(() =>
            {
                GameObject.Destroy(overlay);
                GameObject.Destroy(selected);
                onSelected?.Invoke(next);
            });
        });
    }
}

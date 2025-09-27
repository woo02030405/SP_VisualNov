using UnityEngine;
using DG.Tweening;

public class FollowMouseChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        selected.transform.SetAsLastSibling();
        var rt = selected.GetComponent<RectTransform>();
        // 5초 후에 선택지 제거 및 다음으로 이동
        DOVirtual.DelayedCall(5f, () =>
        {
            GameObject.Destroy(selected);
            onSelected?.Invoke(next);
        });

        // 5초 동안 마우스를 따라다니게 함
        DOVirtual.Float(0, 1, 5f, t =>
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                choicePanel as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, pos, 0.3f);
        });
    }
}

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RunAwayChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        var btn = selected.GetComponent<Button>();
        btn.onClick.RemoveAllListeners(); // 클릭 못하게 차단

        var rt = selected.GetComponent<RectTransform>();

        // 랜덤한 위치로 순간이동 (choicePanel 내부)
        Vector2 randomPos = new Vector2(Random.Range(-300, 300), Random.Range(-150, 150));
        rt.DOLocalMove(randomPos, 0.3f).SetEase(Ease.OutQuad);

        // 여기서는 onSelected 호출 안 함 (사용자가 못 고르게끔)
        Debug.Log("[RunAway] 버튼이 도망갔습니다!");
    }
}

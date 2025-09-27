using UnityEngine;
using DG.Tweening;

public class BulletChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        var rt = selected.GetComponent<RectTransform>();
        var start = new Vector3(0, -500, 0); // 총알 출발점 (화면 아래)
        var end = rt.position;

        var bullet = new GameObject("Bullet");
        var bulletRT = bullet.AddComponent<RectTransform>();
        bulletRT.SetParent(choicePanel, false);
        bulletRT.sizeDelta = new Vector2(10, 10);
        bulletRT.position = start;

        bulletRT.DOMove(end, 0.3f).SetEase(Ease.Linear).OnComplete(() =>// 총알이 도착하면 선택지 흔들리고 제거
        {
            GameObject.Destroy(bullet);
            rt.DOShakePosition(0.3f, 10, 20).OnComplete(() =>// 흔들기 끝나면 선택지 제거 및 다음으로 이동
            {
                GameObject.Destroy(selected);
                onSelected?.Invoke(next);
            });
        });
    }
}

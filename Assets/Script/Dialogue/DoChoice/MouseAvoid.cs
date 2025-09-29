using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class MouseAvoid : MonoBehaviour
{
    public RectTransform bounds;   // 보통 ChoiceContainer의 RectTransform
    public float triggerRadius = 80f;
    public float evadeDistance = 60f;
    public float tweenTime = 0.15f;

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!bounds) return;

        // 스크린 좌표 → bounds 기준 로컬 좌표로 변환
        Vector2 mouseLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, Input.mousePosition, null, out mouseLocal);

        Vector2 myLocal = bounds.InverseTransformPoint(rt.position);
        float dist = Vector2.Distance(mouseLocal, myLocal);
        if (dist > triggerRadius) return;

        Vector2 dir = (myLocal - mouseLocal).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Random.insideUnitCircle.normalized;

        Vector2 targetLocal = myLocal + dir * evadeDistance;

        // 경계 클램프
        var half = bounds.rect.size * 0.5f;
        targetLocal.x = Mathf.Clamp(targetLocal.x, -half.x + rt.rect.width * 0.5f, half.x - rt.rect.width * 0.5f);
        targetLocal.y = Mathf.Clamp(targetLocal.y, -half.y + rt.rect.height * 0.5f, half.y - rt.rect.height * 0.5f);

        Vector3 world = bounds.TransformPoint(targetLocal);
        rt.DOMove(world, tweenTime).SetEase(Ease.OutQuad);
    }
}

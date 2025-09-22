using UnityEngine;

/// RectTransform을 위/아래로 살짝 흔들어 주는 스크립트 (UI 전용)
[RequireComponent(typeof(RectTransform))]
public class UIBob : MonoBehaviour
{
    public float amplitude = 8f;   // 위/아래 거리
    public float period = 1.2f;    // 한 번 왕복 시간(초)
    public float phaseOffset = 0f; // 시작 위상(여러 개를 어긋나게)

    private RectTransform rt;
    private Vector2 basePos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        basePos = rt.anchoredPosition;
    }

    void OnEnable() => basePos = rt.anchoredPosition;

    void Update()
    {
        float w = (period <= 0.01f) ? 1f : (2f * Mathf.PI / period);
        float y = Mathf.Sin((Time.unscaledTime + phaseOffset) * w) * amplitude;
        rt.anchoredPosition = new Vector2(basePos.x, basePos.y + y);
    }
}

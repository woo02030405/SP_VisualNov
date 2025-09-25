using UnityEngine;

/// RectTransform�� ��/�Ʒ��� ��¦ ���� �ִ� ��ũ��Ʈ (UI ����)
[RequireComponent(typeof(RectTransform))]
public class UIBob : MonoBehaviour
{
    public float amplitude = 8f;   // ��/�Ʒ� �Ÿ�
    public float period = 1.2f;    // �� �� �պ� �ð�(��)
    public float phaseOffset = 0f; // ���� ����(���� ���� ��߳���)

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

using UnityEngine;
using UnityEngine.UI;

public class SkipController : MonoBehaviour
{
    public DialogueManager dm;
    public Toggle skipToggle;      // "스킵" 토글 버튼 (옵션)
    public bool holdToSkip = true; // 홀드-스킵 허용
    public KeyCode skipKey = KeyCode.LeftControl;

    [Tooltip("스킵 틱 간격(초)")] public float skipTick = 0.02f;
    private float _acc;

    void Start()
    {
        if (!dm) dm = FindObjectOfType<DialogueManager>();
        if (skipToggle) skipToggle.onValueChanged.AddListener(OnSkipToggle);
    }

    void OnSkipToggle(bool on) { /* UI표시만, 실제 로직은 Update에서 */ }

    void Update()
    {
        if (!dm) return;
        bool request = (skipToggle && skipToggle.isOn) || (holdToSkip && Input.GetKey(skipKey));
        if (!request) return;

        _acc += Time.unscaledDeltaTime;
        if (_acc < skipTick) return;
        _acc = 0f;

        // 안전 게이트: 읽은 노드만 스킵
        if (dm.CanSkipCurrent())
        {
            // 현재 타이핑 중이면 먼저 완결
            dm.Next();
        }
    }
}

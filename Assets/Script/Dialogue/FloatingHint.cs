using UnityEngine;
using TMPro;

/// <summary>
/// 임시 플레이스홀더. 지금은 시각 효과 없이 컴파일만 되게 함.
/// 나중에 실제 이펙트로 교체하면 됨.
/// </summary>
public class FloatingHint : MonoBehaviour
{
    [Tooltip("선택사항: 텍스트를 보여줄 TMP_Text가 있다면 연결")]
    public TMP_Text text;

    /// <summary>
    /// 힌트를 재생. 지금은 로그만 남기고 아무 것도 하지 않음.
    /// </summary>
    public void Play(string message)
    {
        if (text != null) text.text = message;
        Debug.Log($"[FloatingHint] {message}");
        // 실제 구현에서는 DOTween으로 위로 이동 + 페이드아웃 등을 넣으면 됨.
    }
}

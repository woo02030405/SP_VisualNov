using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class UIPopupAnimator : MonoBehaviour
{
    [Header("Animator (attach to Root)")]
    public Animator animator;               // Root에 붙은 Animator
    [Header("Trigger names in Animator")]
    public string showTrigger = "show";
    public string hideTrigger = "hide";     // 없어도 됨 (빈 칸이면 무시)

    [Header("Events (optional)")]
    public UnityEvent onShown;              // Show 클립 끝에서 AnimationEvent로 호출
    public UnityEvent onHidden;             // Hide 클립 끝에서 AnimationEvent로 호출

    void Reset()
    {
        animator = GetComponent<Animator>();
        showTrigger = "show";
        hideTrigger = "hide";
    }

    void OnEnable()
    {
        // 활성화되면 바로 show 트리거 (Show가 Default면 생략해도 됨)
        if (animator && !string.IsNullOrEmpty(showTrigger))
            animator.SetTrigger(showTrigger);
    }

    // 매니저가 굳이 안 불러줘도 되지만, 원하면 직접 호출 가능
    public void PlayShow()
    {
        if (animator && !string.IsNullOrEmpty(showTrigger))
            animator.SetTrigger(showTrigger);
    }

    public void PlayHide()
    {
        if (animator && !string.IsNullOrEmpty(hideTrigger))
            animator.SetTrigger(hideTrigger);
    }

    // === Animation Event 용도로 Show/Hid 끝에 달아줌 ===
    public void _AnimEvent_Shown() { onShown?.Invoke(); }
    public void _AnimEvent_Hidden() { onHidden?.Invoke(); }
}

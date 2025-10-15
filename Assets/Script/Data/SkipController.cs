using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 읽은 대사만 스킵 + 선택지에서 멈춤 + 아무 입력 시 정지 + 오버레이 깜빡임.
/// </summary>
public class SkipController : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;
    public Button skipButton;             // BottomBar/SkipButton
    public GameObject skipOverlay;        // SkipOverlay 전체 오브젝트
    public CanvasGroup overlayGroup;      // SkipOverlay CanvasGroup (투명도 제어)
    public Graphic blinkTarget;           // 깜빡이게 할 Text 또는 Image

    [Header("Settings")]
    public KeyCode holdKey = KeyCode.LeftControl; // 스킵 키 (눌러야 스킵)
    public float skipDelay = 0.02f;               // 스킵 간격 (초)
    public bool allowAllSkip = false;             // 읽은 대사뿐 아니라 모든 대사 스킵 허용 여부
    public float blinkSpeed = 0.6f;               // 깜빡임 속도 (초)

    private bool _skipActive;
    private float _timer;
    private Tween _blinkTween;

    void Start()
    {
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (skipButton)
            skipButton.onClick.AddListener(OnClickSkipButton);

        if (skipOverlay)
            skipOverlay.SetActive(false); // 평소엔 비활성화
    }

    void OnClickSkipButton()
    {
        if (!_skipActive)
        {
            StartSkip();
        }
    }

    void Update()
    {
        if (!_skipActive && !Input.GetKey(holdKey)) return;

        bool hold = Input.GetKey(holdKey);
        bool wantSkip = _skipActive || hold;

        if (wantSkip)
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= skipDelay)
            {
                _timer = 0f;
                TrySkipOnce();
            }

            // 아무 키 / 마우스 클릭 시 스킵 종료
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                StopSkip();
            }
        }
    }

    private void TrySkipOnce()
    {
        if (!dialogueManager) return;

        // 선택지 UI가 활성화되어 있으면 즉시 스킵 종료
        if (dialogueManager.dialogueUI.IsChoiceActive)
        {
            StopSkip();
            return;
        }

        // 타이핑 중이면 완결
        if (dialogueManager.dialogueUI.IsTyping())
        {
            dialogueManager.dialogueUI.CompleteTyping();
            return;
        }

        // 읽지 않은 대사면 멈춤
        bool canSkip = allowAllSkip || dialogueManager.CanSkipCurrent();
        if (!canSkip)
        {
            StopSkip();
            return;
        }

        dialogueManager.Next();
    }

    private void StartSkip()
    {
        _skipActive = true;
        _timer = 0f;

        if (skipOverlay)
        {
            skipOverlay.SetActive(true);
            overlayGroup.alpha = 0f;
            overlayGroup.DOFade(1f, 0.3f);
        }

        // 깜빡임 효과 시작
        if (blinkTarget)
        {
            _blinkTween?.Kill();
            _blinkTween = blinkTarget.DOFade(0.2f, blinkSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    private void StopSkip()
    {
        if (!_skipActive) return;

        _skipActive = false;
        _blinkTween?.Kill();

        // 오버레이 서서히 사라지고 비활성화
        if (skipOverlay && overlayGroup)
        {
            overlayGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                skipOverlay.SetActive(false);
            });
        }
        else if (skipOverlay)
        {
            skipOverlay.SetActive(false);
        }
    }
}

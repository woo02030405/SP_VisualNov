using UnityEngine;
using UnityEngine.UI;

/// 여러 장의 스프라이트를 UI Image에 순서대로 재생 (GIF처럼)
[RequireComponent(typeof(Image))]
[AddComponentMenu("VN/UI/UI Flipbook Animator")]
public class UIFlipbookAnimator : MonoBehaviour
{
    public Sprite[] frames;
    [Range(1, 60)] public int fps = 10;
    public bool loop = true;
    public bool playOnEnable = true;
    public bool ignoreTimeScale = true;

    private Image img;
    private float timer;
    private int index;
    private bool playing;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void OnEnable()
    {
        index = 0;
        timer = 0f;
        playing = playOnEnable && frames != null && frames.Length > 0;

        if (img && frames != null && frames.Length > 0)
            img.sprite = frames[0];
    }

    void Update()
    {
        if (!playing || frames == null || frames.Length == 0 || fps <= 0) return;

        float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        timer += dt;

        float frameTime = 1f / fps;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            index++;
            if (index >= frames.Length)
            {
                if (loop) index = 0;
                else { playing = false; index = frames.Length - 1; break; }
            }
            img.sprite = frames[index];
        }
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0) return;
        playing = true;
    }

    public void Pause() => playing = false;

    public void Stop()
    {
        playing = false;
        index = 0;
        timer = 0f;
        if (img && frames != null && frames.Length > 0)
            img.sprite = frames[0];
    }
}

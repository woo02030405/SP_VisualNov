using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EffectManager : MonoBehaviour
{
    [Header("Overlay UI")]
    [SerializeField] private Image fadePanel;      // 화면 전체 덮는 검은 패널
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0, 0, 0, 0);
            fadePanel.gameObject.SetActive(true);
        }
    }

    // 🔹 BGM 재생
    public void PlayBGM(string bgmName)
    {
        if (string.IsNullOrEmpty(bgmName)) return;

        AudioClip clip = Resources.Load<AudioClip>($"BGM/{bgmName}");
        if (clip == null)
        {
            Debug.LogWarning($"[EffectManager] BGM not found: {bgmName}");
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 🔹 효과음 재생
    public void PlaySFX(string sfxName)
    {
        if (string.IsNullOrEmpty(sfxName)) return;

        AudioClip clip = Resources.Load<AudioClip>($"SFX/{sfxName}");
        if (clip == null)
        {
            Debug.LogWarning($"[EffectManager] SFX not found: {sfxName}");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // 🔹 화면 전환
    public void Transition(string type, float duration = 0.5f)
    {
        if (fadePanel == null) return;

        switch (type?.ToLower())
        {
            case "fade":
                fadePanel.color = new Color(0, 0, 0, 1);
                fadePanel.DOFade(0, duration);
                break;

            case "fadeout":
                fadePanel.color = new Color(0, 0, 0, 0);
                fadePanel.DOFade(1, duration);
                break;

            case "flash":
                fadePanel.color = new Color(1, 1, 1, 1);
                fadePanel.DOFade(0, duration);
                break;

            case "slide":
                // TODO: 슬라이드 전환 (패널 위치 이동 연출)
                Debug.Log("[EffectManager] Slide transition not implemented");
                break;

            default:
                // transition 없음
                break;
        }
    }
}

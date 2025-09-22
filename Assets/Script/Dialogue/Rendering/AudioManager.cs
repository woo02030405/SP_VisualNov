using UnityEngine;

namespace VN.Rendering
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        public void PlayBGM(string bgmId)
        {
            var clip = Resources.Load<AudioClip>($"BGM/{bgmId}");
            if (clip == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void PlaySFX(string sfxId)
        {
            var clip = Resources.Load<AudioClip>($"SFX/{sfxId}");
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }
    }
}

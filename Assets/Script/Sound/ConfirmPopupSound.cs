using UnityEngine;

public class ConfirmPopupSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip showClip;
    public AudioClip hideClip;

    // Animation Event에서 호출할 함수
    public void PlayShowSound()
    {
        if (audioSource && showClip)
            audioSource.PlayOneShot(showClip);
    }

    public void PlayHideSound()
    {
        if (audioSource && hideClip)
            audioSource.PlayOneShot(hideClip);
    }
}

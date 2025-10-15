// Assets/Script/Dialogue/Backlog/BacklogItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Dialogue
{
    public class BacklogItemUI : MonoBehaviour
    {
        [Header("TMP References")]
        [SerializeField] private TextMeshProUGUI speakerTMP;
        [SerializeField] private TextMeshProUGUI contentTMP;

        [Header("Play (Optional)")]
        [SerializeField] private Button playButton;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private AudioSource voiceSource;

        private AudioClip voiceClip;

        public void Setup(BacklogEntry entry)
        {
            if (entry == null) return;
            Setup(entry.Speaker, entry.Text, entry.VoiceClip);
        }

        public void Setup(string speaker, string text, AudioClip voice = null)
        {
            if (speakerTMP) speakerTMP.text = string.IsNullOrEmpty(speaker) ? "" : $"{speaker} :";
            if (contentTMP) contentTMP.text = text ?? "";

            voiceClip = voice;
            if (playButton)
            {
                bool has = voiceClip != null;
                playButton.gameObject.SetActive(has);
                playButton.interactable = has;
                playButton.onClick.RemoveAllListeners();
                if (has) playButton.onClick.AddListener(OnClickPlayVoice);
            }
        }

        private void OnClickPlayVoice()
        {
            if (!voiceClip) return;
            if (!voiceSource)
            {
                voiceSource = GetComponent<AudioSource>();
                if (!voiceSource) voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
                voiceSource.spatialBlend = 0f;
            }
            voiceSource.Stop();
            voiceSource.clip = voiceClip;
            voiceSource.volume = volume;
            voiceSource.loop = false;
            voiceSource.Play();
        }

        private void OnDisable()
        {
            if (voiceSource && voiceSource.isPlaying) voiceSource.Stop();
        }
    }
}

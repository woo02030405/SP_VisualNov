using UnityEngine;
using VN.Dialogue;

namespace VN.Rendering
{
    public class StreamingResourceManager : MonoBehaviour
    {
        [SerializeField] private BackgroundManager backgroundManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private CharacterManager characterManager;
        [SerializeField] private EventCGManager eventCGManager;
        [SerializeField] private TransitionManager transitionManager;

        public void Apply(DialogueNode node)
        {
            if (node == null) return;

            if (!string.IsNullOrEmpty(node.Background))
                backgroundManager.ApplyBackground(node.Background);

            if (!string.IsNullOrEmpty(node.BGM))
                audioManager.PlayBGM(node.BGM);

            if (!string.IsNullOrEmpty(node.SFX))
                audioManager.PlaySFX(node.SFX);

            if (!string.IsNullOrEmpty(node.CharacterId))
                characterManager.ApplyCharacters(node);

            if (!string.IsNullOrEmpty(node.EventCG))
                eventCGManager.ShowEventCG(node.EventCG);

            if (!string.IsNullOrEmpty(node.Transition))
                transitionManager.PlayTransition(node.Transition);
        }
    }
}

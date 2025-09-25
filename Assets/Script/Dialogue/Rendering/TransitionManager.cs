using UnityEngine;

namespace VN.Rendering
{
    public class TransitionManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlay;

        public void PlayTransition(string type)
        {
            if (overlay == null) return;

            switch (type.ToLowerInvariant())
            {
                case "fade":
                    overlay.alpha = 1f;
                    // TODO: DOTween fade-out���� �ε巴��
                    break;
                case "flash":
                    overlay.alpha = 1f;
                    // TODO: DOTween ���� fade-out
                    break;
                case "dim":
                    overlay.alpha = 0.5f;
                    break;
                default:
                    Debug.LogWarning($"[TransitionManager] Unknown transition: {type}");
                    break;
            }
        }

        public void Clear()
        {
            if (overlay != null)
                overlay.alpha = 0f;
        }
    }
}

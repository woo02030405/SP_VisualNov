using UnityEngine;
using UnityEngine.UI;

namespace VN.Rendering
{
    public class BackgroundManager : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;

        public void ApplyBackground(string bgId)
        {
            var sprite = Resources.Load<Sprite>($"Backgrounds/{bgId}");
            if (sprite != null && backgroundImage != null)
                backgroundImage.sprite = sprite;
            else
                Debug.LogWarning($"[BackgroundManager] Background not found: {bgId}");
        }
    }
}

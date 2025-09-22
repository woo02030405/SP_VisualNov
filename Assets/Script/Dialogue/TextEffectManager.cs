using UnityEngine;
using TMPro;
#if DOTWEEN
using DG.Tweening;
#endif

namespace VN.Rendering
{
    public class TextEffectManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text dialogueText;

        public void ApplyEffect(string effectId)
        {
            if (dialogueText == null || string.IsNullOrEmpty(effectId)) return;

            string[] effects = effectId.Split(';');
            foreach (var e in effects)
            {
                string token = e.Trim().ToLowerInvariant();
                switch (token)
                {
                    case "shake":
#if DOTWEEN
                        dialogueText.rectTransform.DOShakeAnchorPos(0.4f, strength: new Vector2(5, 0), vibrato: 20);
#else
                        Debug.Log("[TextEffect] shake (DOTween not enabled)");
#endif
                        break;

                    case "fade":
#if DOTWEEN
                        dialogueText.alpha = 0f;
                        dialogueText.DOFade(1f, 0.5f);
#else
                        dialogueText.alpha = 1f;
#endif
                        break;

                    case "morendo": // 점점 사라지는 듯한 효과
#if DOTWEEN
                        dialogueText.DOFade(0f, 2f).SetEase(Ease.InCubic);
#else
                        dialogueText.alpha = 0f;
#endif
                        break;

                    case "dim": // 살짝 어둡게
                        dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 0.5f);
                        break;

                    case "clear":
                        dialogueText.text = "";
                        break;

                    default:
                        Debug.LogWarning($"[TextEffectManager] Unknown effect: {token}");
                        break;
                }
            }
        }
    }
}

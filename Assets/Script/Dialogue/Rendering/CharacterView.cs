using UnityEngine;
using UnityEngine.UI;

#if DOTWEEN
using DG.Tweening;
#endif

namespace VN.Rendering
{
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private Image faceImage;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform rect;

        public RectTransform Rect => rect;
        public int ZOrder { get; set; }

        public void ApplySprite(Sprite sprite)
        {
            if (faceImage != null)
            {
                faceImage.sprite = sprite;
                faceImage.enabled = (sprite != null);
            }
        }

        public void ApplyPosition(string posToken)
        {
            if (rect == null) return;

            var parts = posToken.Split('|');
            string h = parts.Length > 0 ? parts[0] : "center";
            string depth = parts.Length > 1 ? parts[1] : "middle";

            float x = h switch
            {
                "left" => -450f,
                "center" or "middle" => 0f,
                "right" => 450f,
                _ => 0f
            };

#if DOTWEEN
            rect.DOAnchorPosX(x, 0.25f).SetEase(Ease.OutCubic);
#else
            rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
#endif

            // ±íÀÌ ¼³Á¤
            switch (depth)
            {
                case "back": ZOrder = 0; SetScale(0.9f); SetAlpha(0.7f); break;
                case "middle": ZOrder = 1; SetScale(1.0f); SetAlpha(1f); break;
                case "front": ZOrder = 2; SetScale(1.1f); SetAlpha(1f); break;
                default: ZOrder = 1; break;
            }
        }

        public void SetScale(float scale)
        {
            if (rect != null)
                rect.localScale = Vector3.one * scale;
        }

        public void SetAlpha(float a)
        {
            if (group != null) group.alpha = a;
        }

        public void PlayEffect(string effect, System.Action onComplete = null)
        {
            if (string.IsNullOrEmpty(effect)) return;

            switch (effect.ToLowerInvariant())
            {
                case "fadein":
#if DOTWEEN
                    group.alpha = 0f;
                    group.DOFade(1f, 0.3f).OnComplete(() => onComplete?.Invoke());
#else
                    SetAlpha(1f);
                    onComplete?.Invoke();
#endif
                    break;
                case "fadeout":
#if DOTWEEN
                    group.DOFade(0f, 0.3f).OnComplete(() => onComplete?.Invoke());
#else
                    SetAlpha(0f);
                    onComplete?.Invoke();
#endif
                    break;
            }
        }

        public void PlayAnimation(string anim)
        {
            if (string.IsNullOrEmpty(anim)) return;

            switch (anim.ToLowerInvariant())
            {
                case "shake":
#if DOTWEEN
                    rect.DOShakeAnchorPos(0.4f, strength: new Vector2(20, 0), vibrato: 20, randomness: 90);
#else
                    rect.anchoredPosition += new Vector2(10f, 0f);
#endif
                    break;

                case "blink":
                    StartCoroutine(BlinkCoroutine(0.15f, 2));
                    break;
            }
        }

        private System.Collections.IEnumerator BlinkCoroutine(float onOffTime, int count)
        {
            if (group == null) yield break;
            for (int i = 0; i < count; i++)
            {
                group.alpha = 0f;
                yield return new WaitForSeconds(onOffTime);
                group.alpha = 1f;
                yield return new WaitForSeconds(onOffTime);
            }
        }
    }
}

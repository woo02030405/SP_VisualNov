using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Game.OverlayUI
{
    /// 버튼 누를 때 살짝 튀고/반짝이는 귀여운 효과
    public class ButtonCuteFX : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public float pressScale = 0.92f;
        public float releaseOvershoot = 1.06f;
        public float pressTime = 0.06f;
        public float releaseTime = 0.18f;
        public float hoverPunch = 0.03f;

        RectTransform rt;
        Tween tw;

        void Awake()
        {
            rt = transform as RectTransform;
            if (rt) rt.localScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            tw?.Kill();
            tw = rt.DOScale(pressScale, pressTime).SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            tw?.Kill();
            rt.localScale = Vector3.one * pressScale;
            tw = rt.DOScale(releaseOvershoot, releaseTime).SetEase(Ease.OutBack)
                   .OnComplete(() => rt.DOScale(1f, 0.08f));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!rt) return;
            rt.DOPunchScale(Vector3.one * hoverPunch, 0.15f, 12, 0.9f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 살짝 정리
            tw?.Kill();
            if (rt) rt.DOScale(1f, 0.08f);
        }
    }
}

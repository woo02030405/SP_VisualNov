using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Game.OverlayUI
{
    public class TopRightPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Fade")]
        public CanvasGroup group;
        public float hoverAlpha = 1f;
        public float idleAlpha = 0.5f;
        public float fadeTime = 0.2f;

        [Header("Auto")]
        public Button autoButton;
        public AutoModeController autoController;
        public Image autoRedLamp;
        public OrbitGlow orbitFX;      // OrbitRoot에 붙임
        public OrbitGlowTrail orbitTrail;   // (선택) 잔상 스포너

        [Header("Menu")]
        public Button menuButton;
        public OverlayMenuController menuController;

        void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
            if (group) group.alpha = idleAlpha;

            if (autoButton) autoButton.onClick.AddListener(ToggleAuto);
            if (menuButton && menuController) menuButton.onClick.AddListener(() => menuController.Toggle());

            SetAutoVisual(false);
        }

        public void OnPointerEnter(PointerEventData _) => FadeTo(hoverAlpha);
        public void OnPointerExit(PointerEventData _) => FadeTo(idleAlpha);
        void FadeTo(float a) { if (group) group.DOFade(a, fadeTime); }

        void ToggleAuto()
        {
            if (!autoController) return;
            bool on = autoController.Toggle();
            SetAutoVisual(on);
        }

        void SetAutoVisual(bool on)
        {
            if (autoRedLamp) autoRedLamp.enabled = on;

            //  공전/잔상 FX 활성화 (여기서만 켜고 끄세요)
            if (orbitFX)
            {
                orbitFX.SetActive(on);
                Debug.Log($"[AutoFX] Orbit {(on ? "ON" : "OFF")} (dot={orbitFX.orbitDot}, root={orbitFX.orbitRoot})");
            }
            if (orbitTrail) orbitTrail.SetActive(on);
        }
    }
}

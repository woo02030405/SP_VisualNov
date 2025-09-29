using UnityEngine;
using DG.Tweening;

namespace Game.OverlayUI
{
    /// BorderGlow(정적 링) 주위를 OrbitDot(빨간 점)만 공전시키는 컨트롤러
    public class OrbitGlow : MonoBehaviour
    {
        [Header("Refs")]
        public RectTransform orbitRoot;   // 회전축(이 스크립트가 붙은 오브젝트를 권장)
        public RectTransform orbitDot;    // 빨간 점
        public RectTransform borderGlow;  // 정적 링(옵션)

        [Header("Orbit")]
        public float radius = 48f;        // 점의 공전 반지름
        public float secondsPerLap = 1.2f;
        public bool clockwise = true;

        private Tween _tw;

        void Reset()
        {
            orbitRoot = transform as RectTransform;
        }

        /// Auto ON/OFF에 맞춰 호출
        public void SetActive(bool on)
        {
            if (on) Play();
            else Stop();

            if (orbitDot) orbitDot.gameObject.SetActive(on);
            if (borderGlow) borderGlow.gameObject.SetActive(on);
        }

        public void Play()
        {
            if (!orbitRoot || !orbitDot) return;

            // 점을 반지름 위치로 이동(오른쪽)
            orbitDot.anchoredPosition = new Vector2(radius, 0f);

            // 이전 트윈 정리
            _tw?.Kill();
            orbitRoot.localRotation = Quaternion.identity;

            float angle = clockwise ? -360f : 360f;
            _tw = orbitRoot
                .DOLocalRotate(new Vector3(0, 0, angle), secondsPerLap, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        public void Stop()
        {
            _tw?.Kill();
            _tw = null;
            if (orbitRoot) orbitRoot.localRotation = Quaternion.identity;
        }
    }
}

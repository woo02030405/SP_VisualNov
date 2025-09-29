using UnityEngine;
using DG.Tweening;

namespace Game.OverlayUI
{
    /// OrbitDot의 현재 위치에 가벼운 잔상 버블을 주기적으로 찍어 페이드아웃
    public class OrbitGlowTrail : MonoBehaviour
    {
        [Header("Refs")]
        public RectTransform orbitDot;          // 빨간 점
        public RectTransform spawnParent;       // 보통 OrbitRoot 또는 Auto 버튼
        public RectTransform trailPrefab;       // OrbitTrailBubble 프리팹(아래 설명대로 제작)

        [Header("Trail Params")]
        public float interval = 0.08f;          // 생성 주기(낮을수록 진한 잔상)
        public float life = 0.45f;              // 잔상 유지시간
        public float startScale = 0.9f;         // 생성 시 스케일
        public float endScale = 1.2f;           // 사라질 때 스케일
        public float drift = 8f;                // 바깥쪽으로 번지는 거리

        private float _t;
        private bool _on;

        public void SetActive(bool on)
        {
            _on = on;
            _t = 0f;
        }

        void Update()
        {
            if (!_on || !orbitDot || !trailPrefab) return;

            _t += Time.deltaTime;
            if (_t >= interval)
            {
                _t = 0f;
                SpawnOne();
            }
        }

        void SpawnOne()
        {
            var tr = Instantiate(trailPrefab, spawnParent ? spawnParent : orbitDot.parent);
            var rt = tr.GetComponent<RectTransform>();
            var cg = tr.GetComponent<CanvasGroup>();

            if (cg) cg.alpha = 0.65f;

            // 현재 점의 월드 좌표 복사
            rt.position = orbitDot.position;
            rt.localScale = Vector3.one * startScale;

            // 점의 '바깥쪽'으로 드리프트 (현재 로컬 오른쪽 방향)
            Vector3 driftDir = orbitDot.right;
            Vector2 targetPos = (Vector2)rt.anchoredPosition + (Vector2)(driftDir * drift);

            // 트윈: 스케일업 + 약간 이동 + 페이드아웃 후 삭제
            var seq = DOTween.Sequence();
            seq.Join(rt.DOScale(endScale, life).SetEase(Ease.OutSine));
            seq.Join(rt.DOAnchorPos(targetPos, life).SetEase(Ease.OutSine));
            if (cg) seq.Join(cg.DOFade(0f, life).SetEase(Ease.InSine));
            seq.OnComplete(() => Destroy(tr.gameObject));
        }
    }
}

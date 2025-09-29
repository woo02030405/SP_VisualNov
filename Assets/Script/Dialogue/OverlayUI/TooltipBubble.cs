using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Game.OverlayUI
{
    public class TooltipBubble : MonoBehaviour
    {
        public CanvasGroup cg;
        public RectTransform panel;
        public TMP_Text text;

        [Header("Motion")]
        public float rise = 40f;       // 위로 이동량
        public float showTime = 0.9f;  // 보여주는 시간
        public float fadeTime = 0.2f;

        void Awake()
        {
            if (!cg) cg = GetComponent<CanvasGroup>();
            if (cg) cg.alpha = 0f;
            if (panel) panel.localScale = Vector3.one * 0.9f;
        }

        public void Play(string msg)
        {
            if (text) text.text = msg;

            Sequence s = DOTween.Sequence();
            if (cg) s.Append(cg.DOFade(1f, 0.12f));
            if (panel) s.Join(panel.DOScale(1f, 0.18f).SetEase(Ease.OutBack));

            var start = panel ? panel.anchoredPosition : Vector2.zero;
            var end = start + new Vector2(0f, rise);

            if (panel) s.Join(panel.DOAnchorPos(end, showTime).SetEase(Ease.OutSine));

            s.AppendInterval(0.05f);
            if (cg) s.Append(cg.DOFade(0f, fadeTime));
            s.OnComplete(() => Destroy(gameObject));
        }
    }
}

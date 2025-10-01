using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.OverlayUI
{
    public class TooltipController : MonoBehaviour
    {
        public static TooltipController I;

        [SerializeField] private GameObject tooltipPrefab;
        private GameObject currentTooltip;
        private RectTransform rect;
        private TextMeshProUGUI text;

        private void Awake()
        {
            I = this;
        }

        public void ShowAboveButton(RectTransform target, string message, float offsetY = 10f)
        {
            if (currentTooltip == null)
            {
                currentTooltip = Instantiate(tooltipPrefab, transform);
                rect = currentTooltip.GetComponent<RectTransform>();
                text = currentTooltip.GetComponentInChildren<TextMeshProUGUI>();
            }

            text.text = message;
            currentTooltip.SetActive(true);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            // TooltipController가 붙은 Canvas RectTransform
            RectTransform canvasRect = transform as RectTransform;
            if (canvasRect == null)
                canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            // 버튼 중심 → Canvas 로컬 좌표
            Vector2 localPoint = canvasRect.InverseTransformPoint(target.position);

            // 버튼 위로 올리기
            localPoint.y += target.rect.height + offsetY;

            // Clamp: 화면 밖으로 나가지 않고 벽에 붙임
            float halfWidth = rect.rect.width * 0.5f;
            float halfHeight = rect.rect.height * 0.5f;

            float minX = -canvasRect.rect.width / 2 + halfWidth;
            float maxX = canvasRect.rect.width / 2 - halfWidth;
            float minY = -canvasRect.rect.height / 2 + halfHeight;
            float maxY = canvasRect.rect.height / 2 - halfHeight;

            localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
            localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

            rect.pivot = new Vector2(0.5f, 0); // 아래쪽 중앙
            rect.anchoredPosition = localPoint;
        }

        public void Hide()
        {
            if (currentTooltip != null)
                currentTooltip.SetActive(false);
        }
    }
}

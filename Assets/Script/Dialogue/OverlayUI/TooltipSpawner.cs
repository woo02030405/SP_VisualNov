using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.OverlayUI
{
    public class TooltipSpawner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea] public string message;   // 버튼별 툴팁 메시지
        private RectTransform rect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipController.I != null)
                TooltipController.I.ShowAboveButton(rect, message, 10f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipController.I != null)
                TooltipController.I.Hide();
        }
    }
}

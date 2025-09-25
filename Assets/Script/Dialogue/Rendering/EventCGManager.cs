using UnityEngine;
using UnityEngine.UI;

namespace VN.Rendering
{
    public class EventCGManager : MonoBehaviour
    {
        [SerializeField] private Image eventImage;
        [SerializeField] private CanvasGroup eventGroup;
        [SerializeField] private float holdSeconds = 2.0f;

        public void ShowEventCG(string eventId)
        {
            var sprite = Resources.Load<Sprite>($"CG/{eventId}");
            if (sprite == null)
            {
                Debug.LogWarning($"[EventCGManager] EventCG not found: {eventId}");
                return;
            }

            eventImage.sprite = sprite;
            eventImage.enabled = true;

            if (eventGroup != null)
                eventGroup.alpha = 1f;

            // Ǯ��ũ�� �ƾ� ���� ���� �Է� ������ �ʿ��ϸ� �ڷ�ƾ���� ó��
            if (holdSeconds > 0)
                StartCoroutine(HoldThenHide());
        }

        private System.Collections.IEnumerator HoldThenHide()
        {
            yield return new WaitForSeconds(holdSeconds);
            HideEventCG();
        }

        public void HideEventCG()
        {
            if (eventImage != null)
                eventImage.enabled = false;
            if (eventGroup != null)
                eventGroup.alpha = 0f;
        }
    }
}

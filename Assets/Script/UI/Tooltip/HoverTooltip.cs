using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [TextArea] public string tip;
    public float showDelay = 0.25f; // 초

    private bool inside;
    private float timer;
    private bool shownOnce;

    private void OnDisable()
    {
        inside = false;
        shownOnce = false;
        TooltipUI.Instance?.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inside = true;
        shownOnce = false;
        timer = 0f;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        // 필요시 이동중에도 타이머 조절 가능 (지금은 단순)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inside = false;
        TooltipUI.Instance?.Hide();
    }

    private void Update()
    {
        if (!inside) return;
        if (TooltipUI.Instance == null) return;

        timer += Time.unscaledDeltaTime;
        if (!shownOnce && timer >= showDelay)
        {
            TooltipUI.Instance.Show(tip);
            shownOnce = true;
        }
    }
}

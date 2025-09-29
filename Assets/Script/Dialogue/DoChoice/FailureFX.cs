using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class FailureFX : MonoBehaviour
{
    private RectTransform rt;
    private Vector3 basePos;
    private Vector3 baseScale;
    private Color baseTextColor = Color.white;
    private Color baseImageColor = Color.white;

    private TMP_Text txt;
    private Graphic graphic; // Image or other Graphic

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        basePos = rt.position;
        baseScale = rt.localScale;

        txt = GetComponentInChildren<TMP_Text>();
        if (txt) baseTextColor = txt.color;
        graphic = GetComponent<Graphic>();
        if (graphic) baseImageColor = graphic.color;
    }

    public void PlayFail(Dictionary<string, string> argMap)
    {
        float scaleUp = ChoiceAnimUtil.GetFloat(argMap, "failscale", 1.15f);
        float upTime = ChoiceAnimUtil.GetFloat(argMap, "failuptime", 0.12f);
        float shakeTime = ChoiceAnimUtil.GetFloat(argMap, "failshaketime", 0.25f);
        float strength = ChoiceAnimUtil.GetFloat(argMap, "failshakestrength", 12f);
        float vibrato = ChoiceAnimUtil.GetFloat(argMap, "failvibrato", 20f);
        float restore = ChoiceAnimUtil.GetFloat(argMap, "failrestoretime", 0.15f);
        float autoRec = ChoiceAnimUtil.GetFloat(argMap, "failautorecover", 0f); // 0이면 복구 안함
        string colorStr = ChoiceAnimUtil.GetString(argMap, "failcolor", "#FF4D4D");

        Color failColor = baseTextColor;
        ColorUtility.TryParseHtmlString(colorStr, out failColor);

        // 시퀀스: 확대 → 흔들림 → 원복 → 빨갛게
        var seq = DOTween.Sequence();
        seq.Append(rt.DOScale(scaleUp, upTime).SetEase(Ease.OutQuad));
        seq.Append(rt.DOShakePosition(shakeTime, strength, (int)vibrato, 90f, false, true));
        seq.Append(rt.DOScale(baseScale, restore).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            if (txt) txt.color = failColor;
            if (graphic) graphic.color = failColor;

            if (autoRec > 0f)
            {
                // 일정 시간 후 색 복구
                DOVirtual.DelayedCall(autoRec, () =>
                {
                    if (txt) txt.DOColor(baseTextColor, 0.15f);
                    if (graphic) graphic.DOColor(baseImageColor, 0.15f);
                });
            }
        });
    }
}

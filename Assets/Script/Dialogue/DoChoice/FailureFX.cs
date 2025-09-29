using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class FailureFX : MonoBehaviour
{
    private RectTransform rt;
    private Vector3 baseScale;

    private TMP_Text txt;
    private Image img; // 버튼 배경

    private Color baseTextColor = Color.white;
    private Color baseImageColor = Color.white;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;

        txt = GetComponentInChildren<TMP_Text>();
        if (txt) baseTextColor = txt.color;

        img = GetComponent<Image>();
        if (img) baseImageColor = img.color;
    }

    public void PlayFail(Dictionary<string, string> argMap)
    {
        float scaleUp = ChoiceAnimUtil.GetFloat(argMap, "failscale", 1.15f);
        float upTime = ChoiceAnimUtil.GetFloat(argMap, "failuptime", 0.12f);
        float shakeTime = ChoiceAnimUtil.GetFloat(argMap, "failshaketime", 0.25f);
        float strength = ChoiceAnimUtil.GetFloat(argMap, "failshakestrength", 12f);
        float vibrato = ChoiceAnimUtil.GetFloat(argMap, "failvibrato", 20f);
        float restore = ChoiceAnimUtil.GetFloat(argMap, "failrestoretime", 0.15f);
        float autoRec = ChoiceAnimUtil.GetFloat(argMap, "failautorecover", 0.6f);
        string colorStr = ChoiceAnimUtil.GetString(argMap, "failcolor", "#FF4D4D");

        Color failColor = Color.red;
        ColorUtility.TryParseHtmlString(colorStr, out failColor);

        var seq = DOTween.Sequence();

        // 확대
        seq.Append(rt.DOScale(scaleUp, upTime).SetEase(Ease.OutQuad));

        // 흔들기 직전에 색상 변경 (글씨 흰색, 배경 붉게)
        seq.AppendCallback(() =>
        {
            if (img) img.color = failColor;
            if (txt) txt.color = Color.white;
        });

        // 흔들림
        seq.Append(rt.DOShakePosition(shakeTime, strength, (int)vibrato, 90f, false, true));

        // 크기 원복
        seq.Append(rt.DOScale(baseScale, restore).SetEase(Ease.OutQuad));

        // 복구 처리
        if (autoRec > 0f)
        {
            seq.AppendInterval(autoRec);
            seq.AppendCallback(() =>
            {
                if (img) img.DOColor(baseImageColor, 0.15f);
                if (txt) txt.DOColor(baseTextColor, 0.15f);
            });
        }
    }
}

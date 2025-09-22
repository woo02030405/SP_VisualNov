using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueLabel : MonoBehaviour
{
    [Header("Refs")]
    public Slider slider;
    public TMP_Text label;

    [Header("Format")]
    public string format = "{0:0.0}s"; // 예: 2.0s
    public float multiplier = 1f;      // 값 변환이 필요하면 사용 (여긴 1)

    private void Reset()
    {
        slider = GetComponentInChildren<Slider>();
        label = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderChanged);
            OnSliderChanged(slider.value);
        }
    }

    private void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float v)
    {
        if (label == null) return;
        float val = v * multiplier;
        label.text = string.Format(format, val);
    }
}

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TimedChoiceAnimation : IChoiceAnimation
{
    private float duration = 5f;

    // 선택지에 타이머 슬라이더를 추가하고, 시간이 다 되면 자동으로 선택 처리
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        var slider = selected.GetComponentInChildren<Slider>();
        if (slider != null)
        {
            slider.maxValue = duration;
            slider.value = duration;

            DOTween.To(() => slider.value, x => slider.value = x, 0, duration)
                .OnComplete(() =>
                {
                    GameObject.Destroy(selected);
                    onSelected?.Invoke(next);
                });
        }
        // 선택지 클릭 시 즉시 선택 처리
        selected.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (slider != null) DOTween.Kill(slider);
            GameObject.Destroy(selected);
            onSelected?.Invoke(next);
        });
    }
}

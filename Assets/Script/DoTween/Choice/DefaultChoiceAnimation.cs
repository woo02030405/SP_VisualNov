using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class DefaultChoiceAnimation : IChoiceAnimation
{
    public void Play(GameObject selected, Transform choicePanel, string next, System.Action<string> onSelected)
    {
        // 🔹 선택된 버튼 텍스트 색상 강제 고정
        var txt = selected.GetComponentInChildren<TMPro.TMP_Text>();
        if (txt != null)
        {
            txt.color = Color.black; // 기본 글씨는 검정

            // 🔹 선택된 텍스트에 Bloom용 머티리얼 적용
            var mat = Resources.Load<Material>("TMP_BloomBlack");
            if (mat != null)
                txt.fontSharedMaterial = mat;
        }

        var selectedCg = selected.GetComponent<CanvasGroup>() ?? selected.AddComponent<CanvasGroup>();

        var seq = DG.Tweening.DOTween.Sequence();

        // 선택된 버튼 강조
        seq.Append(selected.transform.DOScale(1.2f, 0.25f).SetEase(DG.Tweening.Ease.OutBack));
        seq.Join(selected.transform.DOMove(choicePanel.position, 0.35f).SetEase(DG.Tweening.Ease.InOutCubic));
        seq.AppendInterval(0.7f);
        seq.Append(selectedCg.DOFade(0f, 0.3f));



        // 나머지 버튼 처리
        foreach (Transform sibling in choicePanel)
        {
            if (sibling.gameObject != selected)
            {
                var cg = sibling.GetComponent<CanvasGroup>() ?? sibling.gameObject.AddComponent<CanvasGroup>();
                cg.interactable = false;

                DOTween.Sequence()
                    .Append(sibling.transform.DOScale(0.8f, 0.25f).SetEase(Ease.InCubic))
                    .Join(sibling.transform.DOMoveY(sibling.position.y - 80f, 0.3f).SetEase(Ease.InCubic))
                    .Join(cg.DOFade(0f, 0.3f))
                    .OnComplete(() => GameObject.Destroy(sibling.gameObject));
            }
        }

        // 🔹 전체 패널 입력 차단 → 중복 클릭 방지
        var panelCg = choicePanel.GetComponent<CanvasGroup>() ?? choicePanel.gameObject.AddComponent<CanvasGroup>();
        panelCg.interactable = false;
        panelCg.blocksRaycasts = false;

        // 선택된 버튼 끝난 뒤 콜백 실행
        seq.OnComplete(() =>
        {
            GameObject.Destroy(selected);
            onSelected?.Invoke(next);
        });
    }
}

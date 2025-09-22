using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject choiceButtonPrefab; // 선택지 버튼 프리팹
    [SerializeField] private Transform choicePanel;         // 선택지 버튼이 붙을 부모 패널

    private List<GameObject> activeButtons = new List<GameObject>();

    /// <summary>
    /// Story에서 만든 선택지 문구 + Dialogue에서 채워넣은 로직을 합쳐 UI로 표시
    /// </summary>
    public void ShowChoices(List<ChoiceOption> options, SaveData saveData, System.Action<ChoiceOption> onSelected)
    {
        ClearChoices();

        foreach (var option in options)
        {
            if (option == null) continue;

            // 조건 체크
            bool condOK = string.IsNullOrEmpty(option.Condition)
                          || option.Condition == "-"
                          || ConditionParser.Evaluate(option.Condition, saveData);

            // 버튼 생성
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicePanel);
            activeButtons.Add(buttonObj);

            Button btn = buttonObj.GetComponent<Button>();
            TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();

            if (btnText != null)
                btnText.text = option.Text ?? "선택지";

            // 조건 불충족 → 버튼 비활성화 (선택 불가)
            btn.interactable = condOK;

            // 클릭 이벤트 등록
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                // 선택 효과 적용은 DialogueManager에서 처리
                onSelected?.Invoke(option);
                ClearChoices();
            });
        }
    }

    /// <summary>
    /// 기존 버튼 전부 제거
    /// </summary>
    public void ClearChoices()
    {
        foreach (var go in activeButtons)
        {
            if (go != null) Destroy(go);
        }
        activeButtons.Clear();
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue UI")]
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;

    [Header("Choice UI")]
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;

    private readonly List<GameObject> spawnedChoices = new List<GameObject>();

    public System.Action onClickNext; // DialogueManager에서 연결

    void Update()
    {
        // Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!HasChoices()) onClickNext?.Invoke();
        }

        // 화면 전체 클릭
        if (Input.GetMouseButtonDown(0))
        {
            if (!HasChoices()) onClickNext?.Invoke();
        }
    }

    public void ShowDialogue(string speakerName, string text)
    {
        speakerNameText.text = speakerName;
        dialogueText.text = text;
        ClearChoices(); // 대사 출력 시 기존 선택지는 제거
    }

    public void ShowChoices(List<(string text, System.Action callback)> choices)
    {
        ClearChoices();

        if (choiceButtonPrefab == null || choiceContainer == null)
        {
            Debug.LogError("[DialogueUI] choiceButtonPrefab or choiceContainer 미연결");
            return;
        }

        foreach (var choice in choices)
        {
            var btnObj = Object.Instantiate(choiceButtonPrefab, choiceContainer);
            var btnText = btnObj.GetComponentInChildren<TMP_Text>();
            var button = btnObj.GetComponent<Button>();

            if (btnText != null) btnText.text = choice.text;
            if (button != null) button.onClick.AddListener(() => choice.callback?.Invoke());

            spawnedChoices.Add(btnObj);
        }
    }

    public bool HasChoices() => spawnedChoices.Count > 0;

    public void ClearChoices()
    {
        for (int i = 0; i < spawnedChoices.Count; i++)
            Destroy(spawnedChoices[i]);
        spawnedChoices.Clear();
    }
}

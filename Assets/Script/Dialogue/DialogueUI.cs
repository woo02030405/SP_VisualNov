using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;

    [Header("Choices")]
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;

    private readonly List<GameObject> spawnedChoices = new List<GameObject>();

    // 외부(Manager)와 연결되는 이벤트들
    public Action onClickNext;
    public Action<string> ShowChoiceHint; // 선택: 실패/안내 메시지
    public Action<GameObject, string, string> onChoiceSelected; // (버튼GO, nodeId, label)

    private void Update()
    {
        // Enter 또는 좌클릭 → 선택지가 없을 때만 다음 진행
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
            && !HasChoices())
        {
            onClickNext?.Invoke();
        }
    }

    public void ShowDialogue(string speakerName, string text)
    {
        if (speakerNameText) speakerNameText.text = speakerName;
        if (dialogueText) dialogueText.text = text;
        ClearChoices();
    }

    // 새 시그니처: style/args/flags까지 함께 전달받음
    public void ShowChoices(List<(string nodeId, string label, string style, string args, string flags)> choices)
    {
        ClearChoices();

        if (!choiceButtonPrefab || !choiceContainer)
        {
            Debug.LogError("[DialogueUI] choiceButtonPrefab/choiceContainer 연결 필요");
            return;
        }

        foreach (var c in choices)
        {
            var go = Instantiate(choiceButtonPrefab, choiceContainer);
            spawnedChoices.Add(go);

            var binder = go.GetComponent<ChoiceButtonBinder>();
            if (!binder) binder = go.AddComponent<ChoiceButtonBinder>();

            binder.Init(this, c.nodeId, c.label, c.style, c.args, c.flags);

            // 실패 연출용 컴포넌트가 프리팹에 없다면 붙여둠(있어도 문제 없음)
            if (!go.GetComponent<FailureFX>()) go.AddComponent<FailureFX>();
        }
    }

    public bool HasChoices() => spawnedChoices.Count > 0;

    public void ClearChoices()
    {
        for (int i = 0; i < spawnedChoices.Count; i++) Destroy(spawnedChoices[i]);
        spawnedChoices.Clear();
    }

    // ChoiceButtonBinder가 클릭 시 호출
    public void RaiseChoiceSelected(GameObject go, string nodeId, string label)
    {
        onChoiceSelected?.Invoke(go, nodeId, label);
    }
}

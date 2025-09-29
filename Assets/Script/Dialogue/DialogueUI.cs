using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Game.OverlayUI;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;

    [Header("Typing")]
    public float charactersPerSecond = 35f;   // 타이핑 속도
    private Coroutine typingCo;
    private string _fullText;
    private bool _isTyping;

    [Header("Choices")]
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;
    private readonly List<GameObject> spawnedChoices = new List<GameObject>();

    // 외부(Manager)와 연결되는 이벤트들
    public Action onClickNext;
    public Action<GameObject, string, string> onChoiceSelected; // (버튼GO, nodeId, label)

    [Header("Hints (Optional)")]
    public RectTransform hintLayer;
    public FloatingHint hintPrefab;

    private void Update()
    {
        // Blocker 활성화 상태면 입력 차단
        if (UIBlocker.IsBlocked) return;

        // 마우스가 UI 위면 클릭을 진행 입력으로 취급하지 않음
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 터치 환경 방어
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return;
        }

        bool clicked = Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter)
                    || Input.GetMouseButtonDown(0);

        // 선택지가 열려 있을 땐 대사 진행 안 함(선택 버튼에서 처리)
        if (clicked && !HasChoices())
            onClickNext?.Invoke();
    }

    // ===== 타이핑 제어 =====
    public bool IsTyping() => _isTyping;

    public void CompleteTyping()
    {
        if (!_isTyping) return;
        _isTyping = false;
        if (typingCo != null) StopCoroutine(typingCo);
        if (dialogueText != null) dialogueText.text = _fullText;
    }

    private IEnumerator TypeRoutine(string text)
    {
        _isTyping = true;
        _fullText = text ?? "";
        if (dialogueText) dialogueText.text = "";

        float secPerChar = charactersPerSecond > 0 ? 1f / charactersPerSecond : 0f;
        for (int i = 0; i < _fullText.Length; i++)
        {
            if (!_isTyping) break;
            dialogueText.text = _fullText.Substring(0, i + 1);
            if (secPerChar > 0) yield return new WaitForSeconds(secPerChar);
            else yield return null;
        }

        _isTyping = false;
        typingCo = null;
    }

    // ===== 표시 =====
    public void ShowDialogue(string speakerName, string text)
    {
        if (speakerNameText) speakerNameText.text = speakerName ?? "";
        if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }
        if (dialogueText) dialogueText.text = "";
        ClearChoices();

        typingCo = StartCoroutine(TypeRoutine(text ?? ""));
    }

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

            // 실패 연출 컴포넌트 보장(없으면 추가)
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

    // ===== (옵션) 간단 힌트 — 프리팹/레이어 없으면 Debug만 =====
    public void ShowFloatingHintAtSpeaker(string speakerId, string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (hintLayer != null && hintPrefab != null)
        {
            var inst = Instantiate(hintPrefab, hintLayer);
            inst.Play(message);
        }
        else
        {
            Debug.Log($"[Hint] {speakerId}: {message}");
        }
    }

    public void ShowStatPopup(string targetSpeakerId, string statKey, int delta)
    {
        string emoji = statKey.StartsWith("affinity") ? "♡"
                    : statKey.StartsWith("gold") ? "ⓖ"
                    : statKey.StartsWith("item") ? "🎁"
                    : statKey.StartsWith("relation") ? "⚡" : "+";
        string msg = $"{emoji}{(delta >= 0 ? "+" : "")}{delta}";
        ShowFloatingHintAtSpeaker(targetSpeakerId, msg);
    }
}

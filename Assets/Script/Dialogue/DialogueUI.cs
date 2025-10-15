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

    [Header("Next Indicator")]
    public NextBlink nextIndicator;   // 🔹 NextBlink 직접 참조

    // 외부 이벤트
    public Action onClickNext;
    public Action<GameObject, string, string> onChoiceSelected;

    [Header("Hints (Optional)")]
    public RectTransform hintLayer;
    public FloatingHint hintPrefab;

    private void Update()
    {
        if (UIBlocker.IsBlocked) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return;
        }

        bool clicked = Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter)
                    || Input.GetMouseButtonDown(0);

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

        // 강제로 끝내면 인디케이터 켜기
        if (nextIndicator) nextIndicator.StartBlink();
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

        if (nextIndicator != null)
        {
            nextIndicator.StartBlink();
        }
    }

    // ===== 표시 =====
    public void ShowDialogue(string speakerName, string text)
    {
        if (speakerNameText) speakerNameText.text = speakerName ?? "";
        if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }
        if (dialogueText) dialogueText.text = "";
        ClearChoices();

        // 새 대사 시작 → 인디케이터 끄기
        if (nextIndicator) nextIndicator.StopBlink();

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

            if (!go.GetComponent<FailureFX>()) go.AddComponent<FailureFX>();
        }
    }

    public bool HasChoices() => spawnedChoices.Count > 0;

    public void ClearChoices()
    {
        for (int i = 0; i < spawnedChoices.Count; i++) Destroy(spawnedChoices[i]);
        spawnedChoices.Clear();
    }

    public void RaiseChoiceSelected(GameObject go, string nodeId, string label)
    {
        onChoiceSelected?.Invoke(go, nodeId, label);
    }

    // ===== 힌트/스탯 =====
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

    // ===== 스킵 컨트롤러용 간단 프로퍼티 =====
    // 선택지 패널 오브젝트 없이, 현재 스폰된 선택지 유무로 판단
    public bool IsChoiceActive => HasChoices();
}

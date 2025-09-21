using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VN.Dialogue;

namespace VN.UI
{
    public class ChoiceUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Transform choicePanel;

        private Dictionary<string, IChoiceAnimation> animRegistry;

        void Awake()
        {
            animRegistry = new Dictionary<string, IChoiceAnimation>()
            {
                { "Default", new DefaultChoiceAnimation() },
                // { "Slide", new SlideChoiceAnimation() }, // 나중에 추가 가능
                // { "Glow", new GlowChoiceAnimation() },   // 나중에 추가 가능
            };
        }

        public void ShowChoices(List<ChoiceOption> options, string animType, Action<string> onSelected)
        {
            foreach (Transform child in choicePanel)
                Destroy(child.gameObject);

            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option.Text)) continue;

                var go = Instantiate(choiceButtonPrefab, choicePanel);
                var txt = go.GetComponentInChildren<TMP_Text>(true);
                if (txt) txt.text = option.Text;

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    string next = option.NextNodeId;
                    btn.onClick.AddListener(() =>
                    {
                        if (!animRegistry.TryGetValue(animType, out var anim))
                            anim = animRegistry["Default"];
                        anim.Play(go, choicePanel, next, onSelected);
                    });
                }
            }
        }
    }
}

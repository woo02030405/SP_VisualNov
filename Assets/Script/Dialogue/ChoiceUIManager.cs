using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public class ChoiceUIManager : MonoBehaviour
    {
        [SerializeField] private Transform choicePanel;         // 선택지가 나타나는 패널
        [SerializeField] private GameObject choiceButtonPrefab; // 버튼 프리팹
        [SerializeField] private bool autoSort = true;          // 선택지를 순서대로 정렬할지 여부

        // 현재 표시된 버튼들을 관리
        private readonly List<GameObject> activeChoices = new();

        // 선택지 애니메이션 등록소
        private readonly Dictionary<string, IChoiceAnimation> animRegistry = new();

        private void Awake()
        {
            // 사용 가능한 애니메이션 미리 등록
            animRegistry["Default"] = new DefaultChoiceAnimation();
            animRegistry["Fade"] = new FadeChoiceAnimation();
            animRegistry["Slide"] = new SlideChoiceAnimation();
            animRegistry["Bounce"] = new BounceChoiceAnimation();
            animRegistry["RunAway"] = new RunAwayChoiceAnimation();
            animRegistry["FollowMouse"] = new FollowMouseChoiceAnimation();
            animRegistry["Timed"] = new TimedChoiceAnimation();
            animRegistry["Bullet"] = new BulletChoiceAnimation();
            animRegistry["WarpStar"] = new WarpStarChoiceAnimation();
        }

        /// <summary>
        /// 선택지 표시
        /// </summary>
        public void ShowChoices(List<ChoiceOption> options, Action<string> onSelected)
        {
            ClearChoices();

            if (options == null || options.Count == 0)
            {
                Debug.LogWarning("[ChoiceUIManager] ShowChoices called with empty list");
                return;
            }

            // 선택지를 NodeId 순으로 정렬 (옵션)
            if (autoSort)
            {
                options.Sort((a, b) => string.Compare(a.NextNodeId, b.NextNodeId, StringComparison.Ordinal));
            }

            Debug.Log($"[ChoiceUIManager] Show {options.Count} choices");

            foreach (var opt in options)
            {
                // 버튼 생성
                var btnObj = Instantiate(choiceButtonPrefab, choicePanel);
                var btn = btnObj.GetComponent<Button>();
                var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();

                if (txt != null)
                    txt.text = opt.Text;

                // Style 처리: 비어있거나 등록 안된 값 → Default
                string style = string.IsNullOrEmpty(opt.ChoiceStyle) ? "Default" : opt.ChoiceStyle;
                if (!animRegistry.TryGetValue(style, out var anim))
                {
                    Debug.LogWarning($"[ChoiceUIManager] Unknown style '{style}', fallback to Default");
                    anim = animRegistry["Default"];
                }

                // 버튼 클릭 시 애니메이션 실행 후 콜백 호출
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[ChoiceUIManager] Choice selected: {opt.Text} → {opt.NextNodeId}");
                    anim.Play(btnObj, choicePanel, opt.NextNodeId, onSelected);
                });

                activeChoices.Add(btnObj);
            }

            // 패널 활성화
            choicePanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// 기존 선택지 제거
        /// </summary>
        public void ClearChoices()
        {
            foreach (var choice in activeChoices)
            {
                if (choice != null) Destroy(choice);
            }
            activeChoices.Clear();

            choicePanel.gameObject.SetActive(false);
        }
    }
}

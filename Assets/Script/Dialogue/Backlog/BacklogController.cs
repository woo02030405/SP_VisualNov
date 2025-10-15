using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Dialogue
{
    public class BacklogController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private GameObject panelRoot;          // BacklogPanel 루트
        [SerializeField] private Transform contentRoot;          // ScrollView/Viewport/Content
        [SerializeField] private GameObject backlogItemPrefab;   // BacklogItem.prefab
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Debug Visuals")]
        [SerializeField] private bool drawDebugBg = true;
        [SerializeField] private Color debugBg = new Color(0.15f, 0.2f, 0.3f, 0.35f);
        [SerializeField] private Color debugSpeaker = Color.white;
        [SerializeField] private Color debugContent = new Color(0.92f, 0.92f, 0.92f, 1f);
        [SerializeField] private int minItemHeight = 56; // 보이는 높이 확보

        private void Awake()
        {
            if (closeButton) closeButton.onClick.AddListener(Close);
            if (panelRoot) panelRoot.SetActive(false);
        }

        public void Open()
        {
            Refresh();
            if (panelRoot) panelRoot.SetActive(true);
        }

        public void Close()
        {
            if (panelRoot) panelRoot.SetActive(false);
        }

        public void Refresh()
        {
            if (!contentRoot || !backlogItemPrefab)
            {
                Debug.LogError("[Backlog/UI] contentRoot 또는 backlogItemPrefab 미연결");
                return;
            }
            if (BacklogManager.Instance == null)
            {
                Debug.LogError("[Backlog/UI] BacklogManager.Instance == null");
                return;
            }

            // 0) 현재 Content 상태 로그
            Debug.Log($"[Backlog/UI] BEFORE: contentRoot.activeSelf={contentRoot.gameObject.activeSelf}, children={contentRoot.childCount}, scale={contentRoot.localScale}");

            // 1) 기존 비우기
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            // 2) 원본 로그
            var logs = BacklogManager.Instance.GetLogs();
            int rawCount = logs?.Count ?? 0;
            Debug.Log($"[Backlog/UI] 원본 로그 수: {rawCount}");

            // 3) 같은 화자 연속 그룹핑
            var grouped = new List<(string spk, string txt, AudioClip voice)>();
            string curSpk = null;
            string merged = null;
            AudioClip lastVoice = null;

            void Flush()
            {
                if (!string.IsNullOrEmpty(curSpk) && !string.IsNullOrEmpty(merged))
                    grouped.Add((curSpk, merged, lastVoice));
            }

            if (rawCount > 0)
            {
                foreach (var e in logs)
                {
                    var s = e.Speaker ?? "";
                    var t = e.Text ?? "";

                    if (curSpk == null || s != curSpk)
                    {
                        Flush();
                        curSpk = s;
                        merged = t;
                        lastVoice = e.VoiceClip;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(merged)) merged += "\n";
                        merged += t;
                        lastVoice = e.VoiceClip;
                    }
                }
                Flush();
            }
            Debug.Log($"[Backlog/UI] 그룹핑 후 아이템 수: {grouped.Count}");

            // 4) 아이템 생성 (강제 가시화 처리 포함)
            for (int i = 0; i < grouped.Count; i++)
            {
                var g = grouped[i];
                var go = Instantiate(backlogItemPrefab, contentRoot);
                go.name = $"BacklogItem_{i}_{g.spk}";
                if (!go.activeSelf) go.SetActive(true);

                // 배경색 칠해서 "존재"를 눈으로 보이게
                if (drawDebugBg)
                {
                    var bg = go.GetComponent<Image>();
                    if (!bg) bg = go.AddComponent<Image>();
                    bg.color = debugBg;
                }

                // 최소 높이 확보(레이아웃 꼬임 방지)
                var rt = go.GetComponent<RectTransform>();
                if (rt)
                {
                    var sd = rt.sizeDelta;
                    if (sd.y < minItemHeight) sd.y = minItemHeight;
                    rt.sizeDelta = sd;
                }

                var ui = go.GetComponent<BacklogItemUI>();
                if (ui)
                {
                    ui.Setup(g.spk, g.txt, g.voice);

                    // TMP 색 강제(알파 0/머티리얼 문제 대비)
                    ForceTextColors(go, debugSpeaker, debugContent);

                    Debug.Log($"[Backlog/UI] GEN[{i}] speaker='{g.spk}', text='{(g.txt?.Replace("\n", "\\n"))}'");
                }
                else
                {
                    // BacklogItemUI가 없다면 경로로 TMP 찾아서 직접 주입
                    var speakerTMP = go.transform.Find("Row/ColSpeaker/SpeakerTMP")?.GetComponent<TextMeshProUGUI>();
                    var contentTMP = go.transform.Find("Row/ColContent/ContentTMP")?.GetComponent<TextMeshProUGUI>();
                    if (speakerTMP) { speakerTMP.text = string.IsNullOrEmpty(g.spk) ? "" : $"{g.spk} :"; speakerTMP.color = debugSpeaker; }
                    if (contentTMP) { contentTMP.text = g.txt ?? ""; contentTMP.color = debugContent; }

                    Debug.LogWarning($"[Backlog/UI] BacklogItemUI 없음 → 경로 바인딩로 텍스트 주입. GEN[{i}]");
                }
            }

            // 5) 레이아웃 강제 갱신 + 스크롤 맨 아래
            Canvas.ForceUpdateCanvases();
            var crt = contentRoot as RectTransform;
            if (crt) LayoutRebuilder.ForceRebuildLayoutImmediate(crt);
            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }

            // AFTER 상태 로그 + 자식 목록
            Debug.Log($"[Backlog/UI] AFTER: children={contentRoot.childCount}");
            for (int i = 0; i < contentRoot.childCount; i++)
            {
                var c = contentRoot.GetChild(i);
                Debug.Log($"[Backlog/UI]  - Child[{i}] name={c.name}, active={c.gameObject.activeSelf}, scale={c.localScale}, pos={c.localPosition}, size={(c as RectTransform)?.rect.size}");
            }
        }

        private void ForceTextColors(GameObject item, Color spk, Color cnt)
        {
            var speakerTMP = item.transform.Find("Row/ColSpeaker/SpeakerTMP")?.GetComponent<TextMeshProUGUI>();
            var contentTMP = item.transform.Find("Row/ColContent/ContentTMP")?.GetComponent<TextMeshProUGUI>();
            if (speakerTMP)
            {
                var col = spk; col.a = 1f;
                speakerTMP.color = col;
                var cg = speakerTMP.GetComponentInParent<CanvasGroup>();
                if (cg) cg.alpha = 1f;
            }
            if (contentTMP)
            {
                var col = cnt; col.a = 1f;
                contentTMP.color = col;
                var cg = contentTMP.GetComponentInParent<CanvasGroup>();
                if (cg) cg.alpha = 1f;
            }
        }
    }
}

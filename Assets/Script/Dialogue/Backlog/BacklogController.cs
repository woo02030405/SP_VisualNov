// Assets/Script/Dialogue/Backlog/BacklogController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;                // (직접 TMP 찾는 백업 경로용)
using Game.Dialogue;       // BacklogManager / BacklogEntry / BacklogItemUI

namespace Game.Dialogue
{
    /// <summary>
    /// BacklogPanel 프리팹(또는 씬 인스턴스)에 붙여서:
    ///  - Open()  : 패널 열기 + 목록 갱신
    ///  - Close() : 패널 닫기
    ///  - Refresh(): BacklogManager에서 로그를 읽어 UI를 재생성
    /// </summary>
    public class BacklogController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private GameObject panelRoot;          // BacklogPanel (루트)
        [SerializeField] private Transform contentRoot;          // Frame/ScrollView/Viewport/Content
        [SerializeField] private GameObject backlogItemPrefab;   // BacklogItem.prefab (UI 프리팹)
        [SerializeField] private Button closeButton;             // Header/CloseButton
        [SerializeField] private ScrollRect scrollRect;          // Frame/ScrollView 의 ScrollRect

        private void Awake()
        {
            if (closeButton) closeButton.onClick.AddListener(Close);
            if (panelRoot) panelRoot.SetActive(false);
        }

        // Backlog 버튼에서 호출
        public void Open()
        {
            Refresh();                    // 먼저 내용 채우고
            if (panelRoot) panelRoot.SetActive(true); // 그다음 보여주기
        }

        // 닫기 버튼에서 호출
        public void Close()
        {
            if (panelRoot) panelRoot.SetActive(false);
        }

        // 목록 재생성
        public void Refresh()
        {
            if (BacklogManager.Instance == null) return;
            if (!contentRoot || !backlogItemPrefab) return;

            // 1) 기존 항목 제거
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            // 2) 로그 가져오기
            IReadOnlyList<BacklogEntry> logs = BacklogManager.Instance.GetLogs();
            if (logs == null || logs.Count == 0)
            {
                ForceLayoutAndScroll();
                return;
            }

            // 3) 같은 화자 연속 그룹핑 (원하는 포맷: "이름 : 대사\n대사\n대사")
            var grouped = new List<(string speaker, string text, AudioClip voice)>();
            string curSpk = null;
            string merged = null;
            AudioClip lastVoice = null;

            void Flush()
            {
                if (!string.IsNullOrEmpty(curSpk) && !string.IsNullOrEmpty(merged))
                    grouped.Add((curSpk, merged, lastVoice));
            }

            foreach (var e in logs)
            {
                string s = e.Speaker ?? "";
                string t = e.Text ?? "";

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
                    lastVoice = e.VoiceClip;  // 그룹의 마지막 보이스 유지(재생 버튼용)
                }
            }
            Flush();

            // 4) 프리팹 생성 + 데이터 주입 (안전한 부모 지정)
            for (int i = 0; i < grouped.Count; i++)
            {
                var g = grouped[i];

                // 인스턴스 생성
                GameObject go = Instantiate(backlogItemPrefab);
                go.name = $"BacklogItem_{i}_{g.speaker}";

                // ★ UI 부모에 붙이기 (worldPositionStays=false 가 핵심)
                var rt = (RectTransform)go.transform;
                rt.SetParent(contentRoot, worldPositionStays: false);
                rt.localScale = Vector3.one;
                rt.anchoredPosition3D = Vector3.zero;

                // 혹시 비활성 저장된 프리팹 대비
                if (!go.activeSelf) go.SetActive(true);

                // BacklogItemUI가 있으면 사용
                var ui = go.GetComponent<BacklogItemUI>();
                if (ui != null)
                {
                    ui.Setup(g.speaker, g.text, g.voice);
                }
                else
                {
                    // 백업 경로: 이름으로 TMP 찾기 (프리팹 구조가 동일하다는 가정)
                    var speakerTMP = go.transform.Find("Row/ColSpeaker/SpeakerTMP")?.GetComponent<TextMeshProUGUI>();
                    var contentTMP = go.transform.Find("Row/ColContent/ContentTMP")?.GetComponent<TextMeshProUGUI>();
                    if (speakerTMP) speakerTMP.text = string.IsNullOrEmpty(g.speaker) ? "" : $"{g.speaker} :";
                    if (contentTMP) contentTMP.text = g.text ?? "";
                }
            }

            // 5) 레이아웃 강제 갱신 + 스크롤 맨 아래로
            ForceLayoutAndScroll();
        }

        private void ForceLayoutAndScroll()
        {
            Canvas.ForceUpdateCanvases();
            if (contentRoot is RectTransform rt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                // 여러 번 호출해서 보장
                scrollRect.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}

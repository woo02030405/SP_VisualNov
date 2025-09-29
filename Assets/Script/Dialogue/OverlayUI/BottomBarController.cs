using UnityEngine;
using UnityEngine.UI;
using VN.SaveSystem;          // SaveManager
using TMPro;

namespace Game.OverlayUI
{
    /// 하단 버튼들 엔트리(세이브/로드/로그/스킵/퀵세이브)
    public class BottomBarController : MonoBehaviour
    {
        [Header("Refs")]
        public DialogueManager dialogueManager;

        // ★씬에 존재하는 Save/Load UI 인스턴스를 드래그해서 연결하세요.
        public SaveLoadUIManager saveLoadUI;

        [Header("Buttons")]
        public Button logBtn, saveBtn, loadBtn, skipBtn, qsaveBtn;

        [Header("Quick Save")]
        [Tooltip("퀵세이브에 사용할 슬롯 인덱스(예: 0)")]
        public int quickSaveSlot = 0;

        [Header("Tooltip Bubble (Optional)")]
        public TooltipBubble bubblePrefab;     // 툴팁 프리팹
        public Transform bubbleLayer;      // Canvas 하위 레이어 (없으면 버튼의 부모 사용)
        public Button qloadBtn;
        public int quickLoadSlot = 0;

        void Awake()
        {
            Wire(logBtn, OpenLog);
            Wire(saveBtn, OpenSaveUI);
            Wire(loadBtn, OpenLoadUI);
            Wire(skipBtn, ToggleSkip);
            Wire(qsaveBtn, DoQuickSave);
            Wire(qloadBtn, DoQuickLoad); 
        }

        void Wire(Button b, UnityEngine.Events.UnityAction action)
        {
            if (!b) return;
            if (!b.GetComponent<ButtonCuteFX>()) b.gameObject.AddComponent<ButtonCuteFX>();
            b.onClick.AddListener(action);
        }

        void OpenLog()
        {
            // 간단 로그 뷰어가 없으면 임시 로그
            Debug.Log("[Log] 대사 로그는 추후 구현 예정");
            Pop(bubbleFrom: logBtn, msg: "로그는 준비중!");
        }

        void OpenSaveUI()
        {
            if (saveLoadUI != null)
            {
                // SaveLoadUIManager에 OpenSave/OpenLoad가 있다고 가정 (일반적으로 있음)
                saveLoadUI.SendMessage("OpenSave", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[BottomBar] saveLoadUI 참조가 비어있습니다. 씬의 SaveLoadUIManager를 연결하세요.");
            }
        }

        void OpenLoadUI()
        {
            if (saveLoadUI != null)
            {
                saveLoadUI.SendMessage("OpenLoad", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[BottomBar] saveLoadUI 참조가 비어있습니다. 씬의 SaveLoadUIManager를 연결하세요.");
            }
        }

        void DoQuickLoad()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Load(quickLoadSlot);
                Pop(qloadBtn, $"퀵로드 완료\n슬롯 {quickLoadSlot}");
            }
            else
            {
                Debug.LogWarning("[BottomBar] SaveManager.Instance가 없습니다.");
            }
        }

        void ToggleSkip()
        {
            // 스킵 정책 정해지면 붙일 자리 (예: 일정 시간 동안 Next 연타)
            Pop(skipBtn, "스킵: 준비중");
        }

        void DoQuickSave()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Save(quickSaveSlot);
                Pop(qsaveBtn, $"퀵세이브 완료\n슬롯 {quickSaveSlot}");
            }
            else
            {
                Debug.LogWarning("[BottomBar] SaveManager.Instance가 없습니다. SaveManager 프리팹/오브젝트를 씬에 배치하세요.");
            }
        }

        // 작은 말풍선 툴팁
        void Pop(Button bubbleFrom, string msg)
        {
            if (!bubblePrefab || !bubbleFrom) return;

            var parent = bubbleLayer ? bubbleLayer : bubbleFrom.transform.parent;
            var tip = Instantiate(bubblePrefab, parent);
            var rt = tip.GetComponent<RectTransform>();
            var btnRt = bubbleFrom.transform as RectTransform;

            // 버튼 위에 살짝 띄워서
            Vector3 world;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parent as RectTransform,
                RectTransformUtility.WorldToScreenPoint(null, btnRt.position) + new Vector2(0, 32f),
                null,
                out world
            );
            rt.position = world;

            tip.Play(msg);
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

namespace VN.UI
{
    public class LogUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private ScrollRect scrollRect;

        private void Start()
        {
            Close();
        }

        private void Update()
        {
            // ESC�� ���
            if (Input.GetKeyDown(KeyCode.Escape))
                Toggle();

            // �α� ���� �ʿ��� ���� ����
            if (LogManager.Instance != null && LogManager.Instance.IsDirty && panelRoot.activeSelf)
                RefreshLog();
        }

        public void Open()
        {
            panelRoot.SetActive(true);
            RefreshLog();
        }

        public void Close()
        {
            panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (panelRoot.activeSelf) Close();
            else Open();
        }

        public void RefreshLog()
        {
            var entries = LogManager.Instance.GetEntries();
            StringBuilder sb = new();

            foreach (var e in entries)
            {
                if (e.speaker == "system")
                    sb.AppendLine($"<color=#AAAAAA>{e.text}</color>");
                else if (e.speaker == "����")
                    sb.AppendLine($"<color=#00CCFF>[����]</color> {e.text}");
                else
                    sb.AppendLine($"<b>{e.speaker}</b>: {e.text}");
            }

            logText.text = sb.ToString();
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f; // �� �Ʒ���

            LogManager.Instance.MarkClean(); // dirty ����
        }
    }
}

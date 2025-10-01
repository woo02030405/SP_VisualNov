using UnityEngine;
using TMPro;

namespace Game.OverlayUI
{
    public class LogEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text lineText;

        public void Setup(string speaker, string line)
        {
            if (speakerText) speakerText.text = speaker;
            if (lineText) lineText.text = line;
        }
    }
}

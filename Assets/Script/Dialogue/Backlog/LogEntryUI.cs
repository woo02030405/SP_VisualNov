using UnityEngine;
using TMPro;

namespace Game.OverlayUI
{
    public class LogEntryUI : MonoBehaviour
    {
        public TMP_Text speakerText;
        public TMP_Text lineText;

        public void Setup(string speaker, string line)
        {
            if (speakerText) speakerText.text = speaker ?? string.Empty;
            if (lineText) lineText.text = line ?? string.Empty;
        }
    }
}

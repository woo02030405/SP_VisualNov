using UnityEngine;

namespace VN.Data
{
    [System.Serializable]
    public class EventRecord
    {
        public string id;               // 고유 ID (예: EV_CH1_01)
        public string title;            // 표시 제목
        [TextArea] public string desc;  // 설명
        public Sprite thumbnail;        // 썸네일
        public bool unlocked;           // 잠금/해제
        public string replayScene;      // 재생용 씬 이름(“씬만” 재생용)
    }
}

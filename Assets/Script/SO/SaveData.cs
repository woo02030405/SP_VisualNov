using System.Collections.Generic;
namespace VN.SaveSystem

{
    [System.Serializable]
    public class SaveData
    {
        public string title;
        public string dateTime;
        public string thumbnailPath;

        // 게임 진행 상태
        public string chapterId;
        public string nodeId;
        public List<string> unlockedCGs;
        public Dictionary<string, int> affinity;
    }
}

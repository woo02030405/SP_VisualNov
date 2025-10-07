using System;
using System.Collections.Generic;

namespace VN.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        // ── UI 슬롯 메타 (SaveSlotUI가 직접 읽음)
        public string title = "";          // 예: "DAY05 - NIGHT"
        public string dateTime = "";       // 예: "2025-10-07 20:45:22"
        public string thumbnailPath = "";  // PNG 경로(없으면 빈 문자열)

        // ── 실제 세이브 본문
        public PlayerData player = new PlayerData();
        public WorldData world = new WorldData();
        public StoryData story = new StoryData();
        public FlagData flags = new FlagData();
        public SystemData system = new SystemData();
    }

    [Serializable]
    public class PlayerData
    {
        public string name = "Hero";
        public Dictionary<string, int> stats = new();
        public Dictionary<string, int> affinity = new();
        public Dictionary<string, int> inventory = new();
        public int gold = 0;
        public int points = 0;
        public int actionPoint = 0;
        public int fatigue = 0;
    }

    [Serializable]
    public class WorldData
    {
        public int day = 1;
        public string weekday = "MON";
        public string timeSlot = "MORNING";     // MORNING/AFTERNOON/NIGHT
        public string currentMap = "MAP_TOWN_DAY";
        public List<string> visitedLocations = new();
        public int cleanness = 0;
        public string weather = "CLEAR";
    }

    [Serializable]
    public class StoryData
    {
        public string chapter = "CH1";
        public string scene = "SC1";
        public string nodeId = "N001";
        public List<string> finishedEvents = new();
        public Dictionary<string, int> cooldowns = new();
    }

    [Serializable]
    public class FlagData
    {
        public Dictionary<string, bool> boolFlags = new();
        public Dictionary<string, int> vars = new();
    }

    [Serializable]
    public class SystemData
    {
        public int saveSlot = 0;
        public string saveName = "";
        public string timestamp = "";   // 저장 시각
        public int playTime = 0;
        public string version = "1.0.0";
    }
}

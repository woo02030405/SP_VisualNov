using System;
using System.Collections.Generic;
using UnityEngine;

namespace VN.SaveSystem
{
    [Serializable]
    public class GameStatePayload
    {
        // ★ 세이브 당시의 Unity 씬 이름 → 로드시 자동 전환에 사용
        public string gameSceneName;

        // 필요하면 아래 필드들 확장해서 같이 저장/로드 가능 (선택)
        public int day;
        public string timeSlot;
        public string currentMapId;

        public int gold;
        public int points;
        public int actionPoint;

        public List<StatEntry> stats = new();
        public List<ItemEntry> inventory = new();
        public List<AffinityEntry> affinity = new();
        public List<FlagEntry> flags = new();
        public List<RelationshipEntry> relationships = new();
    }

    [Serializable] public class StatEntry { public string key; public int value; }
    [Serializable] public class ItemEntry { public string id; public int count; }
    [Serializable] public class AffinityEntry { public string npc; public int value; }
    [Serializable] public class FlagEntry { public string key; public bool value; }
    [Serializable] public class RelationshipEntry { public string idA; public string idB; public int value; }
}

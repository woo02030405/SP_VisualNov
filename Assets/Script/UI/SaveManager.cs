using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VN.SaveSystem
{
    /// <summary>
    /// JSON 기반 세이브/로드 매니저 (슬롯 페이지 UI 지원).
    /// 파일: Application.persistentDataPath/save_slot_{n}.json
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SaveManager]");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Slots")]
        [SerializeField] private int totalSlots = 30;   // 전체 슬롯 수(페이지 계산용)
        public int GetTotalSlotCount() => Mathf.Max(1, totalSlots);

        // 게임 상태 ↔ SaveData 매핑 콜백 (필요 시 외부에서 주입)
        public Func<SaveData> OnBuildSaveData;
        public Action<SaveData> OnApplySaveData;

        // ── File helpers ─────────────────────────────────────────────────────────
        private static string PathForSlot(int slot)
        {
            string dir = Application.persistentDataPath;
            return Path.Combine(dir, $"save_slot_{slot}.json");
        }

        public static bool HasSave(int slot) => File.Exists(PathForSlot(slot));

        // ── UI 페이지 API ─────────────────────────────────────────────────────────
        public List<SaveData> GetSlotsForPage(int page, int perPage)
        {
            var list = new List<SaveData>();
            int start = page * perPage;
            for (int i = 0; i < perPage; i++)
            {
                int idx = start + i;
                if (idx >= totalSlots) { list.Add(null); continue; }

                var path = PathForSlot(idx);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var data = JsonUtility.FromJson<SaveData>(json);
                    list.Add(data);
                }
                else list.Add(null);
            }
            return list;
        }

        // ── UI 호출용 Save/Load ───────────────────────────────────────────────────
        public void Save(int slot)
        {
            var data = OnBuildSaveData != null ? OnBuildSaveData() : new SaveData();

            data.system.saveSlot = slot;
            data.system.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.dateTime = data.system.timestamp;

            if (string.IsNullOrEmpty(data.title))
                data.title = $"DAY{data.world.day:D2} - {data.world.timeSlot}";

            // ★ 백로그/읽음 정보 포함 (RSBM 존재 시)
            try
            {
                ReadSkipBacklogManager.Instance?.ExportToSave(data.story);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] ExportToSave skipped: {ex.Message}");
            }

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PathForSlot(slot), json);
            Debug.Log($"[SaveManager] Saved slot {slot} → {PathForSlot(slot)}");
        }

        public void Load(int slot)
        {
            string path = PathForSlot(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveManager] Save file not found: {path}");
                return;
            }
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);

            // ★ 읽음/백로그 복원 (RSBM 존재 시)
            try
            {
                ReadSkipBacklogManager.Instance?.ImportFromSave(data.story);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] ImportFromSave skipped: {ex.Message}");
            }

            OnApplySaveData?.Invoke(data);
            Debug.Log($"[SaveManager] Loaded slot {slot}");
        }

        // ── 정적 호환 (원한다면 테스트 코드에서 사용) ─────────────────────────────
        public static void Save(SaveData data, int slot)
        {
            if (data == null) { Debug.LogError("[SaveManager] SaveData is null"); return; }
            data.system.saveSlot = slot;
            data.system.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.dateTime = data.system.timestamp;

            // ★ 백로그/읽음 Export 추가
            try
            {
                ReadSkipBacklogManager.Instance?.ExportToSave(data.story);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Static ExportToSave skipped: {ex.Message}");
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PathForSlot(slot), json);
            Debug.Log($"[SaveManager] Saved slot {slot} → {PathForSlot(slot)}");
        }

        public static SaveData LoadStatic(int slot)
        {
            string path = PathForSlot(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveManager] Save file not found: {path}");
                return null;
            }
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);

            // ★ Import 추가
            try
            {
                ReadSkipBacklogManager.Instance?.ImportFromSave(data.story);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Static ImportFromSave skipped: {ex.Message}");
            }

            Debug.Log($"[SaveManager] Loaded slot {slot}");
            return data;
        }
    }
}

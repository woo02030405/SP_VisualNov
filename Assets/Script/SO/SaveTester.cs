using UnityEngine;
using VN.SaveSystem;   // ★ 추가

public class SaveTester : MonoBehaviour
{
    void Start()
    {
        // A) 싱글톤 경유 (UI와 동일 흐름)
        SaveManager.Instance.OnBuildSaveData = () =>
        {
            var d = new SaveData();
            d.world.day = 5;
            d.world.timeSlot = "NIGHT";
            d.player.gold = 777;
            d.title = "테스트 세이브";
            return d;
        };
        SaveManager.Instance.Save(0);

        SaveManager.Instance.OnApplySaveData = (d) =>
        {
            Debug.Log($"로드확인: {d.title} / GOLD={d.player.gold} / DAY={d.world.day}");
        };
        SaveManager.Instance.Load(0);

        // B) 정적 호환 사용도 가능
        // var raw = new SaveData();
        // SaveManager.Save(raw, 1);
        // var loaded = SaveManager.LoadStatic(1);
    }
}

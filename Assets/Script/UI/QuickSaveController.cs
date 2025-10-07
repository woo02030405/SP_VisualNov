using UnityEngine;
using VN.SaveSystem;

public class QuickSaveController : MonoBehaviour
{
    [Header("Quick slot index (e.g., 0 or 99)")]
    public int quickSlotIndex = 0;

    // Inspector에서 GameManager로부터 콜백 주입하면 더 좋지만,
    // 최소 구성: SaveManager.Instance의 OnBuild/OnApply 콜백이 이미 세팅되어 있다고 가정.
    public void OnClickQuickSave()
    {
        SaveManager.Instance.Save(quickSlotIndex);
        Debug.Log($"[QuickSave] Saved to slot {quickSlotIndex}");
    }

    public void OnClickQuickLoad()
    {
        if (!System.IO.File.Exists(GetPath(quickSlotIndex)))
        {
            Debug.LogWarning($"[QuickLoad] No file at slot {quickSlotIndex}");
            return;
        }
        SaveManager.Instance.Load(quickSlotIndex);
        Debug.Log($"[QuickLoad] Loaded from slot {quickSlotIndex}");
    }

    private string GetPath(int slot)
    {
        // SaveManager 내부 경로를 그대로 쓰진 못하므로 동일 규칙으로 생성
        return System.IO.Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }
}

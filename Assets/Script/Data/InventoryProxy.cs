using System.Collections.Generic;
using UnityEngine;
using VN.SaveSystem;

public class InventoryProxy : MonoBehaviour
{
    [Header("Item ids & counts")]
    public List<string> itemIds = new();
    public List<int> counts = new();

    public void PullFromGame()
    {
        // TODO: Inventory에서 읽어오기
    }

    public void PushToGame()
    {
        // TODO: Inventory에 쓰기
    }

    public void ExportTo(List<ItemEntry> outList)
    {
        outList.Clear();
        for (int i = 0; i < itemIds.Count; i++)
        {
            if (!string.IsNullOrEmpty(itemIds[i]))
                outList.Add(new ItemEntry { id = itemIds[i], count = i < counts.Count ? counts[i] : 1 });
        }
    }

    public void ImportFrom(List<ItemEntry> inList)
    {
        itemIds.Clear(); counts.Clear();
        if (inList == null) return;
        foreach (var it in inList) { itemIds.Add(it.id); counts.Add(it.count); }
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private Image leftSlot;
    [SerializeField] private Image centerSlot;
    [SerializeField] private Image rightSlot;

    private string basePath = "Characters/"; // Resources/Characters/

    public void ShowCharacter(string speakerId, string type, string expression, string pose, string position, string effect)
    {
        string spriteName = $"{speakerId}_{type}_{expression}_{pose}";
        Sprite s = Resources.Load<Sprite>(basePath + spriteName);
        if (s == null)
        {
            Debug.LogWarning($"[CharacterManager] Sprite not found: {spriteName}");
            return;
        }

        Image target = GetSlot(position);
        if (target == null) return;

        target.sprite = s;
        target.enabled = true;

        // 🔹 TODO: effect (fadein/out, shake 등 DOTween 적용 가능)
        if (effect == "fadein")
        {
            var cg = target.GetComponent<CanvasGroup>() ?? target.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.DOFade(1f, 0.5f);
        }
    }

    private Image GetSlot(string position)
    {
        switch (position?.ToLower())
        {
            case "left": return leftSlot;
            case "right": return rightSlot;
            default: return centerSlot;
        }
    }

    public void ClearAll()
    {
        leftSlot.enabled = false;
        centerSlot.enabled = false;
        rightSlot.enabled = false;
    }
}

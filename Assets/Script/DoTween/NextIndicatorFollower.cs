using TMPro;
using UnityEngine;

public class NextIndicatorFollower : MonoBehaviour
{
    public TMP_Text dialogueText;
    public RectTransform indicator;

    [Header("Position Offset")]
    public float offsetX = 20f;
    public float offsetY = 0f;

    void LateUpdate()
    {
        if (!dialogueText || !indicator) return;

        dialogueText.ForceMeshUpdate();
        var info = dialogueText.textInfo;
        if (info.characterCount == 0) return;

        // 마지막 글자
        var lastChar = info.characterInfo[info.characterCount - 1];
        if (!lastChar.isVisible) return;

        // 중간 높이
        float midY = (lastChar.ascender + lastChar.descender) / 2f;

        Vector3 localPos = new Vector3(lastChar.topRight.x, midY, 0);
        Vector3 worldPos = dialogueText.transform.TransformPoint(localPos);

        indicator.position = worldPos + new Vector3(offsetX, offsetY, 0);
    }
}

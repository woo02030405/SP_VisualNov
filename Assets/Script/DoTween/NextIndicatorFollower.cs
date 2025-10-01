using TMPro;
using UnityEngine;

public class NextIndicatorFollower : MonoBehaviour
{
    public TMP_Text dialogueText;
    public RectTransform indicator;

    [Header("Position Offset")]
    public float offsetX = 20f;
    public float offsetY = 0f;

    [Header("Size Control")]
    public Vector2 indicatorSize = new Vector2(40f, 40f); // 기본 크기

    void LateUpdate()
    {
        if (!dialogueText || !indicator) return;

        dialogueText.ForceMeshUpdate();
        var info = dialogueText.textInfo;
        if (info.characterCount == 0) return;

        var lastChar = info.characterInfo[info.characterCount - 1];
        if (!lastChar.isVisible) return;

        Vector3 worldPos = (lastChar.topRight + lastChar.bottomRight) / 2;
        indicator.position = dialogueText.transform.TransformPoint(worldPos + new Vector3(offsetX, offsetY, 0));

        // Inspector에서 설정한 크기로 적용
        indicator.sizeDelta = indicatorSize;
    }
}

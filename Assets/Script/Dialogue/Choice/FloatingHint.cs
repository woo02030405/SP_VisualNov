using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingHint : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] CanvasGroup cg;
    [SerializeField] float duration = 1.5f;
    [SerializeField] float moveUp = 50f;

    public void Play(string message)
    {
        if (text) text.text = message;
        if (!cg) cg = GetComponent<CanvasGroup>();

        cg.alpha = 0f;
        transform.localPosition = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, 0.2f));
        seq.Join(transform.DOLocalMoveY(moveUp, duration));
        seq.Append(cg.DOFade(0f, 0.5f))
           .OnComplete(() => Destroy(gameObject));
    }
}

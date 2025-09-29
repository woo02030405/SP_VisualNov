using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceButtonBinder : MonoBehaviour
{
    public string nodeId;
    public string label;
    public string style; // CSV: ChoiceStyle
    public string args;  // CSV: ChoiceArgs
    public string flags; // CSV: ChoiceFlags

    private Button _btn;
    private TMP_Text _txt;
    private DialogueUI _ui;

    public void Init(DialogueUI ui, string nodeId, string label, string style = null, string args = null, string flags = null)
    {
        this._ui = ui;
        this.nodeId = nodeId;
        this.label = label;
        this.style = style;
        this.args = args;
        this.flags = flags;

        _btn = GetComponent<Button>();
        _txt = GetComponentInChildren<TMP_Text>();

        if (_txt) _txt.text = label;

        if (_btn)
        {
            _btn.onClick.RemoveAllListeners();
            _btn.onClick.AddListener(() =>
            {
                _ui?.RaiseChoiceSelected(gameObject, nodeId, label);
            });
        }
    }

    public void SetInteractable(bool v)
    {
        if (_btn) _btn.interactable = v;
    }
}

// GameBootstrap.cs
using UnityEngine;
using VN.IO;
using VN.Dialogue;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] DialogueManager dialogueManager;

    void Start()
    {
        LocalizationManager.LoadLanguage("ko");
        var map = DialogueParser.LoadFromStreamingAssets("ko");
        dialogueManager.Load(map, "N001");
    }
}

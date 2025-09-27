using System.IO;
using UnityEngine;
using VN;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] DialogueManager dialogueManager;

    [SerializeField] string chapterId = "Ch1_Main"; // 시작 챕터

    void Start()
    {
        // 스피커 데이터 로드 (공용)
        LocalizationManager.LoadLanguage("kr");

        // 챕터별 CSV 로드
        string basePath = Path.Combine(Application.streamingAssetsPath, "Main");
        string storyPath = Path.Combine(basePath, $"{chapterId}_Story_kr.csv");
        string dialoguePath = Path.Combine(basePath, $"{chapterId}_Dialogue.csv");

        // DialogueManager 초기화
        dialogueManager.Init(storyPath, dialoguePath);
    }
}

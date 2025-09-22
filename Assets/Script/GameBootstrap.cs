using UnityEngine;
using VN.Dialogue;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string startChapter = "CH1";
    [SerializeField] private string startNode = "N001";

    private DialogueManager dialogueManager;

    void Awake()
    {
        // 씬에 DialogueManager가 있는지 확인
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("[GameBootstrap] DialogueManager not found in scene.");
            return;
        }

        var speakers = LocalizationManager.LoadSpeakers(); // 스피커 정보 로드
        var storyData = LocalizationManager.LoadStory(startChapter); // 스토리 데이터 로드
        var dialogueData = LocalizationManager.LoadDialogue(startChapter); // 대사 데이터 로드

        if (speakers == null || storyData == null || dialogueData == null) // 데이터 로드 실패 처리
        {
            Debug.LogError("[GameBootstrap] Failed to load CSV data.");
            return;
        }

        dialogueManager.Initialize(speakers, storyData, dialogueData); // DialogueManager 초기화
        dialogueManager.JumpTo(startNode); // 시작 노드로 이동

        Debug.Log($"[GameBootstrap] Bootstrapped {startChapter} starting at {startNode}"); // 부트스트랩 완료 로그
    }
}

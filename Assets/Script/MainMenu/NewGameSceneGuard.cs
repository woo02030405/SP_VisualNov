// Assets/Script/Bootstrap/NewGameSceneGuard.cs
using UnityEngine;

public class NewGameSceneGuard : MonoBehaviour
{
    [Tooltip("씬 첫 프레임에 백로그만 초기화 (읽음 이력은 유지)")]
    public bool clearBacklogOnAwake = true;

    [Tooltip("씬 첫 프레임에 GameState도 한번 더 초기화 (아이템/호감도/변수)")]
    public bool resetGameStateOnAwake = false;

    void Awake()
    {
        // 읽음은 유지, 백로그만 비움
        if (clearBacklogOnAwake && ReadSkipBacklogManager.Instance != null)
            ReadSkipBacklogManager.Instance.ClearBacklog();

        if (resetGameStateOnAwake && GameState.I != null)
            GameState.I.ResetAll();
    }
}

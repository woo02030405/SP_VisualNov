using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadPortal
{
    public enum Mode { Save, Load }

    public static Mode RequestMode { get; private set; }
    public static string CallerScene { get; private set; }

    public static void Open(Mode mode)
    {
        RequestMode = mode;
        CallerScene = SceneManager.GetActiveScene().name;
        // SaveLoadScene을 애드티브로 로드
        SceneManager.LoadScene("SaveLoadScene", LoadSceneMode.Additive);
    }

    public static void CloseAndReturn()
    {
        // SaveLoadScene 언로드
        SceneManager.UnloadSceneAsync("SaveLoadScene");
        // 원하면 여기서 CallerScene으로 돌아가는 로직도 넣을 수 있지만,
        // 애드티브 오버레이면 굳이 씬 전환은 필요 없음.
    }
}

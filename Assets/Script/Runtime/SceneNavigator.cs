using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneNavigator
{
    public static string LastSceneName { get; private set; }

    public static void Load(string sceneName)
    {
        LastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }

    public static void Load(string sceneName, string returnSceneName)
    {
        LastSceneName = returnSceneName;
        SceneManager.LoadScene(sceneName);
    }

    public static void Back()
    {
        if (!string.IsNullOrEmpty(LastSceneName))
            SceneManager.LoadScene(LastSceneName);
        else
            Debug.LogWarning("[SceneNavigator] No last scene to go back to!");
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

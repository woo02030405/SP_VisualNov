using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public static string LastSceneName { get; private set; }

    /// <summary>
    /// 일반적인 씬 로드 (자동으로 현재 씬을 LastSceneName에 기록)
    /// </summary>
    public void Load(string sceneName)
    {
        LastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 특정 씬으로 돌아가도록 강제 지정
    /// </summary>
    public void Load(string sceneName, string returnSceneName)
    {
        LastSceneName = returnSceneName;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 이전 씬(또는 지정된 씬)으로 복귀
    /// </summary>
    public void Back()
    {
        if (!string.IsNullOrEmpty(LastSceneName))
        {
            SceneManager.LoadScene(LastSceneName, LoadSceneMode.Single);
        }
        else
        {
            // 기본값: 메인 메뉴
            SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

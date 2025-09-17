using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    // 버튼 OnClick에서 이 메서드를 호출하고, 인자에 씬 이름을 적어준다.
    public void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    // 뒤로 가기(허브/메인으로)
    public void BackTo(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

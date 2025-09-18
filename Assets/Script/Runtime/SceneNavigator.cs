using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public static string LastSceneName { get; private set; }

    /// <summary>
    /// �Ϲ����� �� �ε� (�ڵ����� ���� ���� LastSceneName�� ���)
    /// </summary>
    public void Load(string sceneName)
    {
        LastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Ư�� ������ ���ư����� ���� ����
    /// </summary>
    public void Load(string sceneName, string returnSceneName)
    {
        LastSceneName = returnSceneName;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// ���� ��(�Ǵ� ������ ��)���� ����
    /// </summary>
    public void Back()
    {
        if (!string.IsNullOrEmpty(LastSceneName))
        {
            SceneManager.LoadScene(LastSceneName, LoadSceneMode.Single);
        }
        else
        {
            // �⺻��: ���� �޴�
            SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

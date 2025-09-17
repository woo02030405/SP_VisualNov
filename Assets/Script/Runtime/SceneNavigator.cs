using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    // ��ư OnClick���� �� �޼��带 ȣ���ϰ�, ���ڿ� �� �̸��� �����ش�.
    public void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    // �ڷ� ����(���/��������)
    public void BackTo(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

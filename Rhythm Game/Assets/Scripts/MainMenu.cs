using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenControllerConfig()
    {
        SceneManager.LoadScene("ControllerConfiguration");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
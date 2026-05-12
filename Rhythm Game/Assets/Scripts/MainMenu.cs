using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button[] menuButtons;

    [Header("Input Settings")]
    public float stickDeadzone = 0.5f;

    private int currentButtonIndex = 0;
    private int previousVertDir = 0;

    void Start()
    {
        if (menuButtons.Length > 0)
        {
            SelectButton(0);
        }
    }

    void Update()
    {
        CheckNavigationInput();
        CheckSelectInput();
    }

    void CheckNavigationInput()
    {
        if (InputBindingManager.Instance == null) return;

        int vertDir = InputBindingManager.Instance.GetVerticalDir(stickDeadzone);

        if (vertDir != previousVertDir && vertDir != 0)
        {
            if (vertDir == 1)
            {
                NavigateUp();
            }
            else if (vertDir == -1)
            {
                NavigateDown();
            }
        }

        previousVertDir = vertDir;
    }

    void CheckSelectInput()
    {
        if (InputBindingManager.Instance == null) return;

        bool selectPressed = InputBindingManager.Instance.GetBindingDown(
            InputBindingManager.Instance.Bindings.button1);

        if (selectPressed && menuButtons.Length > 0)
        {
            menuButtons[currentButtonIndex].onClick.Invoke();
        }
    }

    void NavigateUp()
    {
        if (menuButtons.Length == 0) return;

        currentButtonIndex--;
        if (currentButtonIndex < 0)
            currentButtonIndex = menuButtons.Length - 1;

        SelectButton(currentButtonIndex);
    }

    void NavigateDown()
    {
        if (menuButtons.Length == 0) return;

        currentButtonIndex++;
        if (currentButtonIndex >= menuButtons.Length)
            currentButtonIndex = 0;

        SelectButton(currentButtonIndex);
    }

    void SelectButton(int index)
    {
        currentButtonIndex = index;
        menuButtons[index].Select();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

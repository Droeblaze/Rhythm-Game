using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button[] menuButtons; // Assign in order: Play, Options, Quit (or whatever order you prefer)

    [Header("Input Settings")]
    public float stickDeadzone = 0.5f;
    public int selectButton = 3; 
    public bool selectIsButton = true;
    public string selectAxis = "";
    public float triggerThreshold = 0.5f;

    private int currentButtonIndex = 0;
    private int previousVertDir = 0;
    private bool previousSelectState = false;

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
        float vertical = Input.GetAxisRaw("Vertical");
        int vertDir = vertical < -stickDeadzone ? -1 : (vertical > stickDeadzone ? 1 : 0);

        // Detect vertical direction change
        if (vertDir != previousVertDir && vertDir != 0)
        {
            if (vertDir == 1) // Up
            {
                NavigateUp();
            }
            else if (vertDir == -1) // Down
            {
                NavigateDown();
            }
        }

        previousVertDir = vertDir;
    }

    void CheckSelectInput()
    {
        bool selectPressed = GetSelectButtonDown();

        if (selectPressed && menuButtons.Length > 0)
        {
            menuButtons[currentButtonIndex].onClick.Invoke();
        }
    }

    bool GetSelectButtonDown()
    {
        if (selectIsButton)
        {
            return Input.GetKeyDown(KeyCode.JoystickButton0 + selectButton);
        }
        else
        {
            if (string.IsNullOrEmpty(selectAxis)) return false;

            float axisValue = Input.GetAxis(selectAxis);
            bool currentlyPressed = axisValue > triggerThreshold;
            bool wasPressed = previousSelectState;

            previousSelectState = currentlyPressed;

            return currentlyPressed && !wasPressed;
        }
    }

    void NavigateUp()
    {
        if (menuButtons.Length == 0) return;

        currentButtonIndex--;
        if (currentButtonIndex < 0)
            currentButtonIndex = menuButtons.Length - 1; // Wrap to bottom

        SelectButton(currentButtonIndex);
    }

    void NavigateDown()
    {
        if (menuButtons.Length == 0) return;

        currentButtonIndex++;
        if (currentButtonIndex >= menuButtons.Length)
            currentButtonIndex = 0; // Wrap to top

        SelectButton(currentButtonIndex);
    }

    void SelectButton(int index)
    {
        currentButtonIndex = index;
        menuButtons[index].Select();
    }

    public void PlayGame()
    {
        //Load next scene in the list
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

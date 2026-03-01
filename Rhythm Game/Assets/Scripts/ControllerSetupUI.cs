using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ControllerSetupUI : MonoBehaviour
{
    public enum SetupMode
    {
        First,
        Menu
    }

    [Header("DEBUG MODE (Editor Only)")]
    public bool overrideModeInEditor = true;
    public SetupMode debugMode = SetupMode.First;

    [Header("Top Selection Buttons")]
    public Button keyboardButton;
    public Button gamepadButton;
    public Button fightStickButton;

    [Header("Bottom Buttons")]
    public GameObject continueButtonObj;

    [Header("Mode Containers")]
    public GameObject firstRunExtras;
    public GameObject menuExtras;

    [Header("Optional UI")]
    public TMP_Text selectedText;

    [Header("Device Detection UI")]
    public TMP_Text detectedDeviceText;

    private const string ProfileKey = "control_profile";
    private const string ModeKey = "controller_setup_mode";
    private const string ReturnSceneKey = "controller_setup_return_scene";

    private bool hasSelection = false;

    private readonly Color normalColor = Color.white;
    private readonly Color selectedColor = new Color(0.70f, 0.80f, 1f, 1f);

    void Start()
    {
        // 🔹 Debug override (only affects Editor testing)
        if (overrideModeInEditor)
        {
            string forcedMode = debugMode == SetupMode.First ? "first" : "menu";
            PlayerPrefs.SetString(ModeKey, forcedMode);

            if (debugMode == SetupMode.First)
                PlayerPrefs.DeleteKey(ProfileKey); // optional reset for testing
        }

        string mode = PlayerPrefs.GetString(ModeKey, "first");

        ApplyMode(mode);
        UpdateDetectedDeviceText();

        if (mode == "menu" && PlayerPrefs.HasKey(ProfileKey))
        {
            string saved = PlayerPrefs.GetString(ProfileKey, "KeyboardMouse");
            hasSelection = true;
            ApplySelection(saved);
        }
        else
        {
            hasSelection = false;
            if (selectedText != null)
                selectedText.text = "";
        }

        if (continueButtonObj != null && mode == "first")
            continueButtonObj.SetActive(hasSelection);
    }

    private void ApplyMode(string mode)
    {
        bool isMenu = (mode == "menu");

        if (firstRunExtras != null)
            firstRunExtras.SetActive(!isMenu);

        if (menuExtras != null)
            menuExtras.SetActive(isMenu);

        if (continueButtonObj != null)
            continueButtonObj.SetActive(isMenu);
    }

    private void UpdateDetectedDeviceText()
    {
        if (detectedDeviceText == null) return;

        bool hasGamepad = Gamepad.current != null;
        bool hasKeyboard = Keyboard.current != null;
        bool hasMouse = Mouse.current != null;

        if (hasGamepad)
        {
            string deviceName = Gamepad.current.displayName;
            detectedDeviceText.text =
                $"Detected: Controller connected ({deviceName})";
        }
        else if (hasKeyboard || hasMouse)
        {
            detectedDeviceText.text = "Detected: Keyboard & Mouse";
        }
        else
        {
            detectedDeviceText.text = "Detected: No input device detected";
        }
    }

    public void ChooseKeyboardMouse() => SetProfile("KeyboardMouse");
    public void ChooseGamepad() => SetProfile("Gamepad");
    public void ChooseFightStick() => SetProfile("FightStick");

    private void SetProfile(string profile)
    {
        PlayerPrefs.SetString(ProfileKey, profile);
        PlayerPrefs.Save();

        hasSelection = true;
        ApplySelection(profile);

        string mode = PlayerPrefs.GetString(ModeKey, "first");
        if (continueButtonObj != null && mode == "first")
            continueButtonObj.SetActive(true);
    }

    private void ApplySelection(string profile)
    {
        Highlight(keyboardButton, profile == "KeyboardMouse");
        Highlight(gamepadButton, profile == "Gamepad");
        Highlight(fightStickButton, profile == "FightStick");

        if (selectedText != null)
            selectedText.text = "Selected: " + profile;

        Debug.Log("Selected profile: " + profile);
    }

    private void Highlight(Button btn, bool selected)
    {
        if (btn == null) return;

        var colors = btn.colors;
        colors.normalColor = selected ? selectedColor : normalColor;
        colors.selectedColor = selected ? selectedColor : normalColor;
        btn.colors = colors;
    }

    public void Continue()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Back()
    {
        string returnScene =
            PlayerPrefs.GetString(ReturnSceneKey, "MainMenu");
        SceneManager.LoadScene(returnScene);
    }
}
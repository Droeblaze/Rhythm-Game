using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_INPUT_SYSTEM_AVAILABLE
using UnityEngine.InputSystem;
#endif

public class FirstRunSetupUI : MonoBehaviour
{
    [Header("Scene")]
    public string mainMenuScene = "MainMenu";

    [Header("UI Text")]
    public TextMeshProUGUI detectedDeviceText;
    public TextMeshProUGUI recommendationText;

    [Header("Profile Buttons")]
    public Button fightStickButton;
    public Button gamepadButton;
    public Button keyboardButton;

    [Header("Navigation")]
    public Button continueButton;

    [Header("Panels")]
    [Tooltip("The main background/selection panel — gets hidden when customization opens. Drag BackgroundPanel here.")]
    public GameObject profileSelectionPanel;
    [Tooltip("Drag the CustomizationPanel GameObject here.")]
    public BindingCustomizationPanel customizationPanel;

    [Header("Colors")]
    public Color selectedColor   = new Color(1f, 0.6f, 0f);
    public Color unselectedColor = new Color(0.2f, 0.2f, 0.2f);

    private ControlProfile? selectedProfile = null;

    void Start()
    {
        // Wire profile buttons — each one selects AND opens customization
        fightStickButton.onClick.AddListener(() => SelectAndOpen(ControlProfile.FightStick));
        gamepadButton.onClick.AddListener(   () => SelectAndOpen(ControlProfile.Gamepad));
        keyboardButton.onClick.AddListener(  () => SelectAndOpen(ControlProfile.Keyboard));
        continueButton.onClick.AddListener(OnContinue);

        // Make sure customization panel is hidden on start (belt + suspenders alongside Awake)
        if (customizationPanel != null)
            customizationPanel.gameObject.SetActive(false);

        DetectAndRecommend();

        // Pre-select previously saved profile if one exists
        string saved = PlayerPrefs.GetString(ControlProfileApplicator.KEY_PROFILE, "");
        if (!string.IsNullOrEmpty(saved) && System.Enum.TryParse(saved, out ControlProfile existing))
            SelectProfile(existing); // highlight only, don't open panel on load
    }

    // Device detection

    void DetectAndRecommend()
    {
        ControlProfile recommended = ControlProfile.FightStick;
        string deviceName = "No device detected";

#if UNITY_INPUT_SYSTEM_AVAILABLE
        var gamepads = Gamepad.all;
        if (gamepads.Count > 0)
        {
            deviceName  = gamepads[0].displayName ?? gamepads[0].name;
            recommended = ControlProfile.FightStick;
        }
        else if (Keyboard.current != null)
        {
            deviceName  = "Keyboard";
            recommended = ControlProfile.Keyboard;
        }
#else
        string[] joysticks = Input.GetJoystickNames();
        if (joysticks.Length > 0 && !string.IsNullOrEmpty(joysticks[0]))
        {
            deviceName  = joysticks[0];
            recommended = ControlProfile.FightStick;
        }
        else
        {
            deviceName  = "Keyboard / No Controller";
            recommended = ControlProfile.Keyboard;
        }
#endif

        if (detectedDeviceText != null)
            detectedDeviceText.text = $"Detected: {deviceName}";

        if (recommendationText != null)
            recommendationText.text = $"Recommended: {ProfileDisplayName(recommended)}\n" +
                                       "<size=70%>(Tap a profile to customize its bindings)</size>";

        HighlightRecommended(recommended);
    }

    void HighlightRecommended(ControlProfile recommended)
    {
        SetButtonColor(fightStickButton, unselectedColor);
        SetButtonColor(gamepadButton,    unselectedColor);
        SetButtonColor(keyboardButton,   unselectedColor);

        Color recommendedColor = Color.Lerp(unselectedColor, selectedColor, 0.35f);
        switch (recommended)
        {
            case ControlProfile.FightStick: SetButtonColor(fightStickButton, recommendedColor); break;
            case ControlProfile.Gamepad:    SetButtonColor(gamepadButton,    recommendedColor); break;
            case ControlProfile.Keyboard:   SetButtonColor(keyboardButton,   recommendedColor); break;
        }
    }

    // Profile selection (highlight only, no panel open)

    void SelectProfile(ControlProfile profile)
    {
        selectedProfile = profile;
        SetButtonColor(fightStickButton, profile == ControlProfile.FightStick ? selectedColor : unselectedColor);
        SetButtonColor(gamepadButton,    profile == ControlProfile.Gamepad    ? selectedColor : unselectedColor);
        SetButtonColor(keyboardButton,   profile == ControlProfile.Keyboard   ? selectedColor : unselectedColor);
    }

    // Select + open customization

    void SelectAndOpen(ControlProfile profile)
    {
        SelectProfile(profile);
        SaveProfile(profile); // save immediately so customization knows what profile we're on

        if (customizationPanel == null) return;

        if (profileSelectionPanel != null)
            profileSelectionPanel.SetActive(false);

        customizationPanel.gameObject.SetActive(true);
        customizationPanel.Open(profile, this);
    }

    // Called by BindingCustomizationPanel Back button

    public void ShowProfileSelection()
    {
        if (profileSelectionPanel != null)
            profileSelectionPanel.SetActive(true);

        if (customizationPanel != null)
            customizationPanel.gameObject.SetActive(false);
    }

    // Continue

    void OnContinue()
    {
        if (selectedProfile != null)
            SaveProfile(selectedProfile.Value);

        SceneManager.LoadScene(mainMenuScene);
    }
    // Default
    public void ResetDefaults()
{
    string saved = PlayerPrefs.GetString(ControlProfileApplicator.KEY_PROFILE, "FightStick");

    if (System.Enum.TryParse(saved, out ControlProfile profile))
    {
        CustomBindingsStore.ClearOverrides(profile);
        Debug.Log($"[ControllerConfig] Reset overrides for: {profile}");
    }
    else
    {
        Debug.LogWarning("[ControllerConfig] Failed to parse profile during reset.");
    }
}

    public static void SaveProfile(ControlProfile profile)
    {
        PlayerPrefs.SetString(ControlProfileApplicator.KEY_PROFILE, profile.ToString());
        PlayerPrefs.SetInt(ControlProfileApplicator.KEY_SETUP_COMPLETE, 1);
        PlayerPrefs.Save();
        Debug.Log($"[ControllerConfig] Saved profile: {profile}");
    }

    // Helpers

    void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        var colors = btn.colors;
        colors.normalColor = color;
        btn.colors = colors;
    }

    string ProfileDisplayName(ControlProfile profile)
    {
        switch (profile)
        {
            case ControlProfile.FightStick: return "Fight Stick";
            case ControlProfile.Gamepad:    return "Gamepad";
            case ControlProfile.Keyboard:   return "Keyboard";
            default:                        return profile.ToString();
        }
    }
}
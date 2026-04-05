// FirstRunSetupUI.cs
// ─────────────────────────────────────────────────────────────────────────────
// Attach to a manager GameObject in your FirstRunSetup scene.
// Handles device detection (display only), profile selection, saving, and
// transitioning to the main menu after the player confirms their profile.
//
// ── SCENE SETUP ──────────────────────────────────────────────────────────────
// Your FirstRunSetup scene needs:
//   • A Canvas with:
//       - detectedDeviceText    (TextMeshPro or Text) — shows what Unity detected
//       - recommendationText    (TextMeshPro or Text) — "We recommend: FightStick"
//       - fightStickButton      (Button) — profile option
//       - gamepadButton         (Button) — profile option
//       - keyboardButton        (Button) — profile option
//       - continueButton        (Button) — disabled until a profile is chosen
//   • This script on a manager GameObject, with all references wired in Inspector.
//
// ── DEPENDENCIES ─────────────────────────────────────────────────────────────
//   Requires: Unity Input System package (for device detection display only)
//   Install via Package Manager: "Input System" → com.unity.inputsystem
//   NOTE: This does NOT replace legacy input for gameplay. It only reads
//         connected device names to display a recommendation to the player.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_INPUT_SYSTEM_AVAILABLE
using UnityEngine.InputSystem;
#endif

public class FirstRunSetupUI : MonoBehaviour
{
    [Header("Scene to load after setup")]
    public string mainMenuScene = "MainMenu";  // ← match your actual scene name

    [Header("UI References")]
    public TextMeshProUGUI detectedDeviceText;
    public TextMeshProUGUI recommendationText;

    [Header("Profile Buttons")]
    public Button fightStickButton;
    public Button gamepadButton;
    public Button keyboardButton;

    [Header("Navigation")]
    public Button continueButton;

    [Header("Visual Feedback — Selected Button Colors")]
    public Color selectedColor   = new Color(1f,  0.6f, 0f);    // orange highlight
    public Color unselectedColor = new Color(0.2f, 0.2f, 0.2f); // dark grey

    // ── State ─────────────────────────────────────────────────────────────────
    private ControlProfile? selectedProfile = null;

    void Start()
    {
        // Continue is disabled until player explicitly picks a profile
        continueButton.interactable = false;

        // Wire buttons
        fightStickButton.onClick.AddListener(() => SelectProfile(ControlProfile.FightStick));
        gamepadButton.onClick.AddListener(   () => SelectProfile(ControlProfile.Gamepad));
        keyboardButton.onClick.AddListener(  () => SelectProfile(ControlProfile.Keyboard));
        continueButton.onClick.AddListener(OnContinue);

        // Detect device and auto-recommend
        DetectAndRecommend();

        // Reset all button colors
        SetButtonColor(fightStickButton, unselectedColor);
        SetButtonColor(gamepadButton,    unselectedColor);
        SetButtonColor(keyboardButton,   unselectedColor);
    }

    // ── Device Detection (display + recommendation only) ──────────────────────

    void DetectAndRecommend()
    {
        ControlProfile recommended = ControlProfile.FightStick; // default: hero profile
        string         deviceName  = "No device detected";

#if UNITY_INPUT_SYSTEM_AVAILABLE
        // Use new Input System ONLY for reading device names — not for gameplay
        var gamepads = Gamepad.all;
        if (gamepads.Count > 0)
        {
            deviceName = gamepads[0].displayName ?? gamepads[0].name;

            // Note: A fight stick may appear as a generic HID or Xbox controller.
            // We do NOT auto-select FightStick from this alone — we only recommend.
            // The player must confirm. FightStick is always the default recommendation
            // because it is the primary design target of this game.
            recommended = ControlProfile.FightStick;
        }
        else if (Keyboard.current != null)
        {
            deviceName  = "Keyboard";
            recommended = ControlProfile.Keyboard;
        }
#else
        // Fallback if Input System package isn't installed:
        // Check Unity's legacy joystick detection
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

        // Update UI
        if (detectedDeviceText != null)
            detectedDeviceText.text = $"Detected: {deviceName}";

        if (recommendationText != null)
            recommendationText.text = $"Recommended: {ProfileDisplayName(recommended)}\n" +
                                       "<size=70%>(You may choose any profile below)</size>";

        // Pre-highlight recommended button but do NOT auto-select —
        // the player must press Continue themselves after confirming.
        HighlightRecommended(recommended);
    }

    void HighlightRecommended(ControlProfile recommended)
    {
        // Dim all, then gently highlight the recommended one
        SetButtonColor(fightStickButton, unselectedColor);
        SetButtonColor(gamepadButton,    unselectedColor);
        SetButtonColor(keyboardButton,   unselectedColor);

        // Use a lighter highlight for "recommended" vs full orange for "selected"
        Color recommendedColor = Color.Lerp(unselectedColor, selectedColor, 0.35f);
        switch (recommended)
        {
            case ControlProfile.FightStick: SetButtonColor(fightStickButton, recommendedColor); break;
            case ControlProfile.Gamepad:    SetButtonColor(gamepadButton,    recommendedColor); break;
            case ControlProfile.Keyboard:   SetButtonColor(keyboardButton,   recommendedColor); break;
        }
    }

    // ── Profile Selection ─────────────────────────────────────────────────────

    void SelectProfile(ControlProfile profile)
    {
        selectedProfile = profile;

        // Highlight selected, dim others
        SetButtonColor(fightStickButton, profile == ControlProfile.FightStick ? selectedColor : unselectedColor);
        SetButtonColor(gamepadButton,    profile == ControlProfile.Gamepad    ? selectedColor : unselectedColor);
        SetButtonColor(keyboardButton,   profile == ControlProfile.Keyboard   ? selectedColor : unselectedColor);

        continueButton.interactable = true;

        Debug.Log($"[FirstRunSetup] Profile selected: {profile}");
    }

    // ── Continue / Save ───────────────────────────────────────────────────────

    void OnContinue()
    {
        if (selectedProfile == null)
        {
            Debug.LogWarning("[FirstRunSetup] Continue pressed with no profile selected — this shouldn't happen.");
            return;
        }

        SaveProfile(selectedProfile.Value);
        SceneManager.LoadScene(mainMenuScene);
    }

    public static void SaveProfile(ControlProfile profile)
    {
        PlayerPrefs.SetString(ControlProfileApplicator.KEY_PROFILE, profile.ToString());
        PlayerPrefs.SetInt(ControlProfileApplicator.KEY_SETUP_COMPLETE, 1);
        PlayerPrefs.Save();
        Debug.Log($"[FirstRunSetup] Saved profile: {profile}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

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

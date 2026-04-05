// ControlsSettingsUI.cs
// ─────────────────────────────────────────────────────────────────────────────
// Attach to your settings/controls panel in the main menu scene.
// Phase 2 scope: shows current profile, allows switching profile and
// resetting to defaults. Full per-binding rebind UI is Phase 3.
//
// ── SCENE SETUP ──────────────────────────────────────────────────────────────
// Wire in Inspector:
//   currentProfileText  — shows "Current Profile: Fight Stick"
//   fightStickButton    — switch to fight stick profile
//   gamepadButton       — switch to gamepad profile
//   keyboardButton      — switch to keyboard profile
//   resetDefaultsButton — resets bindings to defaults for current profile
//   backButton          — closes/hides this panel
//
// No ControlProfileApplicator is needed in the main menu scene because
// InputManager only exists in the gameplay scene. Profile changes here
// are saved to PlayerPrefs and applied by ControlProfileApplicator
// when the gameplay scene loads.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlsSettingsUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI currentProfileText;

    [Header("Profile Switch Buttons")]
    public Button fightStickButton;
    public Button gamepadButton;
    public Button keyboardButton;

    [Header("Actions")]
    public Button resetDefaultsButton;
    public Button backButton;

    [Header("Visual")]
    public Color selectedColor   = new Color(1f, 0.6f, 0f);
    public Color unselectedColor = new Color(0.2f, 0.2f, 0.2f);

    // Optional: reference to the panel's root GameObject so Back can hide it
    [Header("Panel (optional — for hide/show)")]
    public GameObject panelRoot;

    void OnEnable()
    {
        // Refresh display every time the panel opens
        RefreshDisplay();
    }

    void Start()
    {
        fightStickButton.onClick.AddListener(() => SwitchProfile(ControlProfile.FightStick));
        gamepadButton.onClick.AddListener(   () => SwitchProfile(ControlProfile.Gamepad));
        keyboardButton.onClick.AddListener(  () => SwitchProfile(ControlProfile.Keyboard));
        resetDefaultsButton.onClick.AddListener(ResetToDefaults);

        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    void RefreshDisplay()
    {
        string saved  = PlayerPrefs.GetString(ControlProfileApplicator.KEY_PROFILE, "FightStick");
        System.Enum.TryParse(saved, out ControlProfile current);

        if (currentProfileText != null)
            currentProfileText.text = $"Current Profile: {ProfileDisplayName(current)}";

        SetButtonColor(fightStickButton, current == ControlProfile.FightStick ? selectedColor : unselectedColor);
        SetButtonColor(gamepadButton,    current == ControlProfile.Gamepad    ? selectedColor : unselectedColor);
        SetButtonColor(keyboardButton,   current == ControlProfile.Keyboard   ? selectedColor : unselectedColor);
    }

    void SwitchProfile(ControlProfile profile)
    {
        // Save new profile — ControlProfileApplicator will pick it up when gameplay loads
        FirstRunSetupUI.SaveProfile(profile);
        RefreshDisplay();
        Debug.Log($"[ControlsSettings] Profile changed to: {profile}");
    }

    void ResetToDefaults()
    {
        // Nothing extra to reset in Phase 2 (no custom bindings yet).
        // When Phase 3 (rebinding) is implemented, delete custom binding keys here.
        string saved = PlayerPrefs.GetString(ControlProfileApplicator.KEY_PROFILE, "FightStick");
        Debug.Log($"[ControlsSettings] Reset to defaults for profile: {saved} (no custom bindings to clear yet)");

        // Future Phase 3: PlayerPrefs.DeleteKey("custom_bindings"); etc.
    }

    void OnBack()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);
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

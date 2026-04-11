using UnityEngine;

public class ControlProfileApplicator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave null to auto-find InputManager in scene via FindObjectOfType.")]
    public InputManager inputManagerRef;

    [Header("Debug")]
    [Tooltip("Log applied profile and field values to Console.")]
    public bool debugLog = true;

    // PlayerPrefs keys — keep in sync with FirstRunSetupUI
    public const string KEY_SETUP_COMPLETE = "input_setup_complete";
    public const string KEY_PROFILE        = "control_profile";

    void Awake()
    {
        // Resolve InputManager reference
        if (inputManagerRef == null)
            inputManagerRef = FindObjectOfType<InputManager>();

        if (inputManagerRef == null)
        {
            Debug.LogError("[ControlProfileApplicator] Could not find InputManager in scene. " +
                           "Attach InputManager to a GameObject in this scene, or assign it manually.");
            return;
        }

        ApplySavedProfile();
    }

    void ApplySavedProfile()
    {
        // (the hero profile) so the game is still playable on first load.
        if (PlayerPrefs.GetInt(KEY_SETUP_COMPLETE, 0) == 0)
        {
            if (debugLog)
                Debug.Log("[ControlProfileApplicator] No profile saved — applying FightStick defaults.");
            ApplyProfile(ControlProfile.FightStick);
            return;
        }

        string saved = PlayerPrefs.GetString(KEY_PROFILE, "FightStick");

        if (!System.Enum.TryParse(saved, out ControlProfile profile))
        {
            Debug.LogWarning($"[ControlProfileApplicator] Unknown profile '{saved}' in PlayerPrefs. " +
                              "Falling back to FightStick.");
            profile = ControlProfile.FightStick;
        }

        ApplyProfile(profile);
    }

    void ApplyProfile(ControlProfile profile)
    {
        InputProfileData data = DefaultMappings.Get(profile);
        CustomBindingsStore.ApplyOverrides(profile, data);
        ApplyToInputManager(data);

        if (debugLog)
        {
            Debug.Log($"[ControlProfileApplicator] Applied profile: {profile}\n" +
                      $"  Lane3 top={data.lane3.topButton}/{data.lane3.topKey}  " +
                                  $"bot={data.lane3.bottomButton}/{data.lane3.bottomKey}\n" +
                      $"  Lane4 top={data.lane4.topButton}/{data.lane4.topKey}  " +
                                  $"bot={data.lane4.bottomButton}/{data.lane4.bottomKey}\n" +
                      $"  Lane5 top={data.lane5.topButton}/{data.lane5.topKey}  " +
                                  $"bot=axis:{data.lane5.bottomAxis}/{data.lane5.bottomKey}");
        }
    }

    void ApplyToInputManager(InputProfileData data)
    {
        InputManager im = inputManagerRef;

        // Lane 3
        im.lane3TopIsButton    = data.lane3.topIsButton;
        im.lane3TopButton      = data.lane3.topButton;
        im.lane3TopAxis        = data.lane3.topAxis;
        im.lane3TopKey         = data.lane3.topKey;

        im.lane3BottomIsButton = data.lane3.bottomIsButton;
        im.lane3BottomButton   = data.lane3.bottomButton;
        im.lane3BottomAxis     = data.lane3.bottomAxis;
        im.lane3BottomKey      = data.lane3.bottomKey;

        // Lane 4
        im.lane4TopIsButton    = data.lane4.topIsButton;
        im.lane4TopButton      = data.lane4.topButton;
        im.lane4TopAxis        = data.lane4.topAxis;
        im.lane4TopKey         = data.lane4.topKey;

        im.lane4BottomIsButton = data.lane4.bottomIsButton;
        im.lane4BottomButton   = data.lane4.bottomButton;
        im.lane4BottomAxis     = data.lane4.bottomAxis;
        im.lane4BottomKey      = data.lane4.bottomKey;

        // Lane 5
        im.lane5TopIsButton    = data.lane5.topIsButton;
        im.lane5TopButton      = data.lane5.topButton;
        im.lane5TopAxis        = data.lane5.topAxis;
        im.lane5TopKey         = data.lane5.topKey;

        im.lane5BottomIsButton = data.lane5.bottomIsButton;
        im.lane5BottomButton   = data.lane5.bottomButton;
        im.lane5BottomAxis     = data.lane5.bottomAxis;
        im.lane5BottomKey      = data.lane5.bottomKey;
    }

    // Public utility — call this from a Settings/Controls page reset button
    // e.g.: FindObjectOfType<ControlProfileApplicator>().ResetToCurrentProfileDefaults();
    public void ResetToCurrentProfileDefaults()
    {
        string saved = PlayerPrefs.GetString(KEY_PROFILE, "FightStick");
        if (System.Enum.TryParse(saved, out ControlProfile profile))
            ApplyProfile(profile);
    }
}

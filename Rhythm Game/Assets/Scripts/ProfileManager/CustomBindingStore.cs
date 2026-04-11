using UnityEngine;

public static class CustomBindingsStore
{
    private const string Prefix = "custom_binding";

    // Key builder

    private static string MakeKey(ControlProfile profile, string laneName, string side, string field)
    {
        return $"{Prefix}_{profile}_{laneName}_{side}_{field}";
    }

    // Public save API

    public static void SaveLaneTop(ControlProfile profile, string laneName,
                                   bool isButton, int button, string axis, KeyCode key)
    {
        SaveLane(profile, laneName, "top", isButton, button, axis, key);
    }

    public static void SaveLaneBottom(ControlProfile profile, string laneName,
                                      bool isButton, int button, string axis, KeyCode key)
    {
        SaveLane(profile, laneName, "bottom", isButton, button, axis, key);
    }

    private static void SaveLane(ControlProfile profile, string laneName, string side,
                                  bool isButton, int button, string axis, KeyCode key)
    {
        PlayerPrefs.SetInt(   MakeKey(profile, laneName, side, "exists"),   1);
        PlayerPrefs.SetInt(   MakeKey(profile, laneName, side, "isButton"), isButton ? 1 : 0);
        PlayerPrefs.SetInt(   MakeKey(profile, laneName, side, "button"),   button);
        PlayerPrefs.SetString(MakeKey(profile, laneName, side, "axis"),     axis ?? "");
        PlayerPrefs.SetInt(   MakeKey(profile, laneName, side, "key"),      (int)key);
        PlayerPrefs.Save();
    }

    // Query
    public static bool HasLaneOverride(ControlProfile profile, string laneName, string side)
    {
        return PlayerPrefs.GetInt(MakeKey(profile, laneName, side, "exists"), 0) == 1;
    }

    // Apply overrides on top of a default mapping
    // Called by ControlProfileApplicator after DefaultMappings.Get(),
    // before writing values into InputManager.

    public static void ApplyOverrides(ControlProfile profile, InputProfileData data)
    {
        ApplyLaneOverride(profile, "lane3", "top",    data.lane3, true);
        ApplyLaneOverride(profile, "lane3", "bottom", data.lane3, false);
        ApplyLaneOverride(profile, "lane4", "top",    data.lane4, true);
        ApplyLaneOverride(profile, "lane4", "bottom", data.lane4, false);
        ApplyLaneOverride(profile, "lane5", "top",    data.lane5, true);
        ApplyLaneOverride(profile, "lane5", "bottom", data.lane5, false);
    }

    private static void ApplyLaneOverride(ControlProfile profile, string laneName,
                                           string side, LaneButtonConfig lane, bool isTop)
    {
        if (!HasLaneOverride(profile, laneName, side)) return;

        bool    savedIsButton = PlayerPrefs.GetInt(   MakeKey(profile, laneName, side, "isButton"), 1) == 1;
        int     savedButton   = PlayerPrefs.GetInt(   MakeKey(profile, laneName, side, "button"),   0);
        string  savedAxis     = PlayerPrefs.GetString(MakeKey(profile, laneName, side, "axis"),      "");
        KeyCode savedKey      = (KeyCode)PlayerPrefs.GetInt(MakeKey(profile, laneName, side, "key"), (int)KeyCode.None);

        if (isTop)
        {
            lane.topIsButton = savedIsButton;
            lane.topButton   = savedButton;
            lane.topAxis     = savedAxis;
            lane.topKey      = savedKey;
        }
        else
        {
            lane.bottomIsButton = savedIsButton;
            lane.bottomButton   = savedButton;
            lane.bottomAxis     = savedAxis;
            lane.bottomKey      = savedKey;
        }
    }

    // Reset to defaults — clears all overrides for a profile
    // After calling this, ControlProfileApplicator will fall back to DefaultMappings.

    public static void ClearOverrides(ControlProfile profile)
    {
        ClearLane(profile, "lane3", "top");
        ClearLane(profile, "lane3", "bottom");
        ClearLane(profile, "lane4", "top");
        ClearLane(profile, "lane4", "bottom");
        ClearLane(profile, "lane5", "top");
        ClearLane(profile, "lane5", "bottom");
        PlayerPrefs.Save();
    }

    private static void ClearLane(ControlProfile profile, string laneName, string side)
    {
        PlayerPrefs.DeleteKey(MakeKey(profile, laneName, side, "exists"));
        PlayerPrefs.DeleteKey(MakeKey(profile, laneName, side, "isButton"));
        PlayerPrefs.DeleteKey(MakeKey(profile, laneName, side, "button"));
        PlayerPrefs.DeleteKey(MakeKey(profile, laneName, side, "axis"));
        PlayerPrefs.DeleteKey(MakeKey(profile, laneName, side, "key"));
    }
}
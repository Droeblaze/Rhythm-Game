// InputProfileData.cs
// Serializable data container that holds the complete button/axis mapping
// for lanes 3, 4, and 5. Lanes 0-2 are always driven by Horizontal/Vertical
// axes and never need profile-based overrides.
//
// LaneButtonConfig mirrors the serialized fields in InputManager exactly:
//   topIsButton / topButton / topAxis
//   bottomIsButton / bottomButton / bottomAxis
// Plus topKey / bottomKey for keyboard profile KeyCode overrides (see InputManager changes).

using UnityEngine;

[System.Serializable]
public class LaneButtonConfig
{
    // --- Button path (joystick buttons) ---
    public bool   topIsButton;
    public int    topButton;
    public string topAxis;
    public KeyCode topKey;       // KeyCode.None = not using keyboard override

    // --- Bottom button ---
    public bool   bottomIsButton;
    public int    bottomButton;
    public string bottomAxis;
    public KeyCode bottomKey;    // KeyCode.None = not using keyboard override
}

[System.Serializable]
public class InputProfileData
{
    public ControlProfile profile;
    public LaneButtonConfig lane3;
    public LaneButtonConfig lane4;
    public LaneButtonConfig lane5;
}

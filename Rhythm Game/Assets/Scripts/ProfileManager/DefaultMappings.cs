// Static lookup table — returns the correct InputProfileData for each ControlProfile.
// This is the ONLY place where button numbers, axis names, and key codes are defined.
// To change any default binding, edit ONLY this file.
//
// FIGHT STICK (PRIMARY / hero profile)
//   Uses the button numbers already configured in InputManager's serialized defaults.
//   Lane 3: JoystickButton8  (top) / JoystickButton9  (bottom)
//   Lane 4: JoystickButton10 (top) / JoystickButton11 (bottom)
//   Lane 5: JoystickButton12 (top) / RightTrigger axis (bottom)
//
//   2-button chord (top + bottom same lane):  handled by ButtonRow.Both in InputManager ✓
//   4-button chord (two full lanes at once):  requires cross-lane detection — see TODO below
//
// GAMEPAD (fallback)
//   Standard Xbox/PS face button layout.
//   Lane 3: A/Cross (0) top    / X/Square  (2) bottom
//   Lane 4: B/Circle (1) top   / Y/Triangle(3) bottom
//   Lane 5: LB/L1   (4) top   / RightTrigger axis bottom
//
// KEYBOARD (fallback)
//   Lanes 0-2: arrow keys / WASD drive Horizontal+Vertical axes — no config needed.
//   Lanes 3-5 layout (vertical pairs, top row is higher on keyboard):
//
//   Lane:      3      4      5
//   Top row:   U      I      O      ← upper keys (physically higher)
//   Bot row:   J      K      L      ← lower keys (physically lower)
//
//   2-button chord: hold both U+J, I+K, or O+L simultaneously → ButtonRow.Both
//   4-button chord: requires cross-lane detection — see TODO below

using UnityEngine;

public static class DefaultMappings
{
    // Public entry point

    public static InputProfileData Get(ControlProfile profile)
    {
        switch (profile)
        {
            case ControlProfile.FightStick: return FightStick();
            case ControlProfile.Gamepad:    return Gamepad();
            case ControlProfile.Keyboard:   return Keyboard();
            default:                        return FightStick(); // hero profile is the safe fallback
        }
    }

    // FightStick 
    static InputProfileData FightStick() => new InputProfileData
    {
        profile = ControlProfile.FightStick,

        lane3 = new LaneButtonConfig
        {
            topIsButton    = true,  topButton    = 8,  topAxis    = "", topKey    = KeyCode.None,
            bottomIsButton = true,  bottomButton = 9,  bottomAxis = "", bottomKey = KeyCode.None
        },
        lane4 = new LaneButtonConfig
        {
            topIsButton    = true,  topButton    = 10, topAxis    = "", topKey    = KeyCode.None,
            bottomIsButton = true,  bottomButton = 11, bottomAxis = "", bottomKey = KeyCode.None
        },
        lane5 = new LaneButtonConfig
        {
            topIsButton    = true,  topButton    = 12, topAxis    = "", topKey    = KeyCode.None,
            bottomIsButton = false, bottomButton = 0,  bottomAxis = "RightTrigger", bottomKey = KeyCode.None
        }
    };

    // Gamepad

    static InputProfileData Gamepad() => new InputProfileData
    {
        profile = ControlProfile.Gamepad,

        lane3 = new LaneButtonConfig
        {
            topIsButton    = true,  topButton    = 0, topAxis    = "", topKey    = KeyCode.None, // A / Cross
            bottomIsButton = true,  bottomButton = 2, bottomAxis = "", bottomKey = KeyCode.None  // X / Square
        },
        lane4 = new LaneButtonConfig
        {
            topIsButton    = true,  topButton    = 1, topAxis    = "", topKey    = KeyCode.None, // B / Circle
            bottomIsButton = true,  bottomButton = 3, bottomAxis = "", bottomKey = KeyCode.None  // Y / Triangle
        },
        lane5 = new LaneButtonConfig
        {
            topIsButton    = true,  topButton    = 4,  topAxis    = "", topKey    = KeyCode.None, // LB / L1
            bottomIsButton = false, bottomButton = 0,  bottomAxis = "RightTrigger", bottomKey = KeyCode.None
        }
    };

    // Keyboard
    // Keys arranged as vertical pairs matching the fight stick's top/bottom layout.
    // Top row = physically higher keys, Bottom row = physically lower keys.
    //
    //   Lane:   3    4    5
    //   Top:    U    I    O
    //   Bottom: J    K    L

    static InputProfileData Keyboard() => new InputProfileData
    {
        profile = ControlProfile.Keyboard,

        lane3 = new LaneButtonConfig
        {
            topIsButton    = false, topButton    = 0, topAxis    = "", topKey    = KeyCode.U,  // top:    U
            bottomIsButton = false, bottomButton = 0, bottomAxis = "", bottomKey = KeyCode.J   // bottom: J
        },
        lane4 = new LaneButtonConfig
        {
            topIsButton    = false, topButton    = 0, topAxis    = "", topKey    = KeyCode.I,  // top:    I
            bottomIsButton = false, bottomButton = 0, bottomAxis = "", bottomKey = KeyCode.K   // bottom: K
        },
        lane5 = new LaneButtonConfig
        {
            topIsButton    = false, topButton    = 0, topAxis    = "", topKey    = KeyCode.O,  // top:    O
            bottomIsButton = false, bottomButton = 0, bottomAxis = "", bottomKey = KeyCode.L   // bottom: L
        }
    };
}

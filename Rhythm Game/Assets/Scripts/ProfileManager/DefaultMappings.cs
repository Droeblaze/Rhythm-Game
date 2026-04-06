// Static lookup table — returns the correct InputProfileData for each ControlProfile.
// This is the ONLY place where button numbers, axis names, and key codes are defined.
// To change any default binding, edit ONLY this file.
//
// FIGHT STICK (PRIMARY PROFILE)
//   Uses the button numbers the team already configured in InputManager defaults.
//   Lane 3: JoystickButton8  / JoystickButton9
//   Lane 4: JoystickButton10 / JoystickButton11
//   Lane 5: JoystickButton12 / RightTrigger axis
//
// GAMEPAD (FALLBACK)
//   Standard Xbox controller Unity button mapping:
//   JoystickButton0=A, 1=B, 2=X, 3=Y, 4=LB, 5=RB, RightTrigger axis for RT
//   Lane 3: A(0) top / X(2) bottom
//   Lane 4: B(1) top / Y(3) bottom
//   Lane 5: LB(4) top / RightTrigger axis bottom
//
// KEYBOARD (FALLBACK)
//   Lanes 0-2 already work via Unity's Horizontal/Vertical axes (arrow keys/WASD).
//   Lanes 3-5 use KeyCode overrides (new fields added to InputManager).
//   Top row:    J / K / L  (lanes 3 / 4 / 5)
//   Bottom row: U / I / O  (lanes 3 / 4 / 5)
//   → Edit these in the Keyboard() method below if anyone chose different keys.

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
            default:                        return FightStick(); // safe fallback to hero profile
        }
    }

    // FightStick
    // Matches InputManager's existing serialized defaults exactly.
    // This profile is the primary design target for this game.

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

    //  Gamepad
    // Standard Xbox face button layout. If using PS controller, Unity remaps
    // Cross/Square/Circle/Triangle to 0/2/1/3 — same numbers, different labels.

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
            bottomIsButton = false, bottomButton = 0,  bottomAxis = "RightTrigger", bottomKey = KeyCode.None // RT / R2
        }
    };

    // Keyboard 
    // Lanes 0-2: arrow keys / WASD already drive Horizontal+Vertical axes — no config needed.
    // Lanes 3-5: two key rows. topKey / bottomKey are read via new KeyCode fields
    //            added to InputManager (see InputManager_KeyboardAdditions.cs).
    // isButton=false + axis="" + key=<KeyCode> = keyboard override path.

    static InputProfileData Keyboard() => new InputProfileData
    {
        profile = ControlProfile.Keyboard,

        lane3 = new LaneButtonConfig
        {
            topIsButton    = false, topButton    = 0, topAxis    = "", topKey    = KeyCode.J,  // top:    J
            bottomIsButton = false, bottomButton = 0, bottomAxis = "", bottomKey = KeyCode.U   // bottom: U
        },
        lane4 = new LaneButtonConfig
        {
            topIsButton    = false, topButton    = 0, topAxis    = "", topKey    = KeyCode.K,  // top:    K
            bottomIsButton = false, bottomButton = 0, bottomAxis = "", bottomKey = KeyCode.I   // bottom: I
        },
        lane5 = new LaneButtonConfig
        {
            topIsButton    = false, topButton    = 0, topAxis    = "", topKey    = KeyCode.L,  // top:    L
            bottomIsButton = false, bottomButton = 0, bottomAxis = "", bottomKey = KeyCode.O   // bottom: O
        }
    };
}

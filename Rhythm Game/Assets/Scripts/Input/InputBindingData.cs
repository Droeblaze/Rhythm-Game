using System;
using UnityEngine;

/// <summary>
/// Stores a single input binding. Can be a joystick button, a keyboard key,
/// an axis name, or a stick direction.
/// </summary>
[Serializable]
public class InputBinding
{
    public enum BindingType { JoystickButton, Axis, KeyboardKey }

    public BindingType type = BindingType.JoystickButton;
    public int joystickButtonIndex;      // e.g. 8 ? JoystickButton8
    public string axisName = "";         // e.g. "RightTrigger"
    public float axisThreshold = 0.5f;   // threshold for axis-based bindings
    public KeyCode keyboardKey = KeyCode.None;
}

/// <summary>
/// Full set of bindings for the game: 4 directional + 6 button inputs,
/// plus the joystick index of the selected device.
/// </summary>
[Serializable]
public class InputBindingData
{
    // -1 = any joystick, 1-8 = specific Unity joystick index
    public int deviceJoystickIndex = -1;
    public string deviceName = "Any";

    // Directional inputs (stick / d-pad)
    public InputBinding dirUp    = new InputBinding { type = InputBinding.BindingType.Axis, axisName = "Vertical", axisThreshold = 0.5f };
    public InputBinding dirDown  = new InputBinding { type = InputBinding.BindingType.Axis, axisName = "Vertical", axisThreshold = -0.5f };
    public InputBinding dirLeft  = new InputBinding { type = InputBinding.BindingType.Axis, axisName = "Horizontal", axisThreshold = -0.5f };
    public InputBinding dirRight = new InputBinding { type = InputBinding.BindingType.Axis, axisName = "Horizontal", axisThreshold = 0.5f };

    // 6 face / action buttons  (default: joystick buttons 8-13 matching current layout)
    public InputBinding button1 = new InputBinding { type = InputBinding.BindingType.JoystickButton, joystickButtonIndex = 8 };  // Lane3 Top
    public InputBinding button2 = new InputBinding { type = InputBinding.BindingType.JoystickButton, joystickButtonIndex = 9 };  // Lane3 Bottom
    public InputBinding button3 = new InputBinding { type = InputBinding.BindingType.JoystickButton, joystickButtonIndex = 10 }; // Lane4 Top
    public InputBinding button4 = new InputBinding { type = InputBinding.BindingType.JoystickButton, joystickButtonIndex = 11 }; // Lane4 Bottom
    public InputBinding button5 = new InputBinding { type = InputBinding.BindingType.JoystickButton, joystickButtonIndex = 12 }; // Lane5 Top
    public InputBinding button6 = new InputBinding { type = InputBinding.BindingType.JoystickButton, joystickButtonIndex = 13 }; // Lane5 Bottom

    public InputBinding[] GetAllBindings()
    {
        return new InputBinding[] { dirUp, dirDown, dirLeft, dirRight, button1, button2, button3, button4, button5, button6 };
    }

    public static readonly string[] BindingLabels = new string[]
    {
        "Direction Up", "Direction Down", "Direction Left", "Direction Right",
        "Button 1 (Lane3 Top)", "Button 2 (Lane3 Bottom)",
        "Button 3 (Lane4 Top)", "Button 4 (Lane4 Bottom)",
        "Button 5 (Lane5 Top)", "Button 6 (Lane5 Bottom)"
    };
}
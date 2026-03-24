using UnityEngine;

/// <summary>
/// Singleton that persists across scenes. All scripts query this instead of
/// reading hardcoded button numbers.
/// </summary>
public class InputBindingManager : MonoBehaviour
{
    public static InputBindingManager Instance { get; private set; }

    private const string PREFS_KEY = "InputBindings";

    public InputBindingData Bindings { get; set; }

    // Previous-frame state for edge detection
    private bool[] prevButtonStates;
    private bool[] prevDirStates;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadBindings();
        prevButtonStates = new bool[6];
        prevDirStates = new bool[4];
    }

    // ??? Persistence ???????????????????????????????????????????
    public void LoadBindings()
    {
        string json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            Bindings = JsonUtility.FromJson<InputBindingData>(json);
        }
        else
        {
            Bindings = new InputBindingData();
        }
    }

    public void SaveBindings()
    {
        string json = JsonUtility.ToJson(Bindings, true);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        Bindings = new InputBindingData();
        SaveBindings();
    }

    // ??? Query helpers ?????????????????????????????????????????

    /// <summary>Returns true the frame the binding is first pressed.</summary>
    public bool GetBindingDown(InputBinding binding)
    {
        return EvaluateBinding(binding, true);
    }

    /// <summary>Returns true every frame the binding is held.</summary>
    public bool GetBindingHeld(InputBinding binding)
    {
        return EvaluateBinding(binding, false);
    }

    private bool EvaluateBinding(InputBinding binding, bool downOnly)
    {
        switch (binding.type)
        {
            case InputBinding.BindingType.JoystickButton:
                KeyCode kc = KeyCode.JoystickButton0 + binding.joystickButtonIndex;
                return downOnly ? Input.GetKeyDown(kc) : Input.GetKey(kc);

            case InputBinding.BindingType.Axis:
                if (string.IsNullOrEmpty(binding.axisName)) return false;
                float val = Input.GetAxisRaw(binding.axisName);
                // Threshold can be negative (e.g. -0.5 for "left")
                if (binding.axisThreshold >= 0)
                    return val >= binding.axisThreshold;
                else
                    return val <= binding.axisThreshold;

            case InputBinding.BindingType.KeyboardKey:
                return downOnly ? Input.GetKeyDown(binding.keyboardKey) : Input.GetKey(binding.keyboardKey);

            default:
                return false;
        }
    }

    // ??? Convenience accessors matching old InputManager API ???

    /// <summary>Stick deadzone-style quantized direction: -1, 0, or 1.</summary>
    public int GetHorizontalDir(float deadzone = 0.5f)
    {
        float h = Input.GetAxisRaw("Horizontal");
        return h < -deadzone ? -1 : (h > deadzone ? 1 : 0);
    }

    public int GetVerticalDir(float deadzone = 0.5f)
    {
        float v = Input.GetAxisRaw("Vertical");
        return v < -deadzone ? -1 : (v > deadzone ? 1 : 0);
    }

    // Lane button helpers – returns the binding for a given lane + row
    public InputBinding GetLaneTopBinding(int lane)
    {
        switch (lane)
        {
            case 3: return Bindings.button1;
            case 4: return Bindings.button3;
            case 5: return Bindings.button5;
            default: return null;
        }
    }

    public InputBinding GetLaneBottomBinding(int lane)
    {
        switch (lane)
        {
            case 3: return Bindings.button2;
            case 4: return Bindings.button4;
            case 5: return Bindings.button6;
            default: return null;
        }
    }
}
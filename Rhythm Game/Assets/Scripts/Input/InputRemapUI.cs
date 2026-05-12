using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class InputRemapUI : MonoBehaviour
{
    [Header("Device Detection")]
    public Button detectDeviceButton;
    public TextMeshProUGUI deviceNameText;
    public TextMeshProUGUI detectStatusText;

    [Header("Binding Buttons (10 total, order matches InputBindingData.BindingLabels)")]
    public Button[] bindingButtons;   // Assign 10 buttons in inspector
    public TextMeshProUGUI[] bindingLabels; // Shows current assignment

    [Header("Navigation")]
    public Button saveButton;
    public Button resetButton;
    public Button backButton;

    private InputBindingData editData;
    private int currentRemapIndex = -1;    // -1 = not remapping
    private bool detectingDevice = false;
    private Coroutine remapCoroutine;

    void Start()
    {
        // Work on a copy so we can cancel changes
        if (InputBindingManager.Instance != null)
        {
            string json = JsonUtility.ToJson(InputBindingManager.Instance.Bindings);
            editData = JsonUtility.FromJson<InputBindingData>(json);
        }
        else
        {
            editData = new InputBindingData();
        }

        SetupUI();
        RefreshAllLabels();
    }

    void SetupUI()
    {
        if (detectDeviceButton != null)
            detectDeviceButton.onClick.AddListener(StartDeviceDetection);

        for (int i = 0; i < bindingButtons.Length && i < 10; i++)
        {
            int index = i; // capture for closure
            bindingButtons[i].onClick.AddListener(() => StartRemap(index));
        }

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveAndBack);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetDefaults);

        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
    }

    // ??? Device Detection ??????????????????????????????????????

    void StartDeviceDetection()
    {
        if (remapCoroutine != null) StopCoroutine(remapCoroutine);
        remapCoroutine = StartCoroutine(DetectDeviceCoroutine());
    }

    IEnumerator DetectDeviceCoroutine()
    {
        detectingDevice = true;
        if (detectStatusText != null)
            detectStatusText.text = "Press any button on the device you want to use...";

        SetBindingButtonsInteractable(false);

        // Wait one frame so the click on "Detect" doesn't instantly register
        yield return null;
        yield return null;

        while (detectingDevice)
        {
            // Scan all joystick buttons across all 8 possible Unity joysticks
            for (int joy = 1; joy <= 8; joy++)
            {
                for (int btn = 0; btn < 20; btn++)
                {
                    KeyCode kc = (KeyCode)System.Enum.Parse(typeof(KeyCode), $"Joystick{joy}Button{btn}");
                    if (Input.GetKeyDown(kc))
                    {
                        editData.deviceJoystickIndex = joy;
                        string[] names = Input.GetJoystickNames();
                        editData.deviceName = (joy - 1 < names.Length && !string.IsNullOrEmpty(names[joy - 1]))
                            ? names[joy - 1]
                            : $"Joystick {joy}";
                        detectingDevice = false;
                        break;
                    }
                }
                if (!detectingDevice) break;
            }

            // Also allow keyboard press to mark "keyboard" as device
            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (kc == KeyCode.Mouse0 || kc == KeyCode.Mouse1 || kc == KeyCode.Mouse2) continue;
                if (kc >= KeyCode.JoystickButton0) break; // skip joystick range, handled above

                if (Input.GetKeyDown(kc))
                {
                    editData.deviceJoystickIndex = -1;
                    editData.deviceName = "Keyboard";
                    detectingDevice = false;
                    break;
                }
            }

            yield return null;
        }

        if (detectStatusText != null)
            detectStatusText.text = $"Device set: {editData.deviceName}";

        if (deviceNameText != null)
            deviceNameText.text = editData.deviceName;

        SetBindingButtonsInteractable(true);
        remapCoroutine = null;
    }

    // ??? Remapping ?????????????????????????????????????????????

    void StartRemap(int index)
    {
        if (remapCoroutine != null) StopCoroutine(remapCoroutine);
        remapCoroutine = StartCoroutine(RemapCoroutine(index));
    }

    IEnumerator RemapCoroutine(int index)
    {
        currentRemapIndex = index;
        SetBindingButtonsInteractable(false);

        if (bindingLabels != null && index < bindingLabels.Length && bindingLabels[index] != null)
            bindingLabels[index].text = "... press input ...";

        if (detectStatusText != null)
            detectStatusText.text = $"Remapping: {InputBindingData.BindingLabels[index]}";

        // Wait frames so the mouse click doesn't register
        yield return null;
        yield return null;

        bool captured = false;
        while (!captured)
        {
            // Check joystick buttons
            for (int btn = 0; btn < 20; btn++)
            {
                if (Input.GetKeyDown(KeyCode.JoystickButton0 + btn))
                {
                    InputBinding binding = GetBindingByIndex(index);
                    binding.type = InputBinding.BindingType.JoystickButton;
                    binding.joystickButtonIndex = btn;
                    binding.axisName = "";
                    captured = true;
                    break;
                }
            }

            // Check axes (significant change)
            if (!captured)
            {
                string[] axisNames = { "Horizontal", "Vertical", "Axis 3", "Axis 4", "Axis 5",
                                       "Axis 6", "Axis 7", "Axis 8", "Axis 9", "Axis 10",
                                       "RightTrigger", "LeftTrigger" };

                foreach (string axName in axisNames)
                {
                    try
                    {
                        float val = Input.GetAxisRaw(axName);
                        if (Mathf.Abs(val) > 0.7f)
                        {
                            InputBinding binding = GetBindingByIndex(index);
                            binding.type = InputBinding.BindingType.Axis;
                            binding.axisName = axName;
                            binding.axisThreshold = val > 0 ? 0.5f : -0.5f;
                            captured = true;
                            break;
                        }
                    }
                    catch { /* axis may not exist */ }
                }
            }

            // Check keyboard keys
            if (!captured)
            {
                foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (kc == KeyCode.Mouse0 || kc == KeyCode.Mouse1 || kc == KeyCode.Mouse2) continue;
                    if (kc >= KeyCode.JoystickButton0) break;

                    if (Input.GetKeyDown(kc))
                    {
                        InputBinding binding = GetBindingByIndex(index);
                        binding.type = InputBinding.BindingType.KeyboardKey;
                        binding.keyboardKey = kc;
                        binding.axisName = "";
                        captured = true;
                        break;
                    }
                }
            }

            yield return null;
        }

        currentRemapIndex = -1;
        RefreshAllLabels();
        SetBindingButtonsInteractable(true);

        if (detectStatusText != null)
            detectStatusText.text = "Input captured!";

        remapCoroutine = null;
    }

    // ??? Helpers ???????????????????????????????????????????????

    InputBinding GetBindingByIndex(int index)
    {
        InputBinding[] all = editData.GetAllBindings();
        return all[index];
    }

    void RefreshAllLabels()
    {
        if (deviceNameText != null)
            deviceNameText.text = editData.deviceName;

        InputBinding[] all = editData.GetAllBindings();
        for (int i = 0; i < all.Length && i < bindingLabels.Length; i++)
        {
            if (bindingLabels[i] == null) continue;
            bindingLabels[i].text = FormatBinding(all[i]);
        }
    }

    string FormatBinding(InputBinding binding)
    {
        switch (binding.type)
        {
            case InputBinding.BindingType.JoystickButton:
                return $"Button {binding.joystickButtonIndex}";
            case InputBinding.BindingType.Axis:
                string dir = binding.axisThreshold >= 0 ? "+" : "-";
                return $"{binding.axisName} ({dir})";
            case InputBinding.BindingType.KeyboardKey:
                return binding.keyboardKey.ToString();
            default:
                return "None";
        }
    }

    void SetBindingButtonsInteractable(bool interactable)
    {
        foreach (Button btn in bindingButtons)
        {
            if (btn != null) btn.interactable = interactable;
        }
        if (detectDeviceButton != null) detectDeviceButton.interactable = interactable;
    }

    void SaveAndBack()
    {
        if (InputBindingManager.Instance != null)
        {
            // Copy edited data into the live singleton
            string json = JsonUtility.ToJson(editData);
            InputBindingManager.Instance.Bindings = JsonUtility.FromJson<InputBindingData>(json);
            InputBindingManager.Instance.SaveBindings();
        }
        GoBack();
    }

    void ResetDefaults()
    {
        editData = new InputBindingData();
        RefreshAllLabels();
        if (detectStatusText != null)
            detectStatusText.text = "Reset to defaults.";
    }

    void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
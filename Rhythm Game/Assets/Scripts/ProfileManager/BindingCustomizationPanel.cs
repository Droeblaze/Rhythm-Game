using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BindingCustomizationPanel : MonoBehaviour
{
    [SerializeField] private GameObject profileSelectionPanel;

    [Header("Title (optional — auto-created if null)")]
    public TextMeshProUGUI profileTitleText;

    [Header("Binding Buttons (0=L3top 1=L3bot 2=L4top 3=L4bot 4=L5top 5=L5bot)")]
    [Tooltip("Assign 6 buttons in this order: Lane3Top, Lane3Bottom, Lane4Top, Lane4Bottom, Lane5Top, Lane5Bottom")]
    public Button[] bindingButtons = new Button[6];

    [Header("Action Buttons (optional — auto-created if null)")]
    public Button resetDefaultsButton;
    public Button backButton;

    [Header("Colors")]
    public Color panelBackground  = new Color(0.08f, 0.08f, 0.12f, 0.97f);
    public Color bindingNormal    = new Color(0.18f, 0.18f, 0.25f, 1f);
    public Color bindingListening = new Color(0.8f, 0.7f, 0.0f, 1f);
    public Color bindingSaved     = new Color(0.15f, 0.55f, 0.25f, 1f);
    public Color resetButtonColor = new Color(0.55f, 0.15f, 0.15f, 1f);
    public Color backButtonColor  = new Color(0.18f, 0.18f, 0.18f, 1f);

    private static readonly string[] LaneNames =
    {
        "lane3", "lane3", "lane4", "lane4", "lane5", "lane5"
    };

    private static readonly string[] Sides =
    {
        "top", "bottom", "top", "bottom", "top", "bottom"
    };

    private static readonly string[] RowLabels =
    {
        "Lane 3  Top", "Lane 3  Bottom",
        "Lane 4  Top", "Lane 4  Bottom",
        "Lane 5  Top", "Lane 5  Bottom"
    };

    private ControlProfile currentProfile;
    private FirstRunSetupUI ownerUI;
    private int listeningSlot = -1;
    private bool isListening = false;
    private bool layoutReady = false;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Open(ControlProfile profile, FirstRunSetupUI owner)
    {
        currentProfile = profile;
        ownerUI = owner;
        isListening = false;
        listeningSlot = -1;

        if (!layoutReady)
        {
            BuildLayout();
            layoutReady = true;
        }

        UpdateTitle();
        WireActionButtons();
        RefreshAllLabels();
    }

    void BuildLayout()
    {
        var bg = GetComponent<Image>();
        if (bg != null)
            bg.color = panelBackground;

        var vlg = GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
            vlg = gameObject.AddComponent<VerticalLayoutGroup>();

        vlg.padding = new RectOffset(50, 50, 25, 25);
        vlg.spacing = 10f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        if (profileTitleText != null)
        {
            profileTitleText.alignment = TextAlignmentOptions.Center;
            profileTitleText.fontSize = 26f;
            profileTitleText.color = Color.white;
            AddLayoutElement(profileTitleText.gameObject, 55f);
        }

        foreach (var btn in bindingButtons)
        {
            if (btn == null) continue;

            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = bindingNormal;

            var cb = btn.colors;
            cb.normalColor = bindingNormal;
            cb.highlightedColor = Color.Lerp(bindingNormal, Color.white, 0.2f);
            cb.pressedColor = Color.Lerp(bindingNormal, Color.black, 0.2f);
            cb.selectedColor = bindingNormal;
            btn.colors = cb;

            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 18f;
                tmp.color = Color.white;
            }

            AddLayoutElement(btn.gameObject, 52f);
        }

        if (resetDefaultsButton == null)
            resetDefaultsButton = CreateButton("Reset to Defaults", resetButtonColor, 52f);

        if (backButton == null)
    backButton = CreateButton("Back", backButtonColor, 52f);

if (backButton != null)
{
    var img = backButton.GetComponent<Image>();
    if (img != null)
        img.color = backButtonColor;

    var cb = backButton.colors;
    cb.normalColor = backButtonColor;
    cb.highlightedColor = new Color(
        Mathf.Clamp01(backButtonColor.r + 0.08f),
        Mathf.Clamp01(backButtonColor.g + 0.08f),
        Mathf.Clamp01(backButtonColor.b + 0.08f),
        1f
    );
    cb.pressedColor = new Color(
        Mathf.Clamp01(backButtonColor.r - 0.08f),
        Mathf.Clamp01(backButtonColor.g - 0.08f),
        Mathf.Clamp01(backButtonColor.b - 0.08f),
        1f
    );
    cb.selectedColor = backButtonColor;
    backButton.colors = cb;

    var tmp = backButton.GetComponentInChildren<TextMeshProUGUI>();
    if (tmp != null)
    {
        tmp.text = "← Back";
        tmp.fontSize = 22f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    AddLayoutElement(backButton.gameObject, 52f);
}
        if (backButton == null)
    backButton = CreateButton("Back", backButtonColor, 52f);

if (backButton != null)
{
    var img = backButton.GetComponent<Image>();
    if (img != null)
        img.color = backButtonColor;

    var cb = backButton.colors;
    cb.normalColor = backButtonColor;
    cb.highlightedColor = new Color(
        Mathf.Clamp01(backButtonColor.r + 0.08f),
        Mathf.Clamp01(backButtonColor.g + 0.08f),
        Mathf.Clamp01(backButtonColor.b + 0.08f),
        1f
    );
    cb.pressedColor = new Color(
        Mathf.Clamp01(backButtonColor.r - 0.08f),
        Mathf.Clamp01(backButtonColor.g - 0.08f),
        Mathf.Clamp01(backButtonColor.b - 0.08f),
        1f
    );
    cb.selectedColor = backButtonColor;
    backButton.colors = cb;

    var tmp = backButton.GetComponentInChildren<TextMeshProUGUI>();
    if (tmp != null)
    {
        tmp.text = "← Back";
        tmp.fontSize = 22f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    AddLayoutElement(backButton.gameObject, 52f);
}
    }

    void UpdateTitle()
    {
        if (profileTitleText == null) return;

        profileTitleText.text = currentProfile switch
        {
            ControlProfile.FightStick => "FIGHT STICK  —  Customize Bindings",
            ControlProfile.Gamepad => "GAMEPAD  —  Customize Bindings",
            ControlProfile.Keyboard => "KEYBOARD  —  Customize Bindings",
            _ => "Customize Bindings"
        };
    }

    void WireActionButtons()
    {
        if (resetDefaultsButton != null)
        {
            resetDefaultsButton.onClick.RemoveAllListeners();
            resetDefaultsButton.onClick.AddListener(OnResetDefaults);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBack);
        }

        for (int i = 0; i < bindingButtons.Length; i++)
        {
            if (bindingButtons[i] == null) continue;
            int slot = i;
            bindingButtons[i].onClick.RemoveAllListeners();
            bindingButtons[i].onClick.AddListener(() => OnBindingClicked(slot));
        }
    }

    void RefreshAllLabels()
    {
        InputProfileData data = DefaultMappings.Get(currentProfile);
        CustomBindingsStore.ApplyOverrides(currentProfile, data);

        UpdateSlotLabel(0, RowLabels[0], GetValueLabel(data.lane3, true));
        UpdateSlotLabel(1, RowLabels[1], GetValueLabel(data.lane3, false));
        UpdateSlotLabel(2, RowLabels[2], GetValueLabel(data.lane4, true));
        UpdateSlotLabel(3, RowLabels[3], GetValueLabel(data.lane4, false));
        UpdateSlotLabel(4, RowLabels[4], GetValueLabel(data.lane5, true));
        UpdateSlotLabel(5, RowLabels[5], GetValueLabel(data.lane5, false));

        for (int i = 0; i < bindingButtons.Length; i++)
            SetSlotColor(i, bindingNormal);
    }

    string GetValueLabel(LaneButtonConfig lane, bool isTop)
    {
        if (isTop)
        {
            if (lane.topKey != KeyCode.None) return lane.topKey.ToString();
            if (lane.topIsButton) return $"Button {lane.topButton}";
            if (!string.IsNullOrEmpty(lane.topAxis)) return lane.topAxis;
            return "—";
        }

        if (lane.bottomKey != KeyCode.None) return lane.bottomKey.ToString();
        if (lane.bottomIsButton) return $"Button {lane.bottomButton}";
        if (!string.IsNullOrEmpty(lane.bottomAxis)) return lane.bottomAxis;
        return "—";
    }

    void UpdateSlotLabel(int slot, string rowLabel, string valueLabel)
    {
        if (slot >= bindingButtons.Length || bindingButtons[slot] == null) return;

        var tmp = bindingButtons[slot].GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = $"{rowLabel}     <b>{valueLabel}</b>";
    }

    void OnBindingClicked(int slot)
    {
        if (isListening)
            CancelListening();

        listeningSlot = slot;
        isListening = true;

        SetSlotColor(slot, bindingListening);
        SetSlotRawLabel(slot, $"{RowLabels[slot]}     <i>Press a key...  (Esc = cancel)</i>");
    }

    void CancelListening()
    {
        isListening = false;
        listeningSlot = -1;
        RefreshAllLabels();
    }

    void Update()
    {
        if (!isListening) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelListening();
            return;
        }

        if (currentProfile == ControlProfile.Keyboard)
            ListenKeyboard();
        else
            ListenJoystick();
    }

    void ListenKeyboard()
    {
        if (!Input.anyKeyDown) return;

        foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
        {
            int code = (int)kc;
            if (code >= 330) continue;
            if (kc == KeyCode.Escape) continue;
            if (kc == KeyCode.None) continue;

            if (Input.GetKeyDown(kc))
            {
                ConfirmKeyboard(kc);
                return;
            }
        }
    }

    void ListenJoystick()
    {
        for (int i = 0; i <= 19; i++)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0 + i))
            {
                ConfirmJoystick(i);
                return;
            }
        }
    }

    void ConfirmKeyboard(KeyCode key)
    {
        int slot = listeningSlot;
        isListening = false;
        listeningSlot = -1;

        if (Sides[slot] == "top")
        {
            CustomBindingsStore.SaveLaneTop(
                currentProfile, LaneNames[slot],
                isButton: false, button: 0, axis: "", key: key
            );
        }
        else
        {
            CustomBindingsStore.SaveLaneBottom(
                currentProfile, LaneNames[slot],
                isButton: false, button: 0, axis: "", key: key
            );
        }

        SetSlotColor(slot, bindingSaved);
        UpdateSlotLabel(slot, RowLabels[slot], key.ToString());
        StartCoroutine(ResetSlotColor(slot, 0.6f));

        Debug.Log($"[BindingPanel] {LaneNames[slot]} {Sides[slot]} = {key}");
    }

    void ConfirmJoystick(int buttonNum)
    {
        int slot = listeningSlot;
        isListening = false;
        listeningSlot = -1;

        if (Sides[slot] == "top")
        {
            CustomBindingsStore.SaveLaneTop(
                currentProfile, LaneNames[slot],
                isButton: true, button: buttonNum, axis: "", key: KeyCode.None
            );
        }
        else
        {
            CustomBindingsStore.SaveLaneBottom(
                currentProfile, LaneNames[slot],
                isButton: true, button: buttonNum, axis: "", key: KeyCode.None
            );
        }

        SetSlotColor(slot, bindingSaved);
        UpdateSlotLabel(slot, RowLabels[slot], $"Button {buttonNum}");
        StartCoroutine(ResetSlotColor(slot, 0.6f));

        Debug.Log($"[BindingPanel] {LaneNames[slot]} {Sides[slot]} = Button {buttonNum}");
    }

    System.Collections.IEnumerator ResetSlotColor(int slot, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetSlotColor(slot, bindingNormal);
    }

    void OnResetDefaults()
    {
        if (isListening)
            CancelListening();

        CustomBindingsStore.ClearOverrides(currentProfile);
        RefreshAllLabels();
        Debug.Log($"[BindingPanel] Reset to defaults: {currentProfile}");
    }

    void OnBack()
    {
        if (isListening)
            CancelListening();

        gameObject.SetActive(false);

        if (ownerUI != null)
            ownerUI.ShowProfileSelection();
        else if (profileSelectionPanel != null)
            profileSelectionPanel.SetActive(true);
    }

    void SetSlotColor(int slot, Color color)
    {
        if (slot >= bindingButtons.Length || bindingButtons[slot] == null) return;

        var img = bindingButtons[slot].GetComponent<Image>();
        if (img != null)
            img.color = color;

        var cb = bindingButtons[slot].colors;
        cb.normalColor = color;
        bindingButtons[slot].colors = cb;
    }

    void SetSlotRawLabel(int slot, string text)
    {
        if (slot >= bindingButtons.Length || bindingButtons[slot] == null) return;

        var tmp = bindingButtons[slot].GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;
    }

    void AddLayoutElement(GameObject go, float preferredHeight)
    {
        var le = go.GetComponent<LayoutElement>();
        if (le == null)
            le = go.AddComponent<LayoutElement>();

        le.preferredHeight = preferredHeight;
        le.flexibleWidth = 1f;
        le.minHeight = preferredHeight * 0.8f;
    }

    Button CreateButton(string label, Color color, float height)
    {
        var go = new GameObject(label.Replace(" ", "") + "_Auto");
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);

        var img = go.AddComponent<Image>();
        img.color = color;

        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor = color;
        cb.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
        cb.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        btn.colors = cb;

        AddLayoutElement(go, height);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 19f;
        tmp.color = Color.white;

        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return btn;
    }
}
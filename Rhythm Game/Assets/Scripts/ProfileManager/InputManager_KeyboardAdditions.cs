// InputManager_KeyboardAdditions.cs
// ─────────────────────────────────────────────────────────────────────────────
// THIS IS NOT A REPLACEMENT FILE.
// Add these fields and method changes to your existing InputManager.cs.
// Changes are minimal — nothing in the note-hit or judgement logic is touched.
//
// WHAT CHANGES AND WHY:
//   InputManager uses Input.GetKeyDown(KeyCode.JoystickButton0 + buttonNum).
//   Keyboard KeyCodes (e.g. KeyCode.J = 106) cannot be expressed as an offset
//   from JoystickButton0 (= 330) without going negative. So we add optional
//   KeyCode override fields per lane button. When a key override is set
//   (anything other than KeyCode.None), it takes full priority over the
//   joystick button path. Gamepad and FightStick profiles leave all overrides
//   at KeyCode.None so existing behavior is 100% unchanged.
//
// ─────────────────────────────────────────────────────────────────────────────
// STEP 1 — Add this Header block to InputManager.cs after your existing [Header] blocks:
// ─────────────────────────────────────────────────────────────────────────────

/*
    [Header("Keyboard Key Overrides (set by ControlProfileApplicator — do not edit manually)")]
    public KeyCode lane3TopKey    = KeyCode.None;
    public KeyCode lane3BottomKey = KeyCode.None;
    public KeyCode lane4TopKey    = KeyCode.None;
    public KeyCode lane4BottomKey = KeyCode.None;
    public KeyCode lane5TopKey    = KeyCode.None;
    public KeyCode lane5BottomKey = KeyCode.None;
*/

// ─────────────────────────────────────────────────────────────────────────────
// STEP 2 — Replace CheckButtonInputs() in InputManager.cs with this version:
// ─────────────────────────────────────────────────────────────────────────────

/*
    void CheckButtonInputs()
    {
        CheckLaneButtons(3, lane3TopIsButton, lane3TopButton, lane3TopAxis, lane3TopKey,
                            lane3BottomIsButton, lane3BottomButton, lane3BottomAxis, lane3BottomKey);

        CheckLaneButtons(4, lane4TopIsButton, lane4TopButton, lane4TopAxis, lane4TopKey,
                            lane4BottomIsButton, lane4BottomButton, lane4BottomAxis, lane4BottomKey);

        CheckLaneButtons(5, lane5TopIsButton, lane5TopButton, lane5TopAxis, lane5TopKey,
                            lane5BottomIsButton, lane5BottomButton, lane5BottomAxis, lane5BottomKey);
    }
*/

// ─────────────────────────────────────────────────────────────────────────────
// STEP 3 — Replace the CheckLaneButtons signature with this version:
// ─────────────────────────────────────────────────────────────────────────────

/*
    void CheckLaneButtons(int lane,
                          bool topIsButton, int topBtn, string topAxis, KeyCode topKey,
                          bool bottomIsButton, int bottomBtn, string bottomAxis, KeyCode bottomKey)
    {
        bool topPressed  = GetInputDown(topIsButton,    topBtn,    topAxis,    topKey);
        bool bottomPressed = GetInputDown(bottomIsButton, bottomBtn, bottomAxis, bottomKey);
        bool topHeld     = GetInputHeld(topIsButton,    topBtn,    topAxis,    topKey);
        bool bottomHeld  = GetInputHeld(bottomIsButton, bottomBtn, bottomAxis, bottomKey);
        bool bothHeld    = topHeld && bottomHeld;

        if (bothHeld && (topPressed || bottomPressed))
        {
            CheckButtonNoteHit(lane, ButtonRow.Both);
        }
        else if (topPressed && !bothHeld)
        {
            CheckButtonNoteHit(lane, ButtonRow.Top);
        }
        else if (bottomPressed && !bothHeld)
        {
            CheckButtonNoteHit(lane, ButtonRow.Bottom);
        }
    }
*/

// ─────────────────────────────────────────────────────────────────────────────
// STEP 4 — Replace GetInputDown() and GetInputHeld() with these versions:
// ─────────────────────────────────────────────────────────────────────────────

/*
    bool GetInputDown(bool isButton, int buttonNum, string axisName, KeyCode keyOverride = KeyCode.None)
    {
        // Keyboard override takes full priority — used when profile = Keyboard
        if (keyOverride != KeyCode.None)
            return Input.GetKeyDown(keyOverride);

        if (isButton)
        {
            return Input.GetKeyDown(KeyCode.JoystickButton0 + buttonNum);
        }
        else
        {
            if (string.IsNullOrEmpty(axisName)) return false;

            float axisValue       = Input.GetAxis(axisName);
            bool  currentlyPressed = axisValue > triggerThreshold;
            bool  wasPressed      = previousTriggerStates.ContainsKey(axisName) && previousTriggerStates[axisName];

            previousTriggerStates[axisName] = currentlyPressed;

            return currentlyPressed && !wasPressed;
        }
    }

    bool GetInputHeld(bool isButton, int buttonNum, string axisName, KeyCode keyOverride = KeyCode.None)
    {
        // Keyboard override takes full priority
        if (keyOverride != KeyCode.None)
            return Input.GetKey(keyOverride);

        if (isButton)
        {
            return Input.GetKey(KeyCode.JoystickButton0 + buttonNum);
        }
        else
        {
            if (string.IsNullOrEmpty(axisName)) return false;
            return Input.GetAxis(axisName) > triggerThreshold;
        }
    }
*/

// ─────────────────────────────────────────────────────────────────────────────
// SUMMARY OF ALL CHANGES TO InputManager.cs:
//   + 6 new KeyCode fields (lane3-5 top/bottom)        → new [Header] block
//   ~ CheckButtonInputs()  — passes key fields through  → minor signature update
//   ~ CheckLaneButtons()   — accepts 2 extra KeyCode params per call
//   ~ GetInputDown()       — 1 extra optional param, 1 early-return at top
//   ~ GetInputHeld()       — same as GetInputDown
//
// Zero changes to: Start(), Update(), CheckStickInputs(), DetectStickDirection(),
// CheckNoteHit(), CheckButtonNoteHit(), HitNote(), OnNoteMissed(),
// RegisterNote(), UnregisterNote(), all timing windows, all judgement logic.
// ─────────────────────────────────────────────────────────────────────────────

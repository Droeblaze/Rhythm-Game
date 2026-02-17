using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public NoteSpawner noteSpawner;
    public Transform judgementLine;
    public JudgementDisplay judgementDisplay;

    [Header("Timing Windows (Buttons - Lanes 3-5)")]
    public float perfectWindow = 0.05f;   // ±50ms for perfect
    public float greatWindow = 0.1f;      // ±100ms for great
    public float goodWindow = 0.15f;      // ±150ms for good
    public float missWindow = 0.2f;       // ±200ms, beyond this is a miss

    [Header("Timing Windows (Stick - Lanes 0-2)")]
    public float stickPerfectWindow = 0.08f;   // ±80ms for perfect
    public float stickGreatWindow = 0.15f;     // ±150ms for great
    public float stickGoodWindow = 0.2f;       // ±200ms for good
    public float stickMissWindow = 0.3f;       // ±300ms, beyond this is a miss

    [Header("Stick Settings")]
    public float stickDeadzone = 0.5f; // Threshold for stick input

    [Header("Face Buttons (Lane 3)")]
    public bool lane3TopIsButton = true;
    public int lane3TopButton = 8;
    public string lane3TopAxis = "";
    public bool lane3BottomIsButton = true;
    public int lane3BottomButton = 9;
    public string lane3BottomAxis = "";

    [Header("Face Buttons (Lane 4)")]
    public bool lane4TopIsButton = true;
    public int lane4TopButton = 10;
    public string lane4TopAxis = "";
    public bool lane4BottomIsButton = true;
    public int lane4BottomButton = 11;
    public string lane4BottomAxis = "";

    [Header("Face Buttons (Lane 5)")]
    public bool lane5TopIsButton = true;
    public int lane5TopButton = 12;
    public string lane5TopAxis = "";
    public bool lane5BottomIsButton = false;
    public int lane5BottomButton = 13;
    public string lane5BottomAxis = "RightTrigger";

    [Header("Trigger Settings")]
    public float triggerThreshold = 0.5f;

    private Dictionary<int, List<NoteVisual>> notesInLane = new Dictionary<int, List<NoteVisual>>();
    private Dictionary<string, bool> previousTriggerStates = new Dictionary<string, bool>();

    // Previous stick direction for detecting direction changes
    private int previousHorizDir = 0;
    private int previousVertDir = 0;

    void Start()
    {
        // Initialize lane lists
        for (int i = 0; i < 6; i++)
        {
            notesInLane[i] = new List<NoteVisual>();
        }
    }

    void Update()
    {
        CheckStickInputs();
        CheckButtonInputs();
    }

    void CheckStickInputs()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Quantize to -1, 0, or 1 based on deadzone
        int horizDir = horizontal < -stickDeadzone ? -1 : (horizontal > stickDeadzone ? 1 : 0);
        int vertDir = vertical < -stickDeadzone ? -1 : (vertical > stickDeadzone ? 1 : 0);

        // Fire input whenever the quantized direction changes (not just from neutral)
        bool directionChanged = (horizDir != previousHorizDir) || (vertDir != previousVertDir);

        if (directionChanged && (horizDir != 0 || vertDir != 0))
        {
            DetectStickDirection(horizDir, vertDir);
        }

        // Update state for next frame
        previousHorizDir = horizDir;
        previousVertDir = vertDir;
    }

    void DetectStickDirection(int horizDir, int vertDir)
    {
        // Lane 0 - Left stick (horizontal = -1)
        if (horizDir == -1)
        {
            if (vertDir == 1)
                CheckNoteHit(0, StickDirection.Up);        // Up-left
            else if (vertDir == 0)
                CheckNoteHit(0, StickDirection.Horizontal); // Left
            else if (vertDir == -1)
                CheckNoteHit(0, StickDirection.Down);      // Down-left
        }

        // Lane 1 - Vertical stick (horizontal = 0)
        else if (horizDir == 0)
        {
            if (vertDir == 1)
                CheckNoteHit(1, StickDirection.Up);        // Up
            else if (vertDir == -1)
                CheckNoteHit(1, StickDirection.Down);      // Down
        }

        // Lane 2 - Right stick (horizontal = 1)
        else if (horizDir == 1)
        {
            if (vertDir == 1)
                CheckNoteHit(2, StickDirection.Up);        // Up-right
            else if (vertDir == 0)
                CheckNoteHit(2, StickDirection.Horizontal); // Right
            else if (vertDir == -1)
                CheckNoteHit(2, StickDirection.Down);      // Down-right
        }
    }

    void CheckButtonInputs()
    {
        // Lane 3 buttons
        CheckLaneButtons(3, lane3TopIsButton, lane3TopButton, lane3TopAxis,
                           lane3BottomIsButton, lane3BottomButton, lane3BottomAxis);

        // Lane 4 buttons
        CheckLaneButtons(4, lane4TopIsButton, lane4TopButton, lane4TopAxis,
                           lane4BottomIsButton, lane4BottomButton, lane4BottomAxis);

        // Lane 5 buttons
        CheckLaneButtons(5, lane5TopIsButton, lane5TopButton, lane5TopAxis,
                           lane5BottomIsButton, lane5BottomButton, lane5BottomAxis);
    }

    void CheckLaneButtons(int lane, bool topIsButton, int topBtn, string topAxis,
                                     bool bottomIsButton, int bottomBtn, string bottomAxis)
    {
        bool topPressed = GetInputDown(topIsButton, topBtn, topAxis);
        bool bottomPressed = GetInputDown(bottomIsButton, bottomBtn, bottomAxis);
        bool topHeld = GetInputHeld(topIsButton, topBtn, topAxis);
        bool bottomHeld = GetInputHeld(bottomIsButton, bottomBtn, bottomAxis);
        bool bothHeld = topHeld && bottomHeld;

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

    bool GetInputDown(bool isButton, int buttonNum, string axisName)
    {
        if (isButton)
        {
            return Input.GetKeyDown(KeyCode.JoystickButton0 + buttonNum);
        }
        else
        {
            if (string.IsNullOrEmpty(axisName)) return false;

            float axisValue = Input.GetAxis(axisName);
            bool currentlyPressed = axisValue > triggerThreshold;
            bool wasPressed = previousTriggerStates.ContainsKey(axisName) && previousTriggerStates[axisName];

            previousTriggerStates[axisName] = currentlyPressed;

            return currentlyPressed && !wasPressed;
        }
    }

    bool GetInputHeld(bool isButton, int buttonNum, string axisName)
    {
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

    void CheckNoteHit(int lane, StickDirection direction)
    {
        if (!notesInLane.ContainsKey(lane)) return;

        NoteVisual closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (NoteVisual note in notesInLane[lane])
        {
            if (note == null || note.data.stickDirection != direction) continue;

            float distance = Mathf.Abs(note.transform.position.y - judgementLine.position.y);

            if (distance < closestDistance && distance <= stickMissWindow * 5f)
            {
                closestDistance = distance;
                closestNote = note;
            }
        }

        if (closestNote != null)
        {
            HitNote(closestNote, closestDistance, true);
        }
    }

    void CheckButtonNoteHit(int lane, ButtonRow row)
    {
        if (!notesInLane.ContainsKey(lane)) return;

        NoteVisual closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (NoteVisual note in notesInLane[lane])
        {
            if (note == null || note.data.buttonRow != row) continue;

            float distance = Mathf.Abs(note.transform.position.y - judgementLine.position.y);

            if (distance < closestDistance && distance <= missWindow * 5f)
            {
                closestDistance = distance;
                closestNote = note;
            }
        }

        if (closestNote != null)
        {
            HitNote(closestNote, closestDistance, false);
        }
    }

    void HitNote(NoteVisual note, float distance, bool isStickLane)
    {
        float timing = distance / noteSpawner.scrollSpeed;

        float pWindow = isStickLane ? stickPerfectWindow : perfectWindow;
        float grWindow = isStickLane ? stickGreatWindow : greatWindow;
        float goWindow = isStickLane ? stickGoodWindow : goodWindow;
        float mWindow = isStickLane ? stickMissWindow : missWindow;

        string judgement = "Miss";
        if (timing <= pWindow)
            judgement = "Perfect!";
        else if (timing <= grWindow)
            judgement = "Great!";
        else if (timing <= goWindow)
            judgement = "Good";
        else if (timing <= mWindow)
            judgement = "OK";

        Debug.Log($"Lane {note.data.laneIndex} Hit! Judgement: {judgement} (timing: {timing:F3}s)");

        // Display judgement on screen
        if (judgementDisplay != null)
        {
            judgementDisplay.ShowJudgement(judgement);
        }

        notesInLane[note.data.laneIndex].Remove(note);
        note.MarkAsHit();
    }

    // NEW METHOD: Called when a note is missed (passes judgement line)
    public void OnNoteMissed(NoteVisual note)
    {
        Debug.Log($"Lane {note.data.laneIndex} Missed!");

        // Display miss judgement
        if (judgementDisplay != null)
        {
            judgementDisplay.ShowJudgement("Miss");
        }

        // Remove from tracking
        if (notesInLane.ContainsKey(note.data.laneIndex))
        {
            notesInLane[note.data.laneIndex].Remove(note);
        }
    }

    public void RegisterNote(NoteVisual note)
    {
        if (!notesInLane.ContainsKey(note.data.laneIndex))
            notesInLane[note.data.laneIndex] = new List<NoteVisual>();

        notesInLane[note.data.laneIndex].Add(note);
    }

    public void UnregisterNote(NoteVisual note)
    {
        if (notesInLane.ContainsKey(note.data.laneIndex))
            notesInLane[note.data.laneIndex].Remove(note);
    }
}
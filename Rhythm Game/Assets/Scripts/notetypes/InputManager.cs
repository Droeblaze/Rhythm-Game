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

    [Header("Hold Note Settings")]
    [Tooltip("Percentage of the hold that must be sustained for each judgement tier (0-1)")]
    public float holdPerfectThreshold = 0.95f;
    public float holdGreatThreshold = 0.80f;
    public float holdGoodThreshold = 0.60f;

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

    // Active hold notes being sustained, keyed by lane index
    private Dictionary<int, NoteVisual> activeHoldNotes = new Dictionary<int, NoteVisual>();

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
        UpdateHoldNotes();
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
            // Skip notes that are already being held
            if (note.isHoldActive) continue;

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
            // Skip notes that are already being held
            if (note.isHoldActive) continue;

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

        // For hold notes, start the hold phase instead of removing immediately
        if (note.data.noteType == NoteType.Hold && judgement != "Miss")
        {
            Debug.Log($"Lane {note.data.laneIndex} Hold Started! Initial: {judgement} (timing: {timing:F3}s)");

            if (judgementDisplay != null)
                judgementDisplay.ShowJudgement(judgement);

            note.MarkAsHit(); // Sets isHoldActive = true
            activeHoldNotes[note.data.laneIndex] = note;

            // Remove from normal hit detection but keep in notesInLane for miss tracking
            return;
        }

        Debug.Log($"Lane {note.data.laneIndex} Hit! Judgement: {judgement} (timing: {timing:F3}s)");

        // Display judgement on screen
        if (judgementDisplay != null)
        {
            judgementDisplay.ShowJudgement(judgement);
        }

        notesInLane[note.data.laneIndex].Remove(note);
        note.MarkAsHit();
    }

    /// <summary>
    /// Each frame, tick down active hold notes and check if the input is still held.
    /// </summary>
    void UpdateHoldNotes()
    {
        // Iterate over a copy of keys to allow removal during iteration
        List<int> lanes = new List<int>(activeHoldNotes.Keys);

        foreach (int lane in lanes)
        {
            NoteVisual note = activeHoldNotes[lane];

            if (note == null)
            {
                activeHoldNotes.Remove(lane);
                continue;
            }

            // Check if the corresponding input is still being held
            bool stillHeld = IsInputStillHeld(note);

            if (stillHeld)
            {
                // Tick down the remaining hold time
                note.holdTimeRemaining -= Time.deltaTime;

                if (note.holdTimeRemaining <= 0f)
                {
                    // Hold completed successfully!
                    CompleteHold(note, lane);
                }
            }
            else
            {
                // Player released early — evaluate how much was held
                FailHold(note, lane);
            }
        }
    }

    /// <summary>
    /// Returns true if the player is still holding the correct input for this note.
    /// </summary>
    bool IsInputStillHeld(NoteVisual note)
    {
        int lane = note.data.laneIndex;

        if (lane <= 2)
        {
            // Stick lanes: check if the stick is still pointing in the required direction
            return IsStickHeld(lane, note.data.stickDirection);
        }
        else
        {
            // Button lanes: check if the required button(s) are still held
            return IsButtonHeld(lane, note.data.buttonRow);
        }
    }

    bool IsStickHeld(int lane, StickDirection direction)
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        int horizDir = horizontal < -stickDeadzone ? -1 : (horizontal > stickDeadzone ? 1 : 0);
        int vertDir = vertical < -stickDeadzone ? -1 : (vertical > stickDeadzone ? 1 : 0);

        // Check that the stick is in the correct lane quadrant
        int requiredHoriz = lane == 0 ? -1 : (lane == 2 ? 1 : 0);

        if (horizDir != requiredHoriz) return false;

        // Check vertical direction matches
        switch (direction)
        {
            case StickDirection.Up:
                return vertDir == 1;
            case StickDirection.Down:
                return vertDir == -1;
            case StickDirection.Horizontal:
                return vertDir == 0;
            case StickDirection.UpDown:
                return vertDir == 1 || vertDir == -1;
            default:
                return false;
        }
    }

    bool IsButtonHeld(int lane, ButtonRow row)
    {
        bool topIsBtn, bottomIsBtn;
        int topBtn, bottomBtn;
        string topAxis, bottomAxis;

        GetLaneButtonConfig(lane, out topIsBtn, out topBtn, out topAxis,
                                  out bottomIsBtn, out bottomBtn, out bottomAxis);

        bool topHeld = GetInputHeld(topIsBtn, topBtn, topAxis);
        bool bottomHeld = GetInputHeld(bottomIsBtn, bottomBtn, bottomAxis);

        switch (row)
        {
            case ButtonRow.Top:
                return topHeld;
            case ButtonRow.Bottom:
                return bottomHeld;
            case ButtonRow.Both:
                return topHeld && bottomHeld;
            default:
                return false;
        }
    }

    void GetLaneButtonConfig(int lane, out bool topIsBtn, out int topBtn, out string topAxis,
                                       out bool bottomIsBtn, out int bottomBtn, out string bottomAxis)
    {
        switch (lane)
        {
            case 3:
                topIsBtn = lane3TopIsButton; topBtn = lane3TopButton; topAxis = lane3TopAxis;
                bottomIsBtn = lane3BottomIsButton; bottomBtn = lane3BottomButton; bottomAxis = lane3BottomAxis;
                break;
            case 4:
                topIsBtn = lane4TopIsButton; topBtn = lane4TopButton; topAxis = lane4TopAxis;
                bottomIsBtn = lane4BottomIsButton; bottomBtn = lane4BottomButton; bottomAxis = lane4BottomAxis;
                break;
            case 5:
                topIsBtn = lane5TopIsButton; topBtn = lane5TopButton; topAxis = lane5TopAxis;
                bottomIsBtn = lane5BottomIsButton; bottomBtn = lane5BottomButton; bottomAxis = lane5BottomAxis;
                break;
            default:
                topIsBtn = true; topBtn = 0; topAxis = "";
                bottomIsBtn = true; bottomBtn = 0; bottomAxis = "";
                break;
        }
    }

    void CompleteHold(NoteVisual note, int lane)
    {
        Debug.Log($"Lane {lane} Hold Complete! Perfect hold!");

        if (judgementDisplay != null)
            judgementDisplay.ShowJudgement("Perfect!");

        notesInLane[lane].Remove(note);
        activeHoldNotes.Remove(lane);
        note.MarkHoldComplete();
    }

    void FailHold(NoteVisual note, int lane)
    {
        // Calculate what percentage of the hold was sustained
        float heldRatio = 1f - (note.holdTimeRemaining / note.data.holdDuration);

        string judgement;
        if (heldRatio >= holdPerfectThreshold)
            judgement = "Perfect!";
        else if (heldRatio >= holdGreatThreshold)
            judgement = "Great!";
        else if (heldRatio >= holdGoodThreshold)
            judgement = "Good";
        else
            judgement = "Miss";

        Debug.Log($"Lane {lane} Hold Released! Held {heldRatio:P0} — {judgement}");

        if (judgementDisplay != null)
            judgementDisplay.ShowJudgement(judgement);

        notesInLane[lane].Remove(note);
        activeHoldNotes.Remove(lane);
        note.MarkHoldFailed();
    }

    // Called when a note is missed (passes judgement line)
    public void OnNoteMissed(NoteVisual note)
    {
        Debug.Log($"Lane {note.data.laneIndex} Missed!");

        // Display miss judgement
        if (judgementDisplay != null)
        {
            judgementDisplay.ShowJudgement("Miss");
        }

        // Clean up if this was an active hold (shouldn't normally happen, but safety check)
        if (activeHoldNotes.ContainsKey(note.data.laneIndex) && activeHoldNotes[note.data.laneIndex] == note)
        {
            activeHoldNotes.Remove(note.data.laneIndex);
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
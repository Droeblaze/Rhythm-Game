using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public NoteSpawner noteSpawner;
    public Transform judgementLine;
    public JudgementDisplay judgementDisplay;

    [Header("Timing Windows (Buttons - Lanes 3-5)")]
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.15f;
    public float missWindow = 0.2f;

    [Header("Timing Windows (Stick - Lanes 0-2)")]
    public float stickPerfectWindow = 0.08f;
    public float stickGreatWindow = 0.15f;
    public float stickGoodWindow = 0.2f;
    public float stickMissWindow = 0.3f;

    [Header("Hold Note Settings")]
    public float holdPerfectThreshold = 0.95f;
    public float holdGreatThreshold = 0.80f;
    public float holdGoodThreshold = 0.60f;

    [Header("Stick Settings")]
    public float stickDeadzone = 0.5f;

    private Dictionary<int, List<NoteVisual>> notesInLane = new Dictionary<int, List<NoteVisual>>();

    private Dictionary<int, NoteVisual> activeHoldNotes = new Dictionary<int, NoteVisual>();

    private int previousHorizDir = 0;
    private int previousVertDir = 0;

    // Edge-detection state for axis-based button bindings
    private Dictionary<string, bool> previousAxisStates = new Dictionary<string, bool>();

    void Start()
    {
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

    // ??? Binding helpers ???????????????????????????????????????

    InputBindingManager Mgr => InputBindingManager.Instance;

    bool GetBindingDown(InputBinding binding)
    {
        if (Mgr != null) return Mgr.GetBindingDown(binding);
        return false;
    }

    bool GetBindingHeld(InputBinding binding)
    {
        if (Mgr != null) return Mgr.GetBindingHeld(binding);
        return false;
    }

    // ??? Stick Inputs ??????????????????????????????????????????

    void CheckStickInputs()
    {
        int horizDir = Mgr != null ? Mgr.GetHorizontalDir(stickDeadzone) : 0;
        int vertDir = Mgr != null ? Mgr.GetVerticalDir(stickDeadzone) : 0;

        bool directionChanged = (horizDir != previousHorizDir) || (vertDir != previousVertDir);

        if (directionChanged && (horizDir != 0 || vertDir != 0))
        {
            DetectStickDirection(horizDir, vertDir);
        }

        previousHorizDir = horizDir;
        previousVertDir = vertDir;
    }

    void DetectStickDirection(int horizDir, int vertDir)
    {
        if (horizDir == -1)
        {
            if (vertDir == 1)
                CheckNoteHit(0, StickDirection.Up);
            else if (vertDir == 0)
                CheckNoteHit(0, StickDirection.Horizontal);
            else if (vertDir == -1)
                CheckNoteHit(0, StickDirection.Down);
        }
        else if (horizDir == 0)
        {
            if (vertDir == 1)
                CheckNoteHit(1, StickDirection.Up);
            else if (vertDir == -1)
                CheckNoteHit(1, StickDirection.Down);
        }
        else if (horizDir == 1)
        {
            if (vertDir == 1)
                CheckNoteHit(2, StickDirection.Up);
            else if (vertDir == 0)
                CheckNoteHit(2, StickDirection.Horizontal);
            else if (vertDir == -1)
                CheckNoteHit(2, StickDirection.Down);
        }
    }

    // ??? Button Inputs (read from InputBindingManager) ?????????

    void CheckButtonInputs()
    {
        if (Mgr == null) return;

        CheckLaneButtons(3);
        CheckLaneButtons(4);
        CheckLaneButtons(5);
    }

    void CheckLaneButtons(int lane)
    {
        InputBinding topBinding = Mgr.GetLaneTopBinding(lane);
        InputBinding bottomBinding = Mgr.GetLaneBottomBinding(lane);
        if (topBinding == null || bottomBinding == null) return;

        bool topPressed = GetBindingDown(topBinding);
        bool bottomPressed = GetBindingDown(bottomBinding);
        bool topHeld = GetBindingHeld(topBinding);
        bool bottomHeld = GetBindingHeld(bottomBinding);
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

    // ??? Note Hit Detection ????????????????????????????????????

    void CheckNoteHit(int lane, StickDirection direction)
    {
        if (!notesInLane.ContainsKey(lane)) return;

        NoteVisual closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (NoteVisual note in notesInLane[lane])
        {
            if (note == null || note.data.stickDirection != direction) continue;
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

        if (note.data.noteType == NoteType.Hold && judgement != "Miss")
        {
            Debug.Log($"Lane {note.data.laneIndex} Hold Started! Initial: {judgement} (timing: {timing:F3}s)");

            if (judgementDisplay != null)
                judgementDisplay.ShowJudgement(judgement);

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.RecordJudgement(judgement);

            note.MarkAsHit();
            activeHoldNotes[note.data.laneIndex] = note;
            return;
        }

        Debug.Log($"Lane {note.data.laneIndex} Hit! Judgement: {judgement} (timing: {timing:F3}s)");

        if (judgementDisplay != null)
        {
            judgementDisplay.ShowJudgement(judgement);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RecordJudgement(judgement);
        }

        notesInLane[note.data.laneIndex].Remove(note);
        note.MarkAsHit();
    }

    // ??? Hold Notes ????????????????????????????????????????????

    void UpdateHoldNotes()
    {
        List<int> lanes = new List<int>(activeHoldNotes.Keys);

        foreach (int lane in lanes)
        {
            NoteVisual note = activeHoldNotes[lane];

            if (note == null)
            {
                activeHoldNotes.Remove(lane);
                continue;
            }

            bool stillHeld = IsInputStillHeld(note);

            if (stillHeld)
            {
                note.holdTimeRemaining -= Time.deltaTime;

                if (note.holdTimeRemaining <= 0f)
                {
                    CompleteHold(note, lane);
                }
            }
            else
            {
                FailHold(note, lane);
            }
        }
    }

    bool IsInputStillHeld(NoteVisual note)
    {
        int lane = note.data.laneIndex;

        if (lane <= 2)
        {
            return IsStickHeld(lane, note.data.stickDirection);
        }
        else
        {
            return IsButtonHeld(lane, note.data.buttonRow);
        }
    }

    bool IsStickHeld(int lane, StickDirection direction)
    {
        int horizDir = Mgr != null ? Mgr.GetHorizontalDir(stickDeadzone) : 0;
        int vertDir = Mgr != null ? Mgr.GetVerticalDir(stickDeadzone) : 0;

        int requiredHoriz = lane == 0 ? -1 : (lane == 2 ? 1 : 0);

        if (horizDir != requiredHoriz) return false;

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
        if (Mgr == null) return false;

        InputBinding topBinding = Mgr.GetLaneTopBinding(lane);
        InputBinding bottomBinding = Mgr.GetLaneBottomBinding(lane);

        bool topHeld = topBinding != null && GetBindingHeld(topBinding);
        bool bottomHeld = bottomBinding != null && GetBindingHeld(bottomBinding);

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

    void CompleteHold(NoteVisual note, int lane)
    {
        Debug.Log($"Lane {lane} Hold Complete! Perfect hold!");

        if (judgementDisplay != null)
            judgementDisplay.ShowJudgement("Perfect!");

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.RecordJudgement("Perfect!");

        notesInLane[lane].Remove(note);
        activeHoldNotes.Remove(lane);
        note.MarkHoldComplete();
    }

    void FailHold(NoteVisual note, int lane)
    {
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

        Debug.Log($"Lane {lane} Hold Released! Held {heldRatio:P0} – {judgement}");

        if (judgementDisplay != null)
            judgementDisplay.ShowJudgement(judgement);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.RecordJudgement(judgement);

        notesInLane[lane].Remove(note);
        activeHoldNotes.Remove(lane);
        note.MarkHoldFailed();
    }

    public void OnNoteMissed(NoteVisual note)
    {
        Debug.Log($"Lane {note.data.laneIndex} Missed!");

        if (judgementDisplay != null)
        {
            judgementDisplay.ShowJudgement("Miss");
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RecordJudgement("Miss");
            if (note.data.noteType == NoteType.Hold)
            {
                ScoreManager.Instance.RecordJudgement("Miss");
            }
        }

        if (activeHoldNotes.ContainsKey(note.data.laneIndex) && activeHoldNotes[note.data.laneIndex] == note)
        {
            activeHoldNotes.Remove(note.data.laneIndex);
        }

        if (notesInLane.ContainsKey(note.data.laneIndex))
        {
            notesInLane[note.data.laneIndex].Remove(note);
        }
    }

    public void RegisterNote(NoteVisual note)
    {
        if (!notesInLane.ContainsKey(note.data.laneIndex))
        {
            notesInLane[note.data.laneIndex] = new List<NoteVisual>();
        }

        notesInLane[note.data.laneIndex].Add(note);
    }

    public void UnregisterNote(NoteVisual note)
    {
        if (notesInLane.ContainsKey(note.data.laneIndex))
        {
            notesInLane[note.data.laneIndex].Remove(note);
        }

        if (activeHoldNotes.ContainsKey(note.data.laneIndex) && activeHoldNotes[note.data.laneIndex] == note)
        {
            activeHoldNotes.Remove(note.data.laneIndex);
        }
    }
}
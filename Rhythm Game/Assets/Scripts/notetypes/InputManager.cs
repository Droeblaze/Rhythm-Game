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
    public float okWindow = 0.2f;
    public float missWindow = 0.25f;

    [Header("Timing Windows (Stick - Lanes 0-2)")]
    public float stickPerfectWindow = 0.07f;
    public float stickGreatWindow = 0.14f;
    public float stickGoodWindow = 0.21f;
    public float stickOkWindow = 0.28f;
    public float stickMissWindow = 0.35f;

    [Header("Hold Note Settings")]
    public float holdPerfectThreshold = 0.95f;
    public float holdGreatThreshold = 0.90f;
    public float holdGoodThreshold = 0.85f;
    public float holdOkThreshold = 0.80f;
    public float holdGracePeriod = 0.30f;

    [Header("Stick Settings")]
    public float stickDeadzone = 0.5f;

    private Dictionary<int, List<NoteVisual>> notesInLane = new Dictionary<int, List<NoteVisual>>();

    private Dictionary<int, NoteVisual> activeHoldNotes = new Dictionary<int, NoteVisual>();

    // Tracks how long a hold has been continuously released per lane
    private Dictionary<int, float> holdReleaseTimers = new Dictionary<int, float>();

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
        float closestAbsDistance = float.MaxValue;

        float maxHitDistance = stickMissWindow * noteSpawner.scrollSpeed;

        foreach (NoteVisual note in notesInLane[lane])
        {
            if (note == null || note.data.stickDirection != direction) continue;
            if (note.isHoldActive || note.holdFinished) continue;

            float absDistance = Mathf.Abs(note.transform.position.y - judgementLine.position.y);

            if (absDistance < closestAbsDistance && absDistance <= maxHitDistance)
            {
                closestAbsDistance = absDistance;
                closestNote = note;
            }
        }

        if (closestNote != null)
        {
            HitNote(closestNote, closestAbsDistance, true);
        }
    }

    void CheckButtonNoteHit(int lane, ButtonRow row)
    {
        if (!notesInLane.ContainsKey(lane)) return;

        NoteVisual closestNote = null;
        float closestAbsDistance = float.MaxValue;

        float maxHitDistance = missWindow * noteSpawner.scrollSpeed;

        foreach (NoteVisual note in notesInLane[lane])
        {
            if (note == null || note.data.buttonRow != row) continue;
            if (note.isHoldActive || note.holdFinished) continue;

            float absDistance = Mathf.Abs(note.transform.position.y - judgementLine.position.y);

            if (absDistance < closestAbsDistance && absDistance <= maxHitDistance)
            {
                closestAbsDistance = absDistance;
                closestNote = note;
            }
        }

        if (closestNote != null)
        {
            HitNote(closestNote, closestAbsDistance, false);
        }
    }

    void HitNote(NoteVisual note, float distance, bool isStickLane)
    {
        float timing = distance / noteSpawner.scrollSpeed;

        float pWindow = isStickLane ? stickPerfectWindow : perfectWindow;
        float grWindow = isStickLane ? stickGreatWindow : greatWindow;
        float goWindow = isStickLane ? stickGoodWindow : goodWindow;
        float oWindow = isStickLane ? stickOkWindow : okWindow;
        float mWindow = isStickLane ? stickMissWindow : missWindow;

        string judgement = "Miss";
        if (timing <= pWindow)
            judgement = "Perfect!";
        else if (timing <= grWindow)
            judgement = "Great!";
        else if (timing <= goWindow)
            judgement = "Good";
        else if (timing <= oWindow)
            judgement = "OK";
        else if (timing <= mWindow)
            judgement = "Miss";

        if (note.data.noteType == NoteType.Hold && judgement != "Miss")
        {
            Debug.Log($"Lane {note.data.laneIndex} Hold Started! Initial: {judgement} (timing: {timing:F3}s)");

            if (judgementDisplay != null)
                judgementDisplay.ShowJudgement(judgement);

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.RecordJudgement(judgement);

            // If there's already an active hold in this lane, fail it first
            // so its isHoldActive flag gets cleared and NoteSpawner stops pinning it
            int lane = note.data.laneIndex;
            if (activeHoldNotes.ContainsKey(lane) && activeHoldNotes[lane] != null)
            {
                NoteVisual previousHold = activeHoldNotes[lane];
                FailHold(previousHold, lane);
            }

            note.MarkAsHit();
            activeHoldNotes[note.data.laneIndex] = note;
            holdReleaseTimers[note.data.laneIndex] = 0f;
            return;
        }

        Debug.Log($"Lane {note.data.laneIndex} Hit! Judgement: {judgement} (timing: {timing:F3}s)");

        if (judgementDisplay != null)
            judgementDisplay.ShowJudgement(judgement);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.RecordJudgement(judgement);

        notesInLane[note.data.laneIndex].Remove(note);

        // FIX: A Hold note hit with "Miss" timing must NOT call MarkAsHit(), which would set
        // isHoldActive=true without registering in activeHoldNotes, permanently pinning the note.
        // Instead, treat it like a missed note so it scrolls off cleanly.
        if (note.data.noteType == NoteType.Hold)
            note.MarkAsMiss();
        else
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
                holdReleaseTimers.Remove(lane);
                continue;
            }

            bool stillHeld = IsInputStillHeld(note);

            // Always count down hold time while the hold is active,
            // even during the grace period, so the tail keeps shrinking
            note.holdTimeRemaining -= Time.deltaTime;

            if (note.holdTimeRemaining <= 0f)
            {
                // Hold duration fully elapsed — check if still held or within grace
                if (stillHeld || holdReleaseTimers[lane] < holdGracePeriod)
                {
                    CompleteHold(note, lane);
                }
                else
                {
                    FailHold(note, lane);
                }
                continue;
            }

            if (stillHeld)
            {
                // Input is held — reset the grace timer
                holdReleaseTimers[lane] = 0f;
            }
            else
            {
                // Input released — accumulate grace timer
                if (!holdReleaseTimers.ContainsKey(lane))
                    holdReleaseTimers[lane] = 0f;

                holdReleaseTimers[lane] += Time.deltaTime;

                if (holdReleaseTimers[lane] >= holdGracePeriod)
                {
                    FailHold(note, lane);
                }
                // else: still within grace period, hold stays alive
            }
        }

        // Safety net: catch any hold note that has isHoldActive=true but is not tracked in
        // activeHoldNotes. This should not normally occur, but guards against edge cases
        // causing a note to be permanently pinned at the judgement line.
        foreach (var kvp in notesInLane)
        {
            int lane = kvp.Key;
            List<NoteVisual> orphans = null;

            foreach (NoteVisual note in kvp.Value)
            {
                if (note != null && note.isHoldActive &&
                    (!activeHoldNotes.ContainsKey(lane) || activeHoldNotes[lane] != note))
                {
                    if (orphans == null) orphans = new List<NoteVisual>();
                    orphans.Add(note);
                }
            }

            if (orphans != null)
            {
                foreach (NoteVisual orphan in orphans)
                {
                    Debug.LogWarning($"[InputManager] Orphaned hold note on lane {lane} — force failing.");
                    FailHold(orphan, lane);
                }
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
        holdReleaseTimers.Remove(lane);
        note.MarkHoldComplete();
    }

    void FailHold(NoteVisual note, int lane)
    {
        float heldRatio = 1f - (note.holdTimeRemaining / note.data.holdDuration);
        heldRatio = Mathf.Clamp01(heldRatio);

        string judgement;
        if (heldRatio >= holdPerfectThreshold)
            judgement = "Perfect!";
        else if (heldRatio >= holdGreatThreshold)
            judgement = "Great!";
        else if (heldRatio >= holdGoodThreshold)
            judgement = "Good";
        else if (heldRatio >= holdOkThreshold)
            judgement = "OK";
        else
            judgement = "Miss";

        Debug.Log($"Lane {lane} Hold Released! Held {heldRatio:P0} – {judgement}");

        if (judgementDisplay != null)
            judgementDisplay.ShowJudgement(judgement);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.RecordJudgement(judgement);

        notesInLane[lane].Remove(note);
        activeHoldNotes.Remove(lane);
        holdReleaseTimers.Remove(lane);
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
            holdReleaseTimers.Remove(note.data.laneIndex);
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
            holdReleaseTimers.Remove(note.data.laneIndex);
        }
    }
}
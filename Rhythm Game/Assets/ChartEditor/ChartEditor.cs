using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ChartEditor : EditorWindow
{
    private ChartData currentChart;
    private AudioClip audioClip;
    private AudioSource previewAudioSource;

    // Playback
    private bool isPlaying = false;
    private double songTime = 0;
    private double lastEditorTime = 0;

    // Editor settings
    private float zoom = 1f;
    private Vector2 scrollPosition;
    private float timelineHeight = 600f;
    private float laneWidth = 80f;
    private float beatHeight = 100f; // Height per beat

    // Note placement
    private int selectedLane = 0;
    private NoteType selectedNoteType = NoteType.Tap;
    private StickDirection selectedStickDirection = StickDirection.Horizontal;
    private ButtonRow selectedButtonRow = ButtonRow.Top;

    // Grid snapping
    private int beatDivision = 4; // Snap to 1/4 beats

    // Selection & Copy/Paste
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private List<NoteData> selectedNotes = new List<NoteData>();
    private List<NoteData> clipboard = new List<NoteData>();

    [MenuItem("Window/Chart Editor")]
    public static void ShowWindow()
    {
        GetWindow<ChartEditor>("Chart Editor");
    }

    void OnEnable()
    {
        // Create audio source for preview
        GameObject audioObj = GameObject.Find("ChartEditorAudio");
        if (audioObj == null)
        {
            audioObj = new GameObject("ChartEditorAudio");
            audioObj.hideFlags = HideFlags.HideAndDontSave;
        }

        previewAudioSource = audioObj.GetComponent<AudioSource>();
        if (previewAudioSource == null)
        {
            previewAudioSource = audioObj.AddComponent<AudioSource>();
        }

        EditorApplication.update += OnEditorUpdate;
    }

    void OnDisable()
    {
        StopPlayback();
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        if (isPlaying && previewAudioSource != null && previewAudioSource.isPlaying)
        {
            songTime = previewAudioSource.time;
            Repaint();
        }
    }

    void OnGUI()
    {
        DrawToolbar();
        DrawTimeline();
        HandleKeyboardShortcuts();
    }

    void DrawToolbar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.toolbar);

        // Chart selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Chart:", GUILayout.Width(50));
        ChartData newChart = (ChartData)EditorGUILayout.ObjectField(currentChart, typeof(ChartData), false);
        if (newChart != currentChart)
        {
            currentChart = newChart;
            if (currentChart != null && currentChart.songAudio != null)
            {
                audioClip = currentChart.songAudio;
                previewAudioSource.clip = audioClip;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (currentChart == null)
        {
            EditorGUILayout.HelpBox("Select or create a ChartData asset to begin editing.", MessageType.Info);
            if (GUILayout.Button("Create New Chart"))
            {
                CreateNewChart();
            }
            EditorGUILayout.EndVertical();
            return;
        }

        // Playback controls
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isPlaying ? "? Pause" : "? Play", GUILayout.Width(80)))
        {
            TogglePlayback();
        }
        if (GUILayout.Button("? Stop", GUILayout.Width(80)))
        {
            StopPlayback();
        }

        EditorGUILayout.LabelField($"Time: {songTime:F2}s", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        // Note placement tools
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Note Placement", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Lane:", GUILayout.Width(50));
        selectedLane = EditorGUILayout.IntSlider(selectedLane, 0, 5);
        EditorGUILayout.EndHorizontal();

        selectedNoteType = (NoteType)EditorGUILayout.EnumPopup("Note Type:", selectedNoteType);

        // Show appropriate options based on lane
        if (selectedLane <= 2)
        {
            selectedStickDirection = (StickDirection)EditorGUILayout.EnumPopup("Direction:", selectedStickDirection);
        }
        else
        {
            selectedButtonRow = (ButtonRow)EditorGUILayout.EnumPopup("Button Row:", selectedButtonRow);
        }

        // Grid settings
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Beat Division:", GUILayout.Width(100));
        beatDivision = EditorGUILayout.IntPopup(beatDivision, new string[] { "1/4", "1/8", "1/16" }, new int[] { 4, 8, 16 });
        EditorGUILayout.EndHorizontal();

        // Zoom
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Zoom:", GUILayout.Width(50));
        zoom = EditorGUILayout.Slider(zoom, 0.5f, 3f);
        EditorGUILayout.EndHorizontal();

        // Selection info
        if (selectedNotes.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Selected: {selectedNotes.Count} notes | Ctrl+C Copy, Ctrl+V Paste, Del Delete", EditorStyles.miniLabel);
        }
        if (clipboard.Count > 0)
        {
            EditorGUILayout.LabelField($"Clipboard: {clipboard.Count} notes", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();
    }

    void DrawTimeline()
    {
        if (currentChart == null) return;

        // Adjust this value to give more space to toolbar - increased from 140 to 200
        Rect timelineRect = new Rect(0, 200, position.width, position.height - 200);

        scrollPosition = GUI.BeginScrollView(timelineRect, scrollPosition,
            new Rect(0, 0, laneWidth * 6, GetTimelineLength()));

        DrawLanes();
        DrawGridLines();
        DrawNotes();
        DrawPlayhead();
        DrawSelectionRect();

        // HandleInput is inside the scroll view, so mousePosition is in content-space
        HandleInput(timelineRect);

        GUI.EndScrollView();
    }

    float GetTimelineLength()
    {
        if (audioClip == null) return 1000f;
        float songLength = audioClip.length;
        float beatsPerSecond = currentChart.bpm / 60f;
        float totalBeats = songLength * beatsPerSecond;
        return totalBeats * beatHeight * zoom;
    }

    void DrawLanes()
    {
        for (int i = 0; i < 6; i++)
        {
            Rect laneRect = new Rect(i * laneWidth, 0, laneWidth, GetTimelineLength());
            EditorGUI.DrawRect(laneRect, i % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.25f, 0.25f, 0.25f));

            // Lane labels - only draw at the top with fixed position
            if (scrollPosition.y < 50) // Only show label when scrolled near top
            {
                Rect labelRect = new Rect(i * laneWidth + 5, scrollPosition.y + 5, laneWidth - 10, 20);
                GUI.Label(labelRect, $"Lane {i}", EditorStyles.whiteBoldLabel);
            }
        }
    }

    void DrawGridLines()
    {
        if (currentChart.bpm <= 0) return;

        float beatsPerSecond = currentChart.bpm / 60f;
        float totalTime = audioClip != null ? audioClip.length : 60f;

        for (float time = 0; time < totalTime; time += 1f / beatsPerSecond / beatDivision)
        {
            float y = TimeToY(time);
            bool isMajorBeat = Mathf.Approximately(time % (1f / beatsPerSecond), 0f);

            Color lineColor = isMajorBeat ? new Color(1, 1, 1, 0.3f) : new Color(1, 1, 1, 0.1f);
            EditorGUI.DrawRect(new Rect(0, y, laneWidth * 6, 1), lineColor);
        }
    }

    void DrawNotes()
    {
        if (currentChart.notes == null) return;

        foreach (NoteData note in currentChart.notes)
        {
            float y = TimeToY(note.timestamp);
            Rect noteRect = new Rect(note.laneIndex * laneWidth + 10, y - 10, laneWidth - 20, 20);

            Color noteColor = GetNoteColor(note);

            // Highlight selected notes
            if (selectedNotes.Contains(note))
            {
                // Draw selection outline
                EditorGUI.DrawRect(new Rect(noteRect.x - 2, noteRect.y - 2, noteRect.width + 4, noteRect.height + 4), Color.white);
            }

            EditorGUI.DrawRect(noteRect, noteColor);

            // Draw note info
            string noteLabel = note.laneIndex <= 2 ? note.stickDirection.ToString() : note.buttonRow.ToString();
            GUI.Label(noteRect, noteLabel, EditorStyles.centeredGreyMiniLabel);
        }
    }

    void DrawPlayhead()
    {
        float y = TimeToY((float)songTime);
        EditorGUI.DrawRect(new Rect(0, y - 2, laneWidth * 6, 4), Color.red);
    }

    void DrawSelectionRect()
    {
        if (!isDragging) return;

        Rect selRect = GetSelectionRect();
        // Semi-transparent fill
        EditorGUI.DrawRect(selRect, new Color(0.3f, 0.6f, 1f, 0.15f));
        // Border lines
        EditorGUI.DrawRect(new Rect(selRect.x, selRect.y, selRect.width, 1), new Color(0.3f, 0.6f, 1f, 0.8f)); // top
        EditorGUI.DrawRect(new Rect(selRect.x, selRect.yMax - 1, selRect.width, 1), new Color(0.3f, 0.6f, 1f, 0.8f)); // bottom
        EditorGUI.DrawRect(new Rect(selRect.x, selRect.y, 1, selRect.height), new Color(0.3f, 0.6f, 1f, 0.8f)); // left
        EditorGUI.DrawRect(new Rect(selRect.xMax - 1, selRect.y, 1, selRect.height), new Color(0.3f, 0.6f, 1f, 0.8f)); // right
    }

    Rect GetSelectionRect()
    {
        float x = Mathf.Min(dragStartPos.x, dragEndPos.x);
        float y = Mathf.Min(dragStartPos.y, dragEndPos.y);
        float w = Mathf.Abs(dragEndPos.x - dragStartPos.x);
        float h = Mathf.Abs(dragEndPos.y - dragStartPos.y);
        return new Rect(x, y, w, h);
    }

    Color GetNoteColor(NoteData note)
    {
        if (note.laneIndex <= 2)
        {
            return note.laneIndex == 0 ? Color.red : (note.laneIndex == 1 ? Color.blue : Color.green);
        }
        return Color.yellow;
    }

    float TimeToY(float time)
    {
        if (currentChart.bpm <= 0) return 0;
        float beatsPerSecond = currentChart.bpm / 60f;
        float beats = time * beatsPerSecond;
        return beats * beatHeight * zoom;
    }

    float YToTime(float y)
    {
        if (currentChart.bpm <= 0) return 0;
        float beatsPerSecond = currentChart.bpm / 60f;
        float beats = y / (beatHeight * zoom);
        return beats / beatsPerSecond;
    }

    void HandleInput(Rect timelineRect)
    {
        Event e = Event.current;

        Rect visibleContentRect = new Rect(scrollPosition.x, scrollPosition.y, timelineRect.width, timelineRect.height);
        if (!visibleContentRect.Contains(e.mousePosition))
            return;

        // Shift+Click+Drag to select a block of notes
        if (e.shift)
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                isDragging = true;
                dragStartPos = e.mousePosition;
                dragEndPos = e.mousePosition;
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.MouseDrag && isDragging)
            {
                dragEndPos = e.mousePosition;
                e.Use();
                Repaint();
            }
        }

        if (e.type == EventType.MouseUp && e.button == 0 && isDragging)
        {
            isDragging = false;
            dragEndPos = e.mousePosition;
            SelectNotesInRect();
            e.Use();
            Repaint();
            return;
        }

        // Normal click (no shift) — place or delete notes
        if (!e.shift && e.type == EventType.MouseDown && e.button == 0)
        {
            // Click on empty area clears selection
            if (selectedNotes.Count > 0 && !e.control)
            {
                selectedNotes.Clear();
            }

            int clickedLane = Mathf.FloorToInt(e.mousePosition.x / laneWidth);
            float clickedTime = YToTime(e.mousePosition.y);

            Debug.Log($"Content Mouse: {e.mousePosition}, Lane: {clickedLane}, Time: {clickedTime:F2}s");

            if (clickedLane >= 0 && clickedLane < 6)
            {
                NoteData clickedNote = FindNoteAt(clickedLane, clickedTime, 0.2f);

                if (clickedNote != null && e.control) // Ctrl+Click to delete
                {
                    DeleteNote(clickedNote);
                    e.Use();
                    Repaint();
                }
                else if (clickedNote == null) // Empty space - place new note
                {
                    PlaceNote(clickedLane, clickedTime);
                    e.Use();
                    Repaint();
                }
            }
        }

        // Right-click to delete
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            int clickedLane = Mathf.FloorToInt(e.mousePosition.x / laneWidth);
            float clickedTime = YToTime(e.mousePosition.y);

            NoteData clickedNote = FindNoteAt(clickedLane, clickedTime, 0.2f);
            if (clickedNote != null)
            {
                DeleteNote(clickedNote);
                e.Use();
                Repaint();
            }
        }
    }

    void HandleKeyboardShortcuts()
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;

        // Ctrl+C — Copy selected notes
        if (e.control && e.keyCode == KeyCode.C && selectedNotes.Count > 0)
        {
            CopySelection();
            e.Use();
        }
        // Ctrl+V — Paste at playhead
        else if (e.control && e.keyCode == KeyCode.V && clipboard.Count > 0)
        {
            PasteAtPlayhead();
            e.Use();
        }
        // Delete / Backspace — Delete selected notes
        else if ((e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) && selectedNotes.Count > 0)
        {
            DeleteSelectedNotes();
            e.Use();
        }
        // Escape — Clear selection
        else if (e.keyCode == KeyCode.Escape)
        {
            selectedNotes.Clear();
            Repaint();
            e.Use();
        }
    }

    void SelectNotesInRect()
    {
        selectedNotes.Clear();

        if (currentChart == null || currentChart.notes == null) return;

        Rect selRect = GetSelectionRect();
        float minTime = YToTime(selRect.y);
        float maxTime = YToTime(selRect.yMax);
        int minLane = Mathf.FloorToInt(selRect.x / laneWidth);
        int maxLane = Mathf.FloorToInt(selRect.xMax / laneWidth);

        minLane = Mathf.Clamp(minLane, 0, 5);
        maxLane = Mathf.Clamp(maxLane, 0, 5);

        foreach (NoteData note in currentChart.notes)
        {
            if (note.laneIndex >= minLane && note.laneIndex <= maxLane &&
                note.timestamp >= minTime && note.timestamp <= maxTime)
            {
                selectedNotes.Add(note);
            }
        }

        Debug.Log($"Selected {selectedNotes.Count} notes (Lanes {minLane}-{maxLane}, Time {minTime:F2}s-{maxTime:F2}s)");
    }

    void CopySelection()
    {
        clipboard.Clear();

        // Find the earliest timestamp to use as the reference point
        float earliestTime = selectedNotes.Min(n => n.timestamp);

        foreach (NoteData note in selectedNotes)
        {
            clipboard.Add(new NoteData
            {
                // Store time as relative offset, but keep absolute lane indices
                timestamp = note.timestamp - earliestTime,
                laneIndex = note.laneIndex,
                noteType = note.noteType,
                stickDirection = note.stickDirection,
                buttonRow = note.buttonRow,
                holdDuration = note.holdDuration
            });
        }

        Debug.Log($"Copied {clipboard.Count} notes to clipboard");
    }

    void PasteAtPlayhead()
    {
        if (currentChart == null || clipboard.Count == 0) return;

        float pasteTime = (float)songTime;

        // Snap paste position to grid
        float beatsPerSecond = currentChart.bpm / 60f;
        float snapInterval = 1f / beatsPerSecond / beatDivision;
        pasteTime = Mathf.Round(pasteTime / snapInterval) * snapInterval;

        selectedNotes.Clear();

        foreach (NoteData clipNote in clipboard)
        {
            int absoluteLane = clipNote.laneIndex; // Relative lane offset from copy
            float absoluteTime = pasteTime + clipNote.timestamp; // Relative time offset from clip

            // Clamp lane to valid range
            if (absoluteLane < 0 || absoluteLane > 5) continue;

            // Skip if a note already exists at this position
            if (FindNoteAt(absoluteLane, absoluteTime, 0.01f) != null) continue;

            NoteData newNote = new NoteData
            {
                timestamp = absoluteTime,
                laneIndex = absoluteLane,
                noteType = clipNote.noteType,
                stickDirection = clipNote.stickDirection,
                buttonRow = clipNote.buttonRow,
                holdDuration = clipNote.holdDuration
            };

            currentChart.notes.Add(newNote);
            selectedNotes.Add(newNote);
        }

        currentChart.SortNotes();
        EditorUtility.SetDirty(currentChart);
        Debug.Log($"Pasted {selectedNotes.Count} notes at {pasteTime:F2}s");
        Repaint();
    }

    void DeleteSelectedNotes()
    {
        foreach (NoteData note in selectedNotes)
        {
            currentChart.notes.Remove(note);
        }

        Debug.Log($"Deleted {selectedNotes.Count} selected notes");
        selectedNotes.Clear();
        EditorUtility.SetDirty(currentChart);
        Repaint();
    }

    void DeleteNote(NoteData note)
    {
        currentChart.notes.Remove(note);
        selectedNotes.Remove(note);
        EditorUtility.SetDirty(currentChart);
        Debug.Log($"Deleted note at {note.timestamp:F2}s in lane {note.laneIndex}");
    }

    void PlaceNote(int lane, float time)
    {
        // Snap to grid
        float beatsPerSecond = currentChart.bpm / 60f;
        float snapInterval = 1f / beatsPerSecond / beatDivision;
        time = Mathf.Round(time / snapInterval) * snapInterval;

        NoteData newNote = new NoteData
        {
            timestamp = time,
            laneIndex = lane,
            noteType = selectedNoteType,
            stickDirection = selectedStickDirection,
            buttonRow = selectedButtonRow,
            holdDuration = 0f
        };

        currentChart.notes.Add(newNote);
        currentChart.SortNotes();
        EditorUtility.SetDirty(currentChart);
    }

    NoteData FindNoteAt(int lane, float time, float tolerance)
    {
        foreach (NoteData note in currentChart.notes)
        {
            if (note.laneIndex == lane && Mathf.Abs(note.timestamp - time) < tolerance)
            {
                return note;
            }
        }
        return null;
    }

    void TogglePlayback()
    {
        if (previewAudioSource == null || audioClip == null) return;

        if (isPlaying)
        {
            previewAudioSource.Pause();
            isPlaying = false;
        }
        else
        {
            previewAudioSource.time = (float)songTime;
            previewAudioSource.Play();
            isPlaying = true;
        }
    }

    void StopPlayback()
    {
        if (previewAudioSource != null)
        {
            previewAudioSource.Stop();
        }
        isPlaying = false;
        songTime = 0;
    }

    void CreateNewChart()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Chart", "NewChart", "asset", "Create a new chart data file");
        if (!string.IsNullOrEmpty(path))
        {
            ChartData newChart = CreateInstance<ChartData>();
            newChart.bpm = 120f;
            AssetDatabase.CreateAsset(newChart, path);
            AssetDatabase.SaveAssets();
            currentChart = newChart;
        }
    }
}








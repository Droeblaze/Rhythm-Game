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
    private float playbackSpeed = 1f;

    // Note click / metronome
    private bool noteClickEnabled = false;
    private AudioSource clickAudioSource;
    private AudioClip clickSound;
    private float clickVolume = 0.7f;
    private int lastClickedNoteIndex = -1;

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

    // Grid division options
    private static readonly string[] beatDivisionLabels = new string[]
    {
        "1/1", "1/2", "1/3", "1/4", "1/6", "1/8", "1/12", "1/16", "1/24", "1/32"
    };
    private static readonly int[] beatDivisionValues = new int[]
    {
        1, 2, 3, 4, 6, 8, 12, 16, 24, 32
    };

    // Playback speed options
    private static readonly string[] speedLabels = new string[]
    {
        "25%", "50%", "75%", "100%", "150%", "200%"
    };
    private static readonly float[] speedValues = new float[]
    {
        0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f
    };

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

        // Create a separate audio source for click sounds so they layer on top of the song
        AudioSource[] sources = audioObj.GetComponents<AudioSource>();
        if (sources.Length > 1)
        {
            clickAudioSource = sources[1];
        }
        else
        {
            clickAudioSource = audioObj.AddComponent<AudioSource>();
        }

        clickAudioSource.playOnAwake = false;
        clickAudioSource.loop = false;

        // Generate a short click sound procedurally (no external asset needed)
        clickSound = GenerateClickSound();

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

            // Play click sounds on notes as the playhead crosses them
            if (noteClickEnabled && currentChart != null && currentChart.notes != null)
            {
                PlayNoteClicks();
            }

            Repaint();
        }
    }

    void PlayNoteClicks()
    {
        // Find any notes that the playhead just crossed since the last update
        for (int i = 0; i < currentChart.notes.Count; i++)
        {
            NoteData note = currentChart.notes[i];

            // Skip notes we've already clicked or that are in the future
            if (i <= lastClickedNoteIndex) continue;

            if (note.timestamp <= (float)songTime)
            {
                // Play the click
                if (clickAudioSource != null && clickSound != null)
                {
                    clickAudioSource.volume = clickVolume;
                    clickAudioSource.PlayOneShot(clickSound);
                }
                lastClickedNoteIndex = i;
            }
            else
            {
                // Notes are sorted, so no more to check
                break;
            }
        }
    }

    AudioClip GenerateClickSound()
    {
        // Generate a short, punchy click sound (a quick sine burst at 1000Hz)
        int sampleRate = 44100;
        float duration = 0.03f; // 30ms click
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float frequency = 1000f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - ((float)i / sampleCount); // Linear fade out
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
        }

        AudioClip clip = AudioClip.Create("EditorClick", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
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

        // Playback speed
        EditorGUILayout.LabelField("Speed:", GUILayout.Width(45));
        float newSpeed = speedValues[EditorGUILayout.IntPopup(
            System.Array.IndexOf(speedValues, playbackSpeed) >= 0 ? System.Array.IndexOf(speedValues, playbackSpeed) : 3,
            speedLabels, Enumerable.Range(0, speedValues.Length).ToArray(), GUILayout.Width(60))];
        if (newSpeed != playbackSpeed)
        {
            playbackSpeed = newSpeed;
            if (previewAudioSource != null)
            {
                previewAudioSource.pitch = playbackSpeed;
            }
        }

        EditorGUILayout.EndHorizontal();

        // Seek controls
        EditorGUILayout.BeginHorizontal();
        float maxTime = audioClip != null ? audioClip.length : 60f;

        // Jump to start / back / forward / end buttons
        if (GUILayout.Button("|<", GUILayout.Width(30)))
        {
            SeekTo(0f);
        }
        if (GUILayout.Button("<<", GUILayout.Width(30)))
        {
            SeekByBeats(-4);
        }
        if (GUILayout.Button("<", GUILayout.Width(30)))
        {
            SeekByBeats(-1);
        }

        // Time scrubber slider
        EditorGUI.BeginChangeCheck();
        float scrubTime = EditorGUILayout.Slider((float)songTime, 0f, maxTime);
        if (EditorGUI.EndChangeCheck())
        {
            SeekTo(scrubTime);
        }

        if (GUILayout.Button(">", GUILayout.Width(30)))
        {
            SeekByBeats(1);
        }
        if (GUILayout.Button(">>", GUILayout.Width(30)))
        {
            SeekByBeats(4);
        }
        if (GUILayout.Button(">|", GUILayout.Width(30)))
        {
            SeekTo(maxTime);
        }
        EditorGUILayout.EndHorizontal();

        // Beat position display
        if (currentChart.bpm > 0)
        {
            float beatsPerSecond = currentChart.bpm / 60f;
            float currentBeat = (float)songTime * beatsPerSecond;
            int measure = Mathf.FloorToInt(currentBeat / 4f) + 1;
            float beatInMeasure = (currentBeat % 4f) + 1f;
            EditorGUILayout.LabelField($"Position: Measure {measure}, Beat {beatInMeasure:F2}  |  Beat {currentBeat:F2}  |  BPM: {currentChart.bpm}", EditorStyles.miniLabel);
        }

        // Note click toggle
        EditorGUILayout.BeginHorizontal();
        noteClickEnabled = EditorGUILayout.Toggle("Note Click", noteClickEnabled);
        if (noteClickEnabled)
        {
            EditorGUILayout.LabelField("Volume:", GUILayout.Width(50));
            clickVolume = EditorGUILayout.Slider(clickVolume, 0f, 1f);
        }
        EditorGUILayout.EndHorizontal();

        // Custom click sound (optional override)
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Click Sound:", GUILayout.Width(80));
        AudioClip customClick = (AudioClip)EditorGUILayout.ObjectField(
            clickSound != null && clickSound.name != "EditorClick" ? clickSound : null,
            typeof(AudioClip), false);
        if (customClick != null)
        {
            clickSound = customClick;
        }
        else if (clickSound == null || (clickSound.name != "EditorClick" && customClick == null))
        {
            clickSound = GenerateClickSound();
        }
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
        beatDivision = EditorGUILayout.IntPopup(beatDivision, beatDivisionLabels, beatDivisionValues);
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

        // Adjust this value to give more space to toolbar
        float toolbarBottom = 260;
        Rect timelineRect = new Rect(0, toolbarBottom, position.width, position.height - toolbarBottom);

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
        string[] laneNames = { "L-Stick", "V-Stick", "R-Stick", "Btn A", "Btn B", "Btn C" };

        for (int i = 0; i < 6; i++)
        {
            Rect laneRect = new Rect(i * laneWidth, 0, laneWidth, GetTimelineLength());
            EditorGUI.DrawRect(laneRect, i % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.25f, 0.25f, 0.25f));

            // Lane labels - only draw at the top with fixed position
            if (scrollPosition.y < 50) // Only show label when scrolled near top
            {
                Rect labelRect = new Rect(i * laneWidth + 5, scrollPosition.y + 5, laneWidth - 10, 20);
                GUI.Label(labelRect, $"{i}: {laneNames[i]}", EditorStyles.whiteBoldLabel);
            }
        }
    }

    void DrawGridLines()
    {
        if (currentChart.bpm <= 0) return;

        float beatsPerSecond = currentChart.bpm / 60f;
        float secondsPerBeat = 1f / beatsPerSecond;
        float totalTime = audioClip != null ? audioClip.length : 60f;
        float subdivisionInterval = secondsPerBeat / beatDivision;

        // Determine visible Y range to only draw visible grid lines
        float visibleTop = scrollPosition.y;
        float visibleBottom = scrollPosition.y + position.height;
        float startTime = Mathf.Max(0f, YToTime(visibleTop) - subdivisionInterval);
        float endTime = Mathf.Min(totalTime, YToTime(visibleBottom) + subdivisionInterval);

        // Snap startTime to nearest subdivision
        int startDiv = Mathf.FloorToInt(startTime / subdivisionInterval);
        startTime = startDiv * subdivisionInterval;

        for (float time = startTime; time <= endTime; time += subdivisionInterval)
        {
            float y = TimeToY(time);
            float beatNumber = time * beatsPerSecond;

            // Determine line importance
            bool isMeasureLine = Mathf.Approximately(beatNumber % 4f, 0f) || beatNumber % 4f < 0.001f;
            bool isBeatLine = Mathf.Approximately(beatNumber % 1f, 0f) || beatNumber % 1f < 0.001f;
            bool isTripletLine = (beatDivision % 3 == 0) && !isBeatLine;

            Color lineColor;
            float lineHeight;

            if (isMeasureLine)
            {
                lineColor = new Color(1, 1, 1, 0.5f);
                lineHeight = 2;
            }
            else if (isBeatLine)
            {
                lineColor = new Color(1, 1, 1, 0.3f);
                lineHeight = 1;
            }
            else if (isTripletLine)
            {
                lineColor = new Color(0.6f, 0.4f, 1f, 0.2f); // Purple tint for triplets
                lineHeight = 1;
            }
            else
            {
                lineColor = new Color(1, 1, 1, 0.1f);
                lineHeight = 1;
            }

            EditorGUI.DrawRect(new Rect(0, y, laneWidth * 6, lineHeight), lineColor);

            // Draw beat/measure numbers on major lines
            if (isMeasureLine || isBeatLine)
            {
                int measure = Mathf.FloorToInt(beatNumber / 4f) + 1;
                float beatInMeasure = (beatNumber % 4f) + 1f;
                string label = isMeasureLine ? $"M{measure}" : $"{beatInMeasure:F0}";
                Rect labelRect = new Rect(laneWidth * 6 + 4, y - 8, 40, 16);
                GUI.Label(labelRect, label, EditorStyles.miniLabel);
            }
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

    void SeekTo(float time)
    {
        float maxTime = audioClip != null ? audioClip.length : 60f;
        songTime = Mathf.Clamp(time, 0f, maxTime);

        if (isPlaying && previewAudioSource != null)
        {
            previewAudioSource.time = (float)songTime;
        }

        ResetClickTracking();
        Repaint();
    }

    void SeekByBeats(int beats)
    {
        if (currentChart == null || currentChart.bpm <= 0) return;
        float secondsPerBeat = 60f / currentChart.bpm;
        SeekTo((float)songTime + beats * secondsPerBeat);
    }

    void HandleInput(Rect timelineRect)
    {
        Event e = Event.current;

        Rect visibleContentRect = new Rect(scrollPosition.x, scrollPosition.y, timelineRect.width, timelineRect.height);
        if (!visibleContentRect.Contains(e.mousePosition))
            return;

        // Middle-click to seek playhead to clicked position
        if (e.type == EventType.MouseDown && e.button == 2)
        {
            float clickedTime = YToTime(e.mousePosition.y);
            SeekTo(clickedTime);
            e.Use();
            return;
        }

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
        // Left/Right arrow — seek by one beat
        else if (e.keyCode == KeyCode.LeftArrow)
        {
            SeekByBeats(-1);
            e.Use();
        }
        else if (e.keyCode == KeyCode.RightArrow)
        {
            SeekByBeats(1);
        }
        // Space — toggle playback
        else if (e.keyCode == KeyCode.Space)
        {
            TogglePlayback();
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
            previewAudioSource.pitch = playbackSpeed;
            previewAudioSource.time = (float)songTime;
            previewAudioSource.Play();
            isPlaying = true;

            // Reset click tracking to match current playhead position
            ResetClickTracking();
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
        lastClickedNoteIndex = -1;
    }

    void ResetClickTracking()
    {
        lastClickedNoteIndex = -1;

        if (currentChart == null || currentChart.notes == null) return;

        // Find the last note index that's already before the current playhead
        // so we don't re-trigger clicks for notes already passed
        for (int i = 0; i < currentChart.notes.Count; i++)
        {
            if (currentChart.notes[i].timestamp <= (float)songTime)
            {
                lastClickedNoteIndex = i;
            }
            else
            {
                break;
            }
        }
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








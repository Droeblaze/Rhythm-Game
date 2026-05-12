using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ChartEditor : EditorWindow
{
    private SongContainer currentSong;
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
    private float selectedHoldDuration = 1f;

    // Grid snapping
    private int beatDivision = 4;

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

    // BPM change editing
    private bool showBpmChanges = false;
    private float newBpmChangeTime = 0f;
    private float newBpmChangeValue = 120f;
    private Vector2 bpmScrollPosition;

    // BPM change marker selection (for deletion)
    private int selectedBpmChangeIndex = -1;

    // Tools panel scroll
    private Vector2 toolsPanelScroll;
    private float toolsPanelWidth = 320f;

    [MenuItem("Window/Chart Editor")]
    public static void ShowWindow()
    {
        GetWindow<ChartEditor>("Chart Editor");
    }

    void OnEnable()
    {
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

            if (noteClickEnabled && currentChart != null && currentChart.notes != null)
            {
                PlayNoteClicks();
            }

            Repaint();
        }
    }

    void PlayNoteClicks()
    {
        for (int i = 0; i < currentChart.notes.Count; i++)
        {
            NoteData note = currentChart.notes[i];

            if (i <= lastClickedNoteIndex) continue;

            if (note.timestamp <= (float)songTime)
            {
                if (clickAudioSource != null && clickSound != null)
                {
                    clickAudioSource.volume = clickVolume;
                    clickAudioSource.PlayOneShot(clickSound);
                }
                lastClickedNoteIndex = i;
            }
            else
            {
                break;
            }
        }
    }

    AudioClip GenerateClickSound()
    {
        int sampleRate = 44100;
        float duration = 0.03f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float frequency = 1000f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - ((float)i / sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
        }

        AudioClip clip = AudioClip.Create("EditorClick", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void OnGUI()
    {
        // Side-by-side layout: tools on the left, timeline grid on the right
        EditorGUILayout.BeginHorizontal();

        // === LEFT PANEL: Tools & Controls ===
        DrawToolsPanel();

        // === RIGHT PANEL: Timeline Grid ===
        DrawTimeline();

        EditorGUILayout.EndHorizontal();

        HandleKeyboardShortcuts();
    }

    void DrawToolsPanel()
    {
        Rect panelRect = EditorGUILayout.BeginVertical(GUILayout.Width(toolsPanelWidth));
        EditorGUI.DrawRect(new Rect(panelRect.x, panelRect.y, toolsPanelWidth, position.height), new Color(0.18f, 0.18f, 0.18f));

        toolsPanelScroll = EditorGUILayout.BeginScrollView(toolsPanelScroll, GUILayout.Width(toolsPanelWidth), GUILayout.Height(position.height));

        // Song Container selection
        EditorGUILayout.LabelField("Song", EditorStyles.boldLabel);
        SongContainer newSong = (SongContainer)EditorGUILayout.ObjectField("Song Container", currentSong, typeof(SongContainer), false);
        if (newSong != currentSong)
        {
            currentSong = newSong;
            if (currentSong != null && currentSong.songAudio != null)
            {
                audioClip = currentSong.songAudio;
                previewAudioSource.clip = audioClip;
            }
            else
            {
                audioClip = null;
            }
            // Clear chart when song changes
            currentChart = null;
        }

        if (currentSong == null)
        {
            EditorGUILayout.HelpBox("Select or create a Song Container asset to begin.", MessageType.Info);
            if (GUILayout.Button("Create New Song Container"))
            {
                CreateNewSongContainer();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            return;
        }

        // Display song info (read-only)
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Title", currentSong.title);
        EditorGUILayout.TextField("Artist", currentSong.artist);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(4);

        // Chart selection from the SongContainer's charts array
        EditorGUILayout.LabelField("Chart", EditorStyles.boldLabel);
        if (currentSong.charts != null && currentSong.charts.Length > 0)
        {
            // Build dropdown labels from chart names
            string[] chartLabels = new string[currentSong.charts.Length];
            for (int i = 0; i < currentSong.charts.Length; i++)
            {
                if (currentSong.charts[i] != null)
                    chartLabels[i] = $"{currentSong.charts[i].name} (? {currentSong.charts[i].difficulty:F1})";
                else
                    chartLabels[i] = $"[{i}] (null)";
            }

            int currentChartIndex = System.Array.IndexOf(currentSong.charts, currentChart);
            if (currentChartIndex < 0) currentChartIndex = 0;

            int selectedChartIndex = EditorGUILayout.Popup("Difficulty", currentChartIndex, chartLabels);
            if (currentSong.charts[selectedChartIndex] != null)
            {
                currentChart = currentSong.charts[selectedChartIndex];
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No charts assigned to this Song Container. Add ChartData assets to the charts array in the Inspector.", MessageType.Warning);
        }

        // Also allow direct chart assignment for flexibility
        ChartData directChart = (ChartData)EditorGUILayout.ObjectField("Or drag chart", currentChart, typeof(ChartData), false);
        if (directChart != currentChart && directChart != null)
        {
            currentChart = directChart;
        }

        if (currentChart == null)
        {
            if (GUILayout.Button("Create New Chart"))
            {
                CreateNewChart();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.Space(6);

        // --- Playback ---
        EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isPlaying ? "? Pause" : "? Play", GUILayout.Width(80)))
        {
            TogglePlayback();
        }
        if (GUILayout.Button("? Stop", GUILayout.Width(80)))
        {
            StopPlayback();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Time: {songTime:F2}s");

        // Playback speed
        EditorGUILayout.BeginHorizontal();
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

        EditorGUI.BeginChangeCheck();
        float scrubTime = EditorGUILayout.Slider((float)songTime, 0f, maxTime);
        if (EditorGUI.EndChangeCheck())
        {
            SeekTo(scrubTime);
        }

        // Beat position display — BPM now comes from SongContainer
        if (currentSong.bpm > 0)
        {
            float currentBeat = currentSong.TimeToBeat((float)songTime);
            float activeBpm = currentSong.GetBpmAtTime((float)songTime);
            int measure = Mathf.FloorToInt(currentBeat / 4f) + 1;
            float beatInMeasure = (currentBeat % 4f) + 1f;
            EditorGUILayout.LabelField(
                $"M{measure} Beat {beatInMeasure:F2} | BPM: {activeBpm}",
                EditorStyles.miniLabel);
        }

        EditorGUILayout.Space(6);

        // --- Note Click / Metronome ---
        EditorGUILayout.LabelField("Metronome", EditorStyles.boldLabel);

        noteClickEnabled = EditorGUILayout.Toggle("Note Click", noteClickEnabled);
        if (noteClickEnabled)
        {
            clickVolume = EditorGUILayout.Slider("Volume", clickVolume, 0f, 1f);
        }

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

        EditorGUILayout.Space(6);

        // --- Note Placement ---
        EditorGUILayout.LabelField("Note Placement", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Lane:", GUILayout.Width(50));
        selectedLane = EditorGUILayout.IntSlider(selectedLane, 0, 5);
        EditorGUILayout.EndHorizontal();

        selectedNoteType = (NoteType)EditorGUILayout.EnumPopup("Note Type:", selectedNoteType);

        if (selectedNoteType == NoteType.Hold)
        {
            selectedHoldDuration = EditorGUILayout.FloatField("Hold Duration (s):", selectedHoldDuration);
            selectedHoldDuration = Mathf.Max(0.1f, selectedHoldDuration);
        }

        if (selectedLane <= 2)
        {
            selectedStickDirection = (StickDirection)EditorGUILayout.EnumPopup("Direction:", selectedStickDirection);
        }
        else
        {
            selectedButtonRow = (ButtonRow)EditorGUILayout.EnumPopup("Button Row:", selectedButtonRow);
        }

        EditorGUILayout.Space(6);

        // --- Grid Settings ---
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Beat Div:", GUILayout.Width(60));
        beatDivision = EditorGUILayout.IntPopup(beatDivision, beatDivisionLabels, beatDivisionValues);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Zoom:", GUILayout.Width(50));
        zoom = EditorGUILayout.Slider(zoom, 0.1f, 10f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        // --- Selection Info ---
        if (selectedNotes.Count > 0)
        {
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{selectedNotes.Count} notes selected", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Ctrl+C Copy | Ctrl+V Paste | Del Delete", EditorStyles.miniLabel);
        }
        if (clipboard.Count > 0)
        {
            EditorGUILayout.LabelField($"Clipboard: {clipboard.Count} notes", EditorStyles.miniLabel);
        }

        EditorGUILayout.Space(6);

        // --- BPM Changes (now edits SongContainer) ---
        showBpmChanges = EditorGUILayout.Foldout(showBpmChanges, "BPM Changes", true, EditorStyles.foldoutHeader);
        if (showBpmChanges)
        {
            DrawBpmChangesPanel();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    void DrawBpmChangesPanel()
    {
        EditorGUILayout.HelpBox(
            $"Base BPM: {currentSong.bpm}. Add tempo changes below. " +
            "Each entry overrides the BPM from that timestamp onward.",
            MessageType.Info);

        // Add new BPM change
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Time (s):", GUILayout.Width(60));
        newBpmChangeTime = EditorGUILayout.FloatField(newBpmChangeTime, GUILayout.Width(60));
        EditorGUILayout.LabelField("BPM:", GUILayout.Width(35));
        newBpmChangeValue = EditorGUILayout.FloatField(newBpmChangeValue, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            if (newBpmChangeValue > 0f && newBpmChangeTime > 0f)
            {
                Undo.RecordObject(currentSong, "Add BPM Change");
                currentSong.bpmChanges.Add(new BpmChangeData
                {
                    timestamp = newBpmChangeTime,
                    bpm = newBpmChangeValue
                });
                currentSong.SortBpmChanges();
                EditorUtility.SetDirty(currentSong);
            }
        }

        if (GUILayout.Button("Add at Playhead"))
        {
            if (newBpmChangeValue > 0f)
            {
                Undo.RecordObject(currentSong, "Add BPM Change at Playhead");
                currentSong.bpmChanges.Add(new BpmChangeData
                {
                    timestamp = (float)songTime,
                    bpm = newBpmChangeValue
                });
                currentSong.SortBpmChanges();
                EditorUtility.SetDirty(currentSong);
            }
        }
        EditorGUILayout.EndHorizontal();

        // List existing BPM changes
        if (currentSong.bpmChanges.Count > 0)
        {
            bpmScrollPosition = EditorGUILayout.BeginScrollView(bpmScrollPosition, GUILayout.MaxHeight(120));
            for (int i = 0; i < currentSong.bpmChanges.Count; i++)
            {
                BpmChangeData change = currentSong.bpmChanges[i];
                EditorGUILayout.BeginHorizontal();

                // Editable fields
                EditorGUI.BeginChangeCheck();
                float editedTime = EditorGUILayout.FloatField(change.timestamp, GUILayout.Width(60));
                float editedBpm = EditorGUILayout.FloatField(change.bpm, GUILayout.Width(60));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(currentSong, "Edit BPM Change");
                    change.timestamp = editedTime;
                    change.bpm = editedBpm;
                    currentSong.SortBpmChanges();
                    EditorUtility.SetDirty(currentSong);
                }

                // Seek to this BPM change
                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    SeekTo(change.timestamp);
                    selectedBpmChangeIndex = i;
                }

                // Remove this BPM change
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(currentSong, "Remove BPM Change");
                    currentSong.bpmChanges.RemoveAt(i);
                    EditorUtility.SetDirty(currentSong);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("No BPM changes. Song uses constant BPM.", EditorStyles.miniLabel);
        }
    }

    void DrawTimeline()
    {
        if (currentSong == null || currentChart == null) return;

        // The timeline fills the remaining width to the right of the tools panel
        float timelineX = toolsPanelWidth + 2;
        float timelineWidth = position.width - timelineX;
        Rect timelineRect = new Rect(timelineX, 0, timelineWidth, position.height);

        // Draw a subtle border between panels
        EditorGUI.DrawRect(new Rect(timelineX - 2, 0, 2, position.height), new Color(0.1f, 0.1f, 0.1f));

        float contentWidth = laneWidth * 6 + 50;
        scrollPosition = GUI.BeginScrollView(timelineRect, scrollPosition,
            new Rect(0, 0, contentWidth, GetTimelineLength()));

        DrawLanes();
        DrawGridLines();
        DrawBpmChangeMarkers();
        DrawNotes();
        DrawPlayhead();
        DrawSelectionRect();

        HandleInput(timelineRect);

        GUI.EndScrollView();
    }

    float GetTimelineLength()
    {
        if (audioClip == null) return 1000f;
        float totalBeats = currentSong.TimeToBeat(audioClip.length);
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
        if (currentSong.bpm <= 0) return;

        float totalTime = audioClip != null ? audioClip.length : 60f;
        float visibleTop = scrollPosition.y;
        float visibleBottom = scrollPosition.y + position.height;

        var segments = GetBpmSegments(totalTime);

        foreach (var (segStart, segEnd, segBpm) in segments)
        {
            float secondsPerBeat = 60f / segBpm;
            float subdivisionSeconds = secondsPerBeat / beatDivision;
            int totalSubdivs = Mathf.CeilToInt((segEnd - segStart) / subdivisionSeconds);

            // Skip segments entirely outside the visible area
            float segStartY = TimeToY(segStart);
            float segEndY   = TimeToY(segEnd);
            if (segEndY < visibleTop || segStartY > visibleBottom) continue;

            // Cull to visible subdivision range within this segment
            int startSubdiv = 0;
            int endSubdiv   = totalSubdivs;

            if (segStartY < visibleTop)
            {
                float localVisible = YToTime(visibleTop) - segStart;
                startSubdiv = Mathf.Max(0, Mathf.FloorToInt(localVisible / subdivisionSeconds) - 1);
            }
            if (segEndY > visibleBottom)
            {
                float localVisible = YToTime(visibleBottom) - segStart;
                endSubdiv = Mathf.Min(totalSubdivs, Mathf.CeilToInt(localVisible / subdivisionSeconds) + 1);
            }

            for (int s = startSubdiv; s <= endSubdiv; s++)
            {
                float time = segStart + s * subdivisionSeconds;
                if (time > segEnd + subdivisionSeconds * 0.5f) break;

                float y = TimeToY(time);

                // Classify line type based on local beat position within this segment
                bool isMeasureLine = s % (beatDivision * 4) == 0;
                bool isBeatLine    = !isMeasureLine && s % beatDivision == 0;
                bool isTripletLine = beatDivision % 3 == 0 && !isBeatLine && !isMeasureLine;

                Color lineColor;
                float lineHeight;

                if (isMeasureLine)
                {
                    lineColor  = new Color(1f, 1f, 1f, 0.5f);
                    lineHeight = 2f;
                }
                else if (isBeatLine)
                {
                    lineColor  = new Color(1f, 1f, 1f, 0.3f);
                    lineHeight = 1f;
                }
                else if (isTripletLine)
                {
                    lineColor  = new Color(0.6f, 0.4f, 1f, 0.2f);
                    lineHeight = 1f;
                }
                else
                {
                    lineColor  = new Color(1f, 1f, 1f, 0.1f);
                    lineHeight = 1f;
                }

                EditorGUI.DrawRect(new Rect(0, y, laneWidth * 6, lineHeight), lineColor);

                if (isMeasureLine || isBeatLine)
                {
                    int measure       = s / (beatDivision * 4) + 1;
                    int beatInMeasure = s / beatDivision % 4 + 1;
                    string label      = isMeasureLine ? $"M{measure}" : $"{beatInMeasure}";
                    GUI.Label(new Rect(laneWidth * 6 + 4, y - 8, 40, 16), label, EditorStyles.miniLabel);
                }
            }
        }
    }

    /// <summary>
    /// Snaps a time value to the nearest grid subdivision within the active BPM segment.
    /// Phase resets at each BPM change so snapped positions always align with the local grid.
    /// </summary>
    float SnapTimeToGrid(float time)
    {
        float segmentStartTime = 0f;
        float segmentBpm       = currentSong.bpm;

        if (currentSong.bpmChanges != null)
        {
            foreach (BpmChangeData change in currentSong.bpmChanges)
            {
                if (change.timestamp <= time)
                {
                    segmentStartTime = change.timestamp;
                    segmentBpm       = change.bpm;
                }
                else break;
            }
        }

        float subdivisionSeconds = 60f / segmentBpm / beatDivision;
        float localTime          = time - segmentStartTime;
        float snappedLocal       = Mathf.Round(localTime / subdivisionSeconds) * subdivisionSeconds;
        return segmentStartTime + snappedLocal;
    }

    void DrawBpmChangeMarkers()
    {
        if (currentSong.bpmChanges == null) return;

        for (int i = 0; i < currentSong.bpmChanges.Count; i++)
        {
            BpmChangeData change = currentSong.bpmChanges[i];
            float y = TimeToY(change.timestamp);

            // Draw a bright orange line across all lanes
            EditorGUI.DrawRect(new Rect(0, y - 1, laneWidth * 6, 3), new Color(1f, 0.5f, 0f, 0.9f));

            // Draw BPM label
            bool isSelected = (selectedBpmChangeIndex == i);
            GUIStyle markerStyle = new GUIStyle(EditorStyles.miniLabel);
            markerStyle.normal.textColor = isSelected ? Color.yellow : new Color(1f, 0.6f, 0.1f);
            Rect labelRect = new Rect(laneWidth * 6 + 4, y - 14, 80, 16);
            GUI.Label(labelRect, $"BPM: {change.bpm}", markerStyle);
        }
    }

    /// <summary>
    /// Returns BPM segments covering the entire song duration.
    /// Each segment is (startTime, endTime, bpm).
    /// </summary>
    List<(float start, float end, float bpm)> GetBpmSegments(float totalTime)
    {
        var segments = new List<(float start, float end, float bpm)>();
        float prevTime = 0f;
        float currentBpm = currentSong.bpm;

        if (currentSong.bpmChanges != null)
        {
            for (int i = 0; i < currentSong.bpmChanges.Count; i++)
            {
                float changeTime = currentSong.bpmChanges[i].timestamp;
                if (changeTime > totalTime) break;
                if (changeTime > prevTime)
                {
                    segments.Add((prevTime, changeTime, currentBpm));
                }
                prevTime = changeTime;
                currentBpm = currentSong.bpmChanges[i].bpm;
            }
        }

        if (prevTime < totalTime)
        {
            segments.Add((prevTime, totalTime, currentBpm));
        }

        return segments;
    }

    void DrawNotes()
    {
        if (currentChart.notes == null) return;

        // Scale note dimensions with zoom so dense patterns don't overlap
        float noteHeight = Mathf.Clamp(16f * Mathf.Sqrt(zoom), 6f, 30f);
        float noteMarginH = Mathf.Clamp(8f / zoom, 2f, 20f);

        foreach (NoteData note in currentChart.notes)
        {
            float y = TimeToY(note.timestamp);
            float halfH = noteHeight / 2f;
            Rect noteRect = new Rect(
                note.laneIndex * laneWidth + noteMarginH,
                y - halfH,
                laneWidth - noteMarginH * 2f,
                noteHeight);

            Color noteColor = GetNoteColor(note);

            // Draw hold tail first (behind the note head)
            if (note.noteType == NoteType.Hold && note.holdDuration > 0f)
            {
                float endY = TimeToY(note.timestamp + note.holdDuration);
                float tailHeight = endY - y;

                float tailMargin = noteMarginH + (laneWidth - noteMarginH * 2f) * 0.15f;
                Rect holdRect = new Rect(note.laneIndex * laneWidth + tailMargin, y, laneWidth - tailMargin * 2f, tailHeight);
                Color holdColor = noteColor;
                holdColor.a = 0.35f;
                EditorGUI.DrawRect(holdRect, holdColor);

                // Draw hold end cap
                float capMargin = noteMarginH + 2f;
                Rect endCapRect = new Rect(note.laneIndex * laneWidth + capMargin, endY - halfH * 0.5f, laneWidth - capMargin * 2f, noteHeight * 0.5f);
                Color endCapColor = noteColor;
                endCapColor.a = 0.6f;
                EditorGUI.DrawRect(endCapRect, endCapColor);
            }

            // Highlight selected notes
            if (selectedNotes.Contains(note))
            {
                EditorGUI.DrawRect(new Rect(noteRect.x - 2, noteRect.y - 2, noteRect.width + 4, noteRect.height + 4), Color.white);
            }

            EditorGUI.DrawRect(noteRect, noteColor);

            // Draw note info (only when zoomed in enough to read)
            if (zoom >= 0.4f)
            {
                string noteLabel = note.laneIndex <= 2 ? note.stickDirection.ToString() : note.buttonRow.ToString();
                if (note.noteType == NoteType.Hold)
                    noteLabel += " H";
                GUI.Label(noteRect, noteLabel, EditorStyles.centeredGreyMiniLabel);
            }
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
        EditorGUI.DrawRect(selRect, new Color(0.3f, 0.6f, 1f, 0.15f));
        EditorGUI.DrawRect(new Rect(selRect.x, selRect.y, selRect.width, 1), new Color(0.3f, 0.6f, 1f, 0.8f));
        EditorGUI.DrawRect(new Rect(selRect.x, selRect.yMax - 1, selRect.width, 1), new Color(0.3f, 0.6f, 1f, 0.8f));
        EditorGUI.DrawRect(new Rect(selRect.x, selRect.y, 1, selRect.height), new Color(0.3f, 0.6f, 1f, 0.8f));
        EditorGUI.DrawRect(new Rect(selRect.xMax - 1, selRect.y, 1, selRect.height), new Color(0.3f, 0.6f, 1f, 0.8f));
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
        if (currentSong == null || currentSong.bpm <= 0) return 0;
        float beats = currentSong.TimeToBeat(time);
        return beats * beatHeight * zoom;
    }

    float YToTime(float y)
    {
        if (currentSong == null || currentSong.bpm <= 0) return 0;
        float beats = y / (beatHeight * zoom);
        return currentSong.BeatToTime(beats);
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
        if (currentSong == null || currentSong.bpm <= 0) return;
        // Use the BPM at the current time for seeking
        float activeBpm = currentSong.GetBpmAtTime((float)songTime);
        float secondsPerBeat = 60f / activeBpm;
        SeekTo((float)songTime + beats * secondsPerBeat);
    }

    void HandleInput(Rect timelineRect)
    {
        Event e = Event.current;

        Rect visibleContentRect = new Rect(scrollPosition.x, scrollPosition.y, timelineRect.width, timelineRect.height);
        if (!visibleContentRect.Contains(e.mousePosition))
            return;

        // Scroll wheel zoom (Ctrl+Scroll)
        if (e.type == EventType.ScrollWheel && e.control)
        {
            float zoomDelta = -e.delta.y * 0.05f * zoom;
            zoom = Mathf.Clamp(zoom + zoomDelta, 0.1f, 10f);
            e.Use();
            Repaint();
            return;
        }

        // Middle-click to seek playhead
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

        // Normal click — place or delete notes
        if (!e.shift && e.type == EventType.MouseDown && e.button == 0)
        {
            if (selectedNotes.Count > 0 && !e.control)
            {
                selectedNotes.Clear();
            }

            int clickedLane = Mathf.FloorToInt(e.mousePosition.x / laneWidth);
            float clickedTime = YToTime(e.mousePosition.y);

            // Scale click tolerance with zoom — tighter tolerance when zoomed in
            float clickTolerance = Mathf.Clamp(0.2f / zoom, 0.02f, 0.5f);

            Debug.Log($"Content Mouse: {e.mousePosition}, Lane: {clickedLane}, Time: {clickedTime:F2}s");

            if (clickedLane >= 0 && clickedLane < 6)
            {
                NoteData clickedNote = FindNoteAt(clickedLane, clickedTime, clickTolerance);

                if (clickedNote != null && e.control)
                {
                    DeleteNote(clickedNote);
                    e.Use();
                    Repaint();
                }
                else if (clickedNote == null)
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
            float clickTolerance = Mathf.Clamp(0.2f / zoom, 0.02f, 0.5f);

            NoteData clickedNote = FindNoteAt(clickedLane, clickedTime, clickTolerance);
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

        // Ctrl+C — Copy
        if (e.control && e.keyCode == KeyCode.C && selectedNotes.Count > 0)
        {
            CopySelection();
            e.Use();
        }
        // Ctrl+V — Paste
        else if (e.control && e.keyCode == KeyCode.V && clipboard.Count > 0)
        {
            PasteAtPlayhead();
            e.Use();
        }
        // Ctrl+A — Select all visible notes
        else if (e.control && e.keyCode == KeyCode.A)
        {
            SelectAllNotes();
            e.Use();
        }
        // Delete / Backspace
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
        // Arrow keys — seek
        else if (e.keyCode == KeyCode.LeftArrow)
        {
            SeekByBeats(-1);
            e.Use();
        }
        else if (e.keyCode == KeyCode.RightArrow)
        {
            SeekByBeats(1);
            e.Use();
        }
        // Space — toggle playback
        else if (e.keyCode == KeyCode.Space)
        {
            TogglePlayback();
            e.Use();
        }
        // + / - to adjust zoom quickly
        else if (e.keyCode == KeyCode.Equals || e.keyCode == KeyCode.KeypadPlus)
        {
            zoom = Mathf.Clamp(zoom * 1.25f, 0.1f, 10f);
            Repaint();
            e.Use();
        }
        else if (e.keyCode == KeyCode.Minus || e.keyCode == KeyCode.KeypadMinus)
        {
            zoom = Mathf.Clamp(zoom * 0.8f, 0.1f, 10f);
            Repaint();
            e.Use();
        }
    }

    void SelectAllNotes()
    {
        if (currentChart == null || currentChart.notes == null) return;
        selectedNotes.Clear();
        selectedNotes.AddRange(currentChart.notes);
        Debug.Log($"Selected all {selectedNotes.Count} notes");
        Repaint();
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

        float earliestTime = selectedNotes.Min(n => n.timestamp);

        foreach (NoteData note in selectedNotes)
        {
            clipboard.Add(new NoteData
            {
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

        float pasteTime = SnapTimeToGrid((float)songTime);

        Undo.RecordObject(currentChart, "Paste Notes");
        selectedNotes.Clear();

        foreach (NoteData clipNote in clipboard)
        {
            int absoluteLane = clipNote.laneIndex;
            float absoluteTime = pasteTime + clipNote.timestamp;

            if (absoluteLane < 0 || absoluteLane > 5) continue;

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
        Undo.RecordObject(currentChart, "Delete Selected Notes");

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
        Undo.RecordObject(currentChart, "Delete Note");
        currentChart.notes.Remove(note);
        selectedNotes.Remove(note);
        EditorUtility.SetDirty(currentChart);
        Debug.Log($"Deleted note at {note.timestamp:F2}s in lane {note.laneIndex}");
    }

    void PlaceNote(int lane, float time)
    {
        // Snap to grid in beat-space so it works correctly across BPM changes
        time = SnapTimeToGrid(time);

        Undo.RecordObject(currentChart, "Place Note");

        NoteData newNote = new NoteData
        {
            timestamp = time,
            laneIndex = lane,
            noteType = selectedNoteType,
            stickDirection = selectedStickDirection,
            buttonRow = selectedButtonRow,
            holdDuration = selectedNoteType == NoteType.Hold ? selectedHoldDuration : 0f
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

    void CreateNewSongContainer()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Song Container", "NewSong", "asset", "Create a new song container file");
        if (!string.IsNullOrEmpty(path))
        {
            SongContainer newSong = CreateInstance<SongContainer>();
            newSong.bpm = 120f;
            AssetDatabase.CreateAsset(newSong, path);
            AssetDatabase.SaveAssets();
            currentSong = newSong;
        }
    }

    void CreateNewChart()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Chart", "NewChart", "asset", "Create a new chart data file");
        if (!string.IsNullOrEmpty(path))
        {
            ChartData newChart = CreateInstance<ChartData>();
            AssetDatabase.CreateAsset(newChart, path);
            AssetDatabase.SaveAssets();
            currentChart = newChart;
        }
    }
}


























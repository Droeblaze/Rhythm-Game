using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject notePrefab;
    public Transform[] lanePositions;
    public Transform judgementLine;
    public InputManager inputManager;

    [Header("Spawning Settings")]
    public float scrollSpeed = 5f;
    public float spawnDistance = 10f;
    public float audioStartDelay = 3f;

    [Header("Audio Calibration")]
    [Tooltip("Positive values delay audio (notes come earlier), negative values advance audio (notes come later). Measured in seconds.")]
    public float audioOffset = 0f;

    [Header("Song & Chart Data")]
    public SongContainer songContainer;
    public ChartData chartData;
    public bool loadFromSelection = true;

    [Header("Scene Transition")]
    public string resultsSceneName = "Results";
    public float delayAfterSongEnd = 2f;

    private AudioSource audioSource;
    private float songTime = 0f;
    private int nextNoteIndex = 0;
    private List<GameObject> activeNotes = new List<GameObject>();
    private bool audioStarted = false;
    private bool songFinished = false;

    // Fix 1: snapshot-based countdown to avoid deltaTime accumulation drift
    private float countdownStartTime = -1f;

    // Fix 3: DSP time tracking for sub-frame accurate song clock
    private double dspTimeAtStart = 0;
    private double lastKnownDspTime = 0;
    private float realtimeAtLastDspTick = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Load settings from SettingsManager
        scrollSpeed = SettingsManager.GetScrollSpeed();
        audioOffset = SettingsManager.GetAudioOffset();

        // Load selected song and chart if enabled
        if (loadFromSelection)
        {
            SongContainer selectedSong = ChartSelectionManager.LoadSelectedSong();
            ChartData selectedChart = ChartSelectionManager.LoadSelectedChart();

            if (selectedSong != null)
            {
                songContainer = selectedSong;
            }
            if (selectedChart != null)
            {
                chartData = selectedChart;
            }

            // Store song info in ScoreManager for the results screen
            if (ScoreManager.Instance != null && songContainer != null && chartData != null)
            {
                ScoreManager.Instance.SetSongInfo(songContainer.title, chartData.difficulty);
            }
        }

        if (songContainer == null)
        {
            Debug.LogError("No song container loaded!");
            return;
        }

        if (chartData == null || chartData.notes.Count == 0)
        {
            Debug.LogError("No chart data loaded!");
            return;
        }

        if (inputManager == null)
        {
            inputManager = FindObjectOfType<InputManager>();
        }

        // Load and prepare the audio clip from the SongContainer
        if (songContainer.songAudio != null)
        {
            audioSource.clip = songContainer.songAudio;
            audioSource.playOnAwake = false;

            // Pre-load audio by playing and immediately stopping
            audioSource.Play();
            audioSource.Stop();
        }
        else
        {
            Debug.LogError("No audio clip assigned in SongContainer!");
        }

        // Sort notes by timestamp
        chartData.SortNotes();

        audioStarted = false;
        songFinished = false;
    }

    void Update()
    {
        if (chartData == null) return;

        // Handle countdown before audio starts
        if (!audioStarted)
        {
            // Fix 1: use a start-time snapshot instead of accumulating deltaTime
            if (countdownStartTime < 0f)
                countdownStartTime = Time.realtimeSinceStartup;

            float elapsed = Time.realtimeSinceStartup - countdownStartTime;

            // Song time is negative during countdown
            songTime = elapsed - audioStartDelay;

            // Start audio when countdown finishes, accounting for audio offset
            if (elapsed >= audioStartDelay + audioOffset)
            {
                StartSong();
                audioStarted = true;
            }
        }
        else
        {
            // Fix 3: interpolate between DSP ticks using realtime for smooth per-frame movement
            // while staying anchored to the accurate DSP clock.
            double currentDsp = AudioSettings.dspTime;
            if (currentDsp != lastKnownDspTime)
            {
                lastKnownDspTime = currentDsp;
                realtimeAtLastDspTick = Time.realtimeSinceStartup;
            }

            float timeSinceLastTick = Time.realtimeSinceStartup - realtimeAtLastDspTick;
            songTime = (float)(lastKnownDspTime - dspTimeAtStart) + timeSinceLastTick + audioOffset;

            // Check if song has finished
            if (!songFinished && audioSource != null && !audioSource.isPlaying && audioStarted)
            {
                OnSongFinished();
            }
        }

        // Check if we need to spawn the next note
        SpawnNotes();

        // Move all active notes down
        MoveNotes();
    }

    void SpawnNotes()
    {
        // Calculate how far ahead we need to spawn notes
        float spawnTime = songTime + (spawnDistance / scrollSpeed);

        // Spawn all notes that should appear now
        while (nextNoteIndex < chartData.notes.Count)
        {
            NoteData noteData = chartData.notes[nextNoteIndex];

            if (noteData.timestamp <= spawnTime)
            {
                SpawnNote(noteData);
                nextNoteIndex++;
            }
            else
            {
                break;
            }
        }
    }

    void SpawnNote(NoteData noteData)
    {
        if (noteData.laneIndex < 0 || noteData.laneIndex >= lanePositions.Length)
        {
            Debug.LogError($"Invalid lane index: {noteData.laneIndex}");
            return;
        }

        Vector3 spawnPosition = lanePositions[noteData.laneIndex].position;
        spawnPosition.y = judgementLine.position.y + spawnDistance;

        GameObject noteObject = Instantiate(notePrefab, spawnPosition, Quaternion.identity);

        NoteVisual noteVisual = noteObject.GetComponent<NoteVisual>();
        if (noteVisual != null)
        {
            noteVisual.Initialize(noteData);
        }

        activeNotes.Add(noteObject);
    }

    void MoveNotes()
    {
        // Use the largest miss window to determine when a note is truly missed
        float maxMissDistance = Mathf.Max(
            inputManager != null ? inputManager.missWindow : 0.2f,
            inputManager != null ? inputManager.stickMissWindow : 0.3f
        ) * scrollSpeed;

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            if (activeNotes[i] == null)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            NoteVisual noteVisual = activeNotes[i].GetComponent<NoteVisual>();

            // If this hold note is actively being held, pin it at the judgement line.
            // The tail shrinks from the top in NoteVisual.Update() as holdTimeRemaining decreases.
            if (noteVisual != null && noteVisual.isHoldActive)
            {
                Vector3 pinnedPos = activeNotes[i].transform.position;
                pinnedPos.y = judgementLine.position.y;
                activeNotes[i].transform.position = pinnedPos;
                continue;
            }

            // If the hold has finished (complete, failed, or missed), the note resumes scrolling
            // downward so the remaining tail visually drains past the judgement line.
            if (noteVisual != null && noteVisual.holdFinished)
            {
                activeNotes[i].transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

                // Use the authoritative tailTopWorldY from NoteVisual, which tracks
                // the frozen world-space top even after the tail GameObject is deactivated.
                float topY = noteVisual.tailTopWorldY;

                // Destroy once the tail top has scrolled well below the judgement line
                if (topY < judgementLine.position.y - 3f)
                {
                    Destroy(activeNotes[i]);
                    activeNotes.RemoveAt(i);
                }
                continue;
            }

            // Fix 2: pin note Y directly to the song clock instead of integrating deltaTime,
            // keeping visual position perfectly in sync with audio regardless of frame rate.
            if (noteVisual != null && noteVisual.data != null)
            {
                float timeUntilHit = noteVisual.data.timestamp - songTime;
                Vector3 pos = activeNotes[i].transform.position;
                pos.y = judgementLine.position.y + (timeUntilHit * scrollSpeed);
                activeNotes[i].transform.position = pos;
            }
            else
            {
                // Fallback for notes without NoteVisual
                activeNotes[i].transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
            }

            // Only mark as miss once the note has passed beyond the miss window
            float distancePastLine = judgementLine.position.y - activeNotes[i].transform.position.y;
            if (distancePastLine > maxMissDistance)
            {
                if (noteVisual != null)
                {
                    noteVisual.MarkAsMiss();
                }
                // Don't RemoveAt here for hold notes — MarkAsMiss sets holdFinished
                // and the holdFinished branch above will handle cleanup after scrolling off.
                if (noteVisual == null || !noteVisual.holdFinished)
                {
                    activeNotes.RemoveAt(i);
                }
            }
        }
    }

    public void StartSong()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            // Fix 3: schedule playback at the current DSP time and record it
            // so songTime can be derived from the same high-resolution clock.
            dspTimeAtStart = AudioSettings.dspTime;
            audioSource.PlayScheduled(dspTimeAtStart);
        }
    }

    void OnSongFinished()
    {
        songFinished = true;
        Debug.Log("Song finished! Transitioning to results screen...");
        StartCoroutine(TransitionToResults());
    }

    IEnumerator TransitionToResults()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayAfterSongEnd);

        // Load the results scene
        SceneManager.LoadScene(resultsSceneName);
    }
}

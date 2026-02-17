using UnityEngine;
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

    [Header("Chart Data")]
    public ChartData chartData;

    private AudioSource audioSource;
    private float songTime = 0f;
    private int nextNoteIndex = 0;
    private List<GameObject> activeNotes = new List<GameObject>();
    private float countdownTimer = 0f;
    private bool audioStarted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (chartData == null || chartData.notes.Count == 0)
        {
            Debug.LogError("No chart data loaded!");
            return;
        }

        if (inputManager == null)
        {
            inputManager = FindObjectOfType<InputManager>();
        }

        // Load and prepare the audio clip
        if (chartData.songAudio != null)
        {
            audioSource.clip = chartData.songAudio;
            audioSource.playOnAwake = false;

            // Pre-load audio by playing and immediately stopping
            audioSource.Play();
            audioSource.Stop();
        }
        else
        {
            Debug.LogError("No audio clip assigned in ChartData!");
        }

        // Sort notes by timestamp
        chartData.SortNotes();

        countdownTimer = 0f;
        audioStarted = false;
    }

    void Update()
    {
        if (chartData == null) return;

        // Handle countdown before audio starts
        if (!audioStarted)
        {
            countdownTimer += Time.deltaTime;

            // Song time is negative during countdown
            songTime = countdownTimer - audioStartDelay;

            // Start audio when countdown finishes
            if (countdownTimer >= audioStartDelay)
            {
                StartSong();
                audioStarted = true;
            }
        }
        else
        {
            // CRITICAL: Use audioSource.time for accurate rhythm sync
            songTime = audioSource.time;
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

            activeNotes[i].transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

            // Only mark as miss once the note has passed beyond the miss window
            float distancePastLine = judgementLine.position.y - activeNotes[i].transform.position.y;
            if (distancePastLine > maxMissDistance)
            {
                NoteVisual noteVisual = activeNotes[i].GetComponent<NoteVisual>();
                if (noteVisual != null)
                {
                    noteVisual.MarkAsMiss();
                }
                activeNotes.RemoveAt(i);
            }
        }
    }

    public void StartSong()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSong", menuName = "Rhythm Game/Song Container")]
public class SongContainer : ScriptableObject
{
    [Header("Song Info")]
    public string title;
    public string artist;
    public Sprite songArt;

    [Header("Audio")]
    public AudioClip songAudio;

    [Header("Timing")]
    public float bpm;
    public List<BpmChangeData> bpmChanges = new List<BpmChangeData>();

    [Header("Charts (one per difficulty)")]
    public ChartData[] charts;

    /// <summary>
    /// Returns charts sorted by difficulty rating (lowest first).
    /// </summary>
    public ChartData[] GetSortedByDifficulty()
    {
        ChartData[] sorted = (ChartData[])charts.Clone();
        System.Array.Sort(sorted, (a, b) => a.difficulty.CompareTo(b.difficulty));
        return sorted;
    }

    public void SortBpmChanges()
    {
        bpmChanges.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
    }

    /// <summary>
    /// Returns the active BPM at a given time, considering all BPM changes.
    /// </summary>
    public float GetBpmAtTime(float time)
    {
        float currentBpm = bpm;
        for (int i = 0; i < bpmChanges.Count; i++)
        {
            if (bpmChanges[i].timestamp <= time)
                currentBpm = bpmChanges[i].bpm;
            else
                break;
        }
        return currentBpm;
    }

    /// <summary>
    /// Converts a timestamp (seconds) to a beat position, accounting for BPM changes.
    /// </summary>
    public float TimeToBeat(float time)
    {
        float beats = 0f;
        float prevTime = 0f;
        float currentBpm = bpm;

        for (int i = 0; i < bpmChanges.Count; i++)
        {
            if (bpmChanges[i].timestamp >= time) break;

            float segmentEnd = bpmChanges[i].timestamp;
            beats += (segmentEnd - prevTime) * (currentBpm / 60f);
            prevTime = segmentEnd;
            currentBpm = bpmChanges[i].bpm;
        }

        beats += (time - prevTime) * (currentBpm / 60f);
        return beats;
    }

    /// <summary>
    /// Converts a beat position back to a timestamp (seconds), accounting for BPM changes.
    /// </summary>
    public float BeatToTime(float beat)
    {
        float remainingBeats = beat;
        float prevTime = 0f;
        float currentBpm = bpm;

        for (int i = 0; i < bpmChanges.Count; i++)
        {
            float segmentBeats = (bpmChanges[i].timestamp - prevTime) * (currentBpm / 60f);

            if (remainingBeats <= segmentBeats)
            {
                return prevTime + remainingBeats / (currentBpm / 60f);
            }

            remainingBeats -= segmentBeats;
            prevTime = bpmChanges[i].timestamp;
            currentBpm = bpmChanges[i].bpm;
        }

        return prevTime + remainingBeats / (currentBpm / 60f);
    }
}
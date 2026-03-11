using UnityEngine;
using System.Collections.Generic;

public class DifficultyCalculator
{
    private const float MIXED_NOTE_MULTIPLIER = 1.05f; // 5% increase when both note types are used
    private const float DURATION_MULTIPLIER_PER_MINUTE = 1.02f; // 2% increase per 20 seconds of song length

    /// <summary>
    /// Calculates the difficulty rating for a chart.
    /// </summary>
    public static float CalculateDifficulty(ChartData chart)
    {
        if (chart == null || chart.notes == null || chart.notes.Count == 0)
        {
            return 0f;
        }

        // Ensure notes are sorted
        chart.SortNotes();

        // Find the duration of the song (last note timestamp)
        float songDuration = GetSongDuration(chart);
        if (songDuration <= 0f)
        {
            return 0f;
        }

        // Calculate difficulty for each second slice
        List<float> sliceDifficulties = new List<float>();
        int totalSeconds = Mathf.CeilToInt(songDuration);

        for (int second = 0; second < totalSeconds; second++)
        {
            float sliceStart = second;
            float sliceEnd = second + 1f;

            float stickNotesCount = CountNotesInRange(chart, sliceStart, sliceEnd, true);
            float buttonNotesCount = CountNotesInRange(chart, sliceStart, sliceEnd, false);

            // Calculate base difficulty for this slice (notes per second)
            float sliceDifficulty = stickNotesCount + buttonNotesCount;

            // Apply mixed note type multiplier if both types are present
            if (stickNotesCount > 0 && buttonNotesCount > 0)
            {
                sliceDifficulty *= MIXED_NOTE_MULTIPLIER;
            }

            sliceDifficulties.Add(sliceDifficulty);
        }

        // Calculate average difficulty across all slices
        float averageDifficulty = CalculateAverage(sliceDifficulties);

        // Apply duration-based multiplier
        float durationMinutes = songDuration / 20f;
        float durationMultiplier = Mathf.Pow(DURATION_MULTIPLIER_PER_MINUTE, durationMinutes);
        float finalDifficulty = averageDifficulty * durationMultiplier;

        return finalDifficulty;
    }

    /// <summary>
    /// Gets the duration of the song based on the last note timestamp.
    /// </summary>
    private static float GetSongDuration(ChartData chart)
    {
        if (chart.notes.Count == 0)
        {
            return 0f;
        }

        float lastNoteTime = chart.notes[chart.notes.Count - 1].timestamp;

        // If the last note is a hold note, add its duration
        NoteData lastNote = chart.notes[chart.notes.Count - 1];
        if (lastNote.noteType == NoteType.Hold)
        {
            lastNoteTime += lastNote.holdDuration;
        }

        return lastNoteTime;
    }

    /// <summary>
    /// Counts the number of notes in a time range, filtered by note type.
    /// Notes with "Both" type (ButtonRow.Both or StickDirection.UpDown) count as 2 notes.
    /// </summary>
    private static float CountNotesInRange(ChartData chart, float startTime, float endTime, bool countStickNotes)
    {
        float count = 0f;

        foreach (NoteData note in chart.notes)
        {
            // Skip notes outside the time range
            if (note.timestamp < startTime || note.timestamp >= endTime)
            {
                continue;
            }

            // Filter by note type
            bool isStickNote = note.IsStickLane();
            if (countStickNotes && isStickNote)
            {
                // Stick notes with UpDown direction count as 2 notes
                if (note.stickDirection == StickDirection.UpDown)
                {
                    count += 2f;
                }
                else
                {
                    count += 1f;
                }
            }
            else if (!countStickNotes && !isStickNote)
            {
                // Button notes with Both row count as 2 notes
                if (note.buttonRow == ButtonRow.Both)
                {
                    count += 2f;
                }
                else
                {
                    count += 1f;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Calculates the average of a list of values.
    /// </summary>
    private static float CalculateAverage(List<float> values)
    {
        if (values.Count == 0)
        {
            return 0f;
        }

        float sum = 0f;
        foreach (float value in values)
        {
            sum += value;
        }

        return sum / values.Count;
    }

    /// <summary>
    /// Calculates and updates the difficulty for a chart, storing it in the chart data.
    /// </summary>
    public static void UpdateChartDifficulty(ChartData chart)
    {
        if (chart != null)
        {
            chart.difficulty = CalculateDifficulty(chart);
            Debug.Log($"Chart difficulty calculated: {chart.difficulty:F2}");
        }
    }
}
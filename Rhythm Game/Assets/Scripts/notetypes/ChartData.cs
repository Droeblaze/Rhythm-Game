using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChart", menuName = "Rhythm Game/Chart")]
public class ChartData : ScriptableObject
{
    [Header("Difficulty")]
    [Tooltip("Automatically calculated difficulty rating (notes per second with modifiers)")]
    public float difficulty;

    [Header("Note Data")]
    public List<NoteData> notes = new List<NoteData>();

    // Sort notes by timestamp for efficient processing
    public void SortNotes()
    {
        notes.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
    }

    /// <summary>
    /// Calculates and updates the difficulty rating for this chart.
    /// Right-click the ChartData asset and select "Calculate Difficulty" to update.
    /// </summary>
    [ContextMenu("Calculate Difficulty")]
    public void CalculateDifficulty()
    {
        DifficultyCalculator.UpdateChartDifficulty(this);

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
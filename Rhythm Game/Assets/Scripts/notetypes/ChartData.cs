using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChart", menuName = "Rhythm Game/Chart")]
public class ChartData : ScriptableObject
{
    public AudioClip songAudio;
    public float bpm;
    public List<NoteData> notes = new List<NoteData>();

    // Sort notes by timestamp for efficient processing
    public void SortNotes()
    {
        notes.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
    }
}
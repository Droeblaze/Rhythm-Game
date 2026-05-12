using UnityEngine;

[System.Serializable]
public class BpmChangeData
{
    public float timestamp; // Time in seconds where the BPM changes
    public float bpm;       // New BPM from this point onward
}
using UnityEngine;

public enum NoteType { Tap, Hold }
public enum LaneType { StickLeft, StickVertical, StickRight, ButtonA, ButtonB, ButtonC }
public enum StickDirection { Up, Horizontal, Down, UpDown }
public enum ButtonRow { Top, Bottom, Both }

[System.Serializable]
public class NoteData
{
    public float timestamp;
    public NoteType noteType;
    public int laneIndex;  // 0-5

    // Only show these for stick lanes (0-2)
    [Tooltip("For stick lanes (0-2) only")]
    public StickDirection stickDirection = StickDirection.Horizontal;

    // Only show these for button lanes (3-5)
    [Tooltip("For button lanes (3-5) only")]
    public ButtonRow buttonRow = ButtonRow.Top;

    public float holdDuration;

    // Helper to check if this is a stick lane
    public bool IsStickLane() => laneIndex >= 0 && laneIndex <= 2;

    // Helper to check if this is a button lane
    public bool IsButtonLane() => laneIndex >= 3 && laneIndex <= 5;
}
using UnityEngine;

public class LaneNote : MonoBehaviour
{
    public enum LaneType { LeftStick, Buttons }
    public enum StickDir { Left, Up, Right, Down }
    public enum FaceButton { A, B, X, Y }

    [Header("What lane does this note belong to?")]
    public LaneType lane;

    [Header("Pick ONE depending on lane")]
    public StickDir stickDir;       // used when lane = LeftStick
    public FaceButton faceButton;   // used when lane = Buttons

    [HideInInspector] public bool hit;   // gameplay state (later)

    // This is the “role” of the note in the game
    public int GetLaneColumnId()
    {
        // Gives you a consistent integer key to store notes in lists
        if (lane == LaneType.LeftStick) return (int)stickDir;      // 0-3
        else return (int)faceButton;                               // 0-3
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HitRouter : MonoBehaviour
{
    [Header("Lane Parents")]
    public Transform leftLane;
    public Transform buttonLane;

    [Header("Hit Lines")]
    public Transform hitLineLeft;
    public Transform hitLineRight;

    [Header("Hit Settings")]
    public float hitWindow = 0.6f;          // how close you must be to hit
    public float missAfter = 0.8f;          // how far past the hit line becomes a MISS (below)

    private List<LaneNote> allNotes = new List<LaneNote>();

    void Start()
    {
        CollectNotes();
    }

    void CollectNotes()
    {
        allNotes.Clear();

        if (leftLane != null)
            allNotes.AddRange(leftLane.GetComponentsInChildren<LaneNote>());

        if (buttonLane != null)
            allNotes.AddRange(buttonLane.GetComponentsInChildren<LaneNote>());

        Debug.Log("Collected " + allNotes.Count + " notes.");
    }

    void Update()
    {
        var gp = Gamepad.current;

        // Auto-miss runs even if controller disconnects
        AutoMissNotes();

        if (gp == null) return;

        // ===== LEFT STICK INPUTS =====
        if (gp.leftStick.left.wasPressedThisFrame)  TryHit(LaneNote.LaneType.LeftStick, 0);
        if (gp.leftStick.up.wasPressedThisFrame)    TryHit(LaneNote.LaneType.LeftStick, 1);
        if (gp.leftStick.right.wasPressedThisFrame) TryHit(LaneNote.LaneType.LeftStick, 2);
        if (gp.leftStick.down.wasPressedThisFrame)  TryHit(LaneNote.LaneType.LeftStick, 3);

        // ===== ABXY INPUTS =====
        if (gp.buttonSouth.wasPressedThisFrame) TryHit(LaneNote.LaneType.Buttons, 0); // A
        if (gp.buttonEast.wasPressedThisFrame)  TryHit(LaneNote.LaneType.Buttons, 1); // B
        if (gp.buttonWest.wasPressedThisFrame)  TryHit(LaneNote.LaneType.Buttons, 2); // X
        if (gp.buttonNorth.wasPressedThisFrame) TryHit(LaneNote.LaneType.Buttons, 3); // Y
    }

    void AutoMissNotes()
    {
        // Left lane miss threshold
        float leftMissY = hitLineLeft.position.y - missAfter;
        float rightMissY = hitLineRight.position.y - missAfter;

        foreach (var n in allNotes)
        {
            if (n == null || n.hit) continue;
            if (!n.gameObject.activeInHierarchy) continue;

            float missY = (n.lane == LaneNote.LaneType.LeftStick) ? leftMissY : rightMissY;

            // if note passed below miss threshold, count as miss
            if (n.transform.position.y < missY)
            {
                n.hit = true; // consume so it can't be hit later
                Debug.Log($"AUTO MISS: {n.lane} col {n.GetLaneColumnId()} -> {n.name}");
                n.gameObject.SetActive(false);
            }
        }
    }

    void TryHit(LaneNote.LaneType lane, int columnId)
    {
        float targetY = (lane == LaneNote.LaneType.LeftStick)
            ? hitLineLeft.position.y
            : hitLineRight.position.y;

        LaneNote best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var n in allNotes)
        {
            if (n == null || n.hit) continue;
            if (!n.gameObject.activeInHierarchy) continue;

            if (n.lane != lane) continue;
            if (n.GetLaneColumnId() != columnId) continue;

            float dist = Mathf.Abs(n.transform.position.y - targetY);

            // Only allow hits inside the window
            if (dist > hitWindow) continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = n;
            }
        }

        if (best != null)
        {
            best.hit = true;
            Debug.Log($"HIT: {lane} col {columnId} (dist {bestDist:F3}) -> {best.name}");
            best.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"MISS: {lane} col {columnId}");
        }
    }
}
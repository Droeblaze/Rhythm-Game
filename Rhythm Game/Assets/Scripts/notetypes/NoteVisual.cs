using UnityEngine;

public class NoteVisual : MonoBehaviour
{
    public NoteData data;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer directionIndicator;  // Shows the specific direction

    [Header("Hold Note Components")]
    public GameObject holdTail;
    public SpriteRenderer holdTailRenderer;

    [Header("Base Note Sprites - by Lane")]
    public Sprite leftStickBaseSprite;      // Lane 0 base
    public Sprite upStickBaseSprite;        // Lane 1 base
    public Sprite rightStickBaseSprite;     // Lane 2 base
    public Sprite topButtonSprite;          // Lane 3
    public Sprite middleButtonSprite;       // Lane 4
    public Sprite bottomButtonSprite;       // Lane 5

    [Header("Direction Indicator Sprites - for Stick Lanes")]
    public Sprite upLeftArrow;
    public Sprite leftArrow;
    public Sprite downLeftArrow;
    public Sprite upArrow;
    public Sprite downArrow;
    public Sprite upRightArrow;
    public Sprite rightArrow;
    public Sprite downRightArrow;

    [Header("Hold Tail Sprite")]
    public Sprite holdTailSprite;

    private bool isHit = false;
    private bool isActive = true;

    // Hold note state
    [HideInInspector] public bool isHoldActive = false;   // Currently being held
    [HideInInspector] public float holdTimeRemaining = 0f; // Seconds left to hold
    [HideInInspector] public bool holdCompleted = false;   // Successfully held to end
    [HideInInspector] public bool holdFinished = false;    // Hold ended (complete or failed), tail scrolling off

    // Tracks the world-space Y of the tail's top so NoteSpawner knows
    // when the entire visual has scrolled off-screen, even after the
    // holdTail GameObject has been deactivated.
    [HideInInspector] public float tailTopWorldY = 0f;

    private float initialTailLength = 0f;
    private float scrollSpeed = 5f;
    private Transform judgementLine;

    // When the hold ends, we store the tail top as a LOCAL offset from the head,
    // so it scrolls down with the note naturally.
    private float frozenTailTopLocalOffset = 0f;

    public void Initialize(NoteData noteData)
    {
        data = noteData;

        // Get scroll speed from NoteSpawner if available
        NoteSpawner spawner = FindObjectOfType<NoteSpawner>();
        if (spawner != null)
        {
            scrollSpeed = spawner.scrollSpeed;
            judgementLine = spawner.judgementLine;
        }

        SetupVisuals();
    }

    private void SetupVisuals()
    {
        // Set base sprite based on lane
        if (data.laneIndex >= 0 && data.laneIndex <= 2)
        {
            // Stick lanes - use lane-specific sprites
            switch (data.laneIndex)
            {
                case 0: spriteRenderer.sprite = leftStickBaseSprite; break;
                case 1: spriteRenderer.sprite = upStickBaseSprite; break;
                case 2: spriteRenderer.sprite = rightStickBaseSprite; break;
            }

            // Show direction indicator for stick lanes
            SetupStickDirectionIndicator();
        }
        else if (data.laneIndex >= 3 && data.laneIndex <= 5)
        {
            // Button lanes - use buttonRow to determine sprite
            switch (data.buttonRow)
            {
                case ButtonRow.Top: spriteRenderer.sprite = topButtonSprite; break;
                case ButtonRow.Bottom: spriteRenderer.sprite = bottomButtonSprite; break;
                case ButtonRow.Both: spriteRenderer.sprite = middleButtonSprite; break;
            }

            // Hide direction indicator for button lanes
            if (directionIndicator != null)
                directionIndicator.gameObject.SetActive(false);
        }

        // Setup hold tail if it's a hold note
        if (data.noteType == NoteType.Hold)
        {
            holdTimeRemaining = data.holdDuration;
            SetupHoldTail();
        }
        else
        {
            if (holdTail != null)
                holdTail.SetActive(false);
        }
    }

    private void SetupStickDirectionIndicator()
    {
        if (directionIndicator == null) return;

        directionIndicator.gameObject.SetActive(true);

        // Map stick direction to arrow sprite based on lane
        if (data.laneIndex == 0) // Left Stick
        {
            switch (data.stickDirection)
            {
                case StickDirection.Up: directionIndicator.sprite = upLeftArrow; break;
                case StickDirection.Horizontal: directionIndicator.sprite = leftArrow; break;
                case StickDirection.Down: directionIndicator.sprite = downLeftArrow; break;
            }
        }
        else if (data.laneIndex == 1) // Vertical Stick
        {
            switch (data.stickDirection)
            {
                case StickDirection.Up: directionIndicator.sprite = upArrow; break;
                case StickDirection.UpDown: directionIndicator.sprite = upArrow; break;
                case StickDirection.Down: directionIndicator.sprite = downArrow; break;
            }
        }
        else if (data.laneIndex == 2) // Right Stick
        {
            switch (data.stickDirection)
            {
                case StickDirection.Up: directionIndicator.sprite = upRightArrow; break;
                case StickDirection.Horizontal: directionIndicator.sprite = rightArrow; break;
                case StickDirection.Down: directionIndicator.sprite = downRightArrow; break;
            }
        }
    }

    private void SetupHoldTail()
    {
        if (holdTail == null) return;

        holdTail.SetActive(true);

        // Calculate tail length based on hold duration and scroll speed
        float tailLength = data.holdDuration * scrollSpeed;
        initialTailLength = tailLength;

        // Setup the tail sprite
        if (holdTailRenderer != null && holdTailSprite != null)
        {
            holdTailRenderer.sprite = holdTailSprite;

            // Match the color of the main note but semi-transparent
            Color tailColor = spriteRenderer.color;
            tailColor.a = 0.6f;
            holdTailRenderer.color = tailColor;
        }

        // Scale the tail vertically
        holdTail.transform.localScale = new Vector3(1f, tailLength, 1f);

        // Position so it extends upward from the note (tail is above, note head is at judgement line)
        holdTail.transform.localPosition = new Vector3(0, tailLength / 2f, 0);
    }

    void Update()
    {
        if (holdTail == null || judgementLine == null) return;

        // While the hold is actively being held: the note head is pinned at the
        // judgement line (by NoteSpawner), and the tail shrinks from the top downward.
        if (isHoldActive && holdTimeRemaining > 0f)
        {
            float tailLength = holdTimeRemaining * scrollSpeed;
            holdTail.transform.localScale = new Vector3(1f, Mathf.Max(tailLength, 0f), 1f);
            holdTail.transform.localPosition = new Vector3(0, Mathf.Max(tailLength, 0f) / 2f, 0);
        }

        // After the hold ends (failed or missed), the tail is frozen at a fixed LOCAL offset.
        // Since the head keeps scrolling down, the tail scrolls with it naturally.
        // No recalculation needed — the local position/scale were already set and stay fixed
        // relative to the head as it moves.

        // Update tailTopWorldY for NoteSpawner cleanup checks.
        // This converts the tail's local top position into world space.
        if (holdFinished)
        {
            // The frozen offset is relative to the head — as the head scrolls, this moves down too
            tailTopWorldY = transform.position.y + frozenTailTopLocalOffset;
        }
        else if (holdTail != null && holdTail.activeSelf)
        {
            tailTopWorldY = transform.position.y
                          + holdTail.transform.localPosition.y
                          + (holdTail.transform.localScale.y / 2f);
        }
        else
        {
            tailTopWorldY = transform.position.y;
        }
    }

    private InputManager inputManager;

    void Start()
    {
        // Find the InputManager
        inputManager = FindObjectOfType<InputManager>();

        if (inputManager != null)
        {
            inputManager.RegisterNote(this);
        }
    }

    void OnDestroy()
    {
        // Unregister when destroyed
        if (inputManager != null)
        {
            inputManager.UnregisterNote(this);
        }
    }

    public void MarkAsHit()
    {
        isHit = true;

        // For hold notes, don't destroy yet — start the hold phase
        if (data.noteType == NoteType.Hold)
        {
            isHoldActive = true;
            return;
        }

        isActive = false;
        // Play hit animation
        Destroy(gameObject, 0.2f);
    }

    /// <summary>
    /// Called when the hold is successfully completed.
    /// </summary>
    public void MarkHoldComplete()
    {
        holdCompleted = true;
        isHoldActive = false;
        holdFinished = true;
        isActive = false;

        frozenTailTopLocalOffset = 0f; // Tail fully drained, top is at head

        // The tail was fully drained — hide it
        if (holdTail != null)
            holdTail.SetActive(false);

        // Hide the note head visuals
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (directionIndicator != null)
            directionIndicator.enabled = false;
    }

    /// <summary>
    /// Called when the player releases too early during a hold.
    /// </summary>
    public void MarkHoldFailed()
    {
        isHoldActive = false;
        holdFinished = true;
        isActive = false;

        // Freeze the tail at its current local position/scale.
        // Store the local offset from head to tail top so tailTopWorldY stays accurate.
        float remainingTailLength = holdTimeRemaining * scrollSpeed;
        frozenTailTopLocalOffset = remainingTailLength;

        // The tail's local transform is already correct from the last Update() frame,
        // so it will scroll with the head naturally — no further adjustment needed.

        // Grey out the remaining tail to indicate failure
        if (holdTailRenderer != null)
        {
            holdTailRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
        }

        // Hide the note head visuals
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (directionIndicator != null)
            directionIndicator.enabled = false;
    }

    /// <summary>
    /// Called when a hold note is missed entirely (head passed the judgement line without input).
    /// The whole note + tail should scroll off-screen naturally.
    /// </summary>
    public void MarkAsMiss()
    {
        isActive = false;

        // Notify InputManager about the miss
        if (inputManager != null)
        {
            inputManager.OnNoteMissed(this);
        }

        // For hold notes, let the entire note + tail scroll off-screen instead of destroying
        if (data.noteType == NoteType.Hold && holdTail != null && holdTail.activeSelf)
        {
            holdFinished = true;

            // Store the tail top as a local offset — it scrolls with the head
            frozenTailTopLocalOffset = holdTail.transform.localPosition.y
                                     + (holdTail.transform.localScale.y / 2f);

            // Grey out to indicate miss
            if (holdTailRenderer != null)
                holdTailRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
            if (directionIndicator != null)
                directionIndicator.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);

            // Don't destroy — NoteSpawner will scroll it off and clean up
            return;
        }

        // Non-hold notes destroy as normal
        Destroy(gameObject, 0.2f);
    }
}
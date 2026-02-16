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

    public void Initialize(NoteData noteData)
    {
        data = noteData;
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
                case ButtonRow.Both: spriteRenderer.sprite = middleButtonSprite; break; // Or use a special "both" sprite
            }

            // Hide direction indicator for button lanes
            if (directionIndicator != null)
                directionIndicator.gameObject.SetActive(false);
        }

        // Setup hold tail if it's a hold note
        if (data.noteType == NoteType.Hold)
        {
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
        if (directionIndicator == null) return;  // Removed the stickDirection null check

        directionIndicator.gameObject.SetActive(true);

        // Map stick direction to arrow sprite based on lane
        if (data.laneIndex == 0) // Left Stick
        {
            switch (data.stickDirection)  // Removed .Value
            {
                case StickDirection.Up: directionIndicator.sprite = upLeftArrow; break;
                case StickDirection.Horizontal: directionIndicator.sprite = leftArrow; break;
                case StickDirection.Down: directionIndicator.sprite = downLeftArrow; break;
            }
        }
        else if (data.laneIndex == 1) // Vertical Stick
        {
            switch (data.stickDirection)  // Removed .Value
            {
                case StickDirection.Up: directionIndicator.sprite = upArrow; break;
                case StickDirection.UpDown: directionIndicator.sprite = upArrow; break; // Could use a special double arrow
                case StickDirection.Down: directionIndicator.sprite = downArrow; break;
            }
        }
        else if (data.laneIndex == 2) // Right Stick
        {
            switch (data.stickDirection)  // Removed .Value
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
        float scrollSpeed = 5f; // Match this to your NoteSpawner's scroll speed
        float tailLength = data.holdDuration * scrollSpeed;

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

        // Position so it extends downward from the note
        holdTail.transform.localPosition = new Vector3(0, -tailLength / 2f, 0);
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
        isActive = false;
        // Play hit animation
        Destroy(gameObject, 0.2f);
    }

    public void MarkAsMiss()
    {
        isActive = false;
        // Play miss animation
        Destroy(gameObject, 0.2f);
    }
}
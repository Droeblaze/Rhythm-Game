using UnityEngine;
using UnityEngine.InputSystem;

public class LeftStickDirectionController : MonoBehaviour
{
    private SpriteRenderer theSR;

    public Sprite defaultImage;
    public Sprite pressedImage;

    public StickDirection direction = StickDirection.Left;

    [Range(0.1f, 0.95f)]
    public float threshold = 0.6f;

    public enum StickDirection
    {
        Left,
        Up,
        Right,
        Down
    }

    void Start()
    {
        theSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;

        Vector2 stick = gp.leftStick.ReadValue();

        bool isPressed = direction switch
        {
            StickDirection.Left  => stick.x <= -threshold,
            StickDirection.Right => stick.x >= threshold,
            StickDirection.Up    => stick.y >= threshold,
            StickDirection.Down  => stick.y <= -threshold,
            _ => false
        };

        theSR.sprite = isPressed ? pressedImage : defaultImage;
    }
}
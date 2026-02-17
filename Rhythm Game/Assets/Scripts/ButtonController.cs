using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonController : MonoBehaviour
{
    private SpriteRenderer theSR;

    public Sprite defaultImage;
    public Sprite pressedImage;
    public XboxButton buttonToPress;

    public enum XboxButton
    {
        A, // buttonSouth
        B, // buttonEast
        X, // buttonWest
        Y  // buttonNorth
    }

    void Start()
    {
        theSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;

        bool down = false;
        bool up = false;

        switch (buttonToPress)
        {
            case XboxButton.A:
                down = gp.buttonSouth.wasPressedThisFrame;
                up   = gp.buttonSouth.wasReleasedThisFrame;
                break;

            case XboxButton.B:
                down = gp.buttonEast.wasPressedThisFrame;
                up   = gp.buttonEast.wasReleasedThisFrame;
                break;

            case XboxButton.X:
                down = gp.buttonWest.wasPressedThisFrame;
                up   = gp.buttonWest.wasReleasedThisFrame;
                break;

            case XboxButton.Y:
                down = gp.buttonNorth.wasPressedThisFrame;
                up   = gp.buttonNorth.wasReleasedThisFrame;
                break;
        }

        if (down) theSR.sprite = pressedImage;
        if (up)   theSR.sprite = defaultImage;
    }
}
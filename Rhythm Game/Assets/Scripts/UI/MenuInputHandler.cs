using UnityEngine;

public class MenuInputHandler : MonoBehaviour
{
    [Header("References")]
    public SnapScrollRect snapScrollRect;
    public ChartSelectionManager selectionManager;

    [Header("Input Settings")]
    public float stickDeadzone = 0.5f;
    public int selectButton = 8; // Lane 3 top button
    public bool selectIsButton = true;
    public string selectAxis = "";
    public float triggerThreshold = 0.5f;

    private int previousVertDir = 0;
    private bool previousSelectState = false;

    void Update()
    {
        CheckNavigationInput();
        CheckSelectInput();
    }

    void CheckNavigationInput()
    {
        if (snapScrollRect == null) return;

        float vertical = Input.GetAxisRaw("Vertical");
        int vertDir = vertical < -stickDeadzone ? -1 : (vertical > stickDeadzone ? 1 : 0);

        // Detect vertical direction change
        if (vertDir != previousVertDir && vertDir != 0)
        {
            if (vertDir == 1) // Up
            {
                snapScrollRect.NavigateUp();
            }
            else if (vertDir == -1) // Down
            {
                snapScrollRect.NavigateDown();
            }
        }

        previousVertDir = vertDir;
    }

    void CheckSelectInput()
    {
        if (snapScrollRect == null || selectionManager == null) return;

        bool selectPressed = GetSelectButtonDown();

        if (selectPressed)
        {
            SelectCurrentChart();
        }
    }

    bool GetSelectButtonDown()
    {
        if (selectIsButton)
        {
            return Input.GetKeyDown(KeyCode.JoystickButton0 + selectButton);
        }
        else
        {
            if (string.IsNullOrEmpty(selectAxis)) return false;

            float axisValue = Input.GetAxis(selectAxis);
            bool currentlyPressed = axisValue > triggerThreshold;
            bool wasPressed = previousSelectState;

            previousSelectState = currentlyPressed;

            return currentlyPressed && !wasPressed;
        }
    }

    void SelectCurrentChart()
    {
        Transform currentChild = snapScrollRect.contentRect.GetChild(snapScrollRect.GetCurrentIndex());
        if (currentChild == null) return;

        ChartListItem listItem = currentChild.GetComponent<ChartListItem>();
        if (listItem != null && listItem.selectButton != null)
        {
            listItem.selectButton.onClick.Invoke();
        }
    }
}
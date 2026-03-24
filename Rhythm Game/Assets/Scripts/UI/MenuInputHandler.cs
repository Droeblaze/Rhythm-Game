using UnityEngine;

public class MenuInputHandler : MonoBehaviour
{
    [Header("References")]
    public SnapScrollRect snapScrollRect;
    public ChartSelectionManager selectionManager;

    [Header("Input Settings")]
    public float stickDeadzone = 0.5f;

    private int previousVertDir = 0;

    void Update()
    {
        CheckNavigationInput();
        CheckSelectInput();
    }

    void CheckNavigationInput()
    {
        if (snapScrollRect == null) return;
        if (InputBindingManager.Instance == null) return;

        int vertDir = InputBindingManager.Instance.GetVerticalDir(stickDeadzone);

        if (vertDir != previousVertDir && vertDir != 0)
        {
            if (vertDir == 1)
            {
                snapScrollRect.NavigateUp();
            }
            else if (vertDir == -1)
            {
                snapScrollRect.NavigateDown();
            }
        }

        previousVertDir = vertDir;
    }

    void CheckSelectInput()
    {
        if (snapScrollRect == null || selectionManager == null) return;
        if (InputBindingManager.Instance == null) return;

        // Use Button 1 (Lane3 Top) as the select/confirm button
        bool selectPressed = InputBindingManager.Instance.GetBindingDown(
            InputBindingManager.Instance.Bindings.button1);

        if (selectPressed)
        {
            SelectCurrentChart();
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
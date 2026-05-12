using UnityEngine;

public class MenuInputHandler : MonoBehaviour
{
    [Header("References")]
    public SnapScrollRect snapScrollRect;

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
        if (snapScrollRect == null) return;
        if (InputBindingManager.Instance == null) return;

        bool selectPressed = InputBindingManager.Instance.GetBindingDown(
            InputBindingManager.Instance.Bindings.button1);

        if (selectPressed)
        {
            SelectCurrentItem();
        }
    }

    void SelectCurrentItem()
    {
        if (snapScrollRect.contentRect == null) return;

        int index = snapScrollRect.GetCurrentIndex();
        if (index < 0 || index >= snapScrollRect.contentRect.childCount) return;

        Transform currentChild = snapScrollRect.contentRect.GetChild(index);
        if (currentChild == null) return;

        // Try to invoke a Button on the current item
        UnityEngine.UI.Button button = currentChild.GetComponentInChildren<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.Invoke();
        }
    }
}
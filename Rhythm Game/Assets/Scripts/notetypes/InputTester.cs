using UnityEngine;

public class ControllerTest : MonoBehaviour
{
    private float[] previousAxisValues = new float[28];
    private bool[] axisInitialized = new bool[28];

    void Update()
    {
        // Test all buttons (0-30)
        for (int i = 0; i < 30; i++)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0 + i))
            {
                Debug.Log($"<color=cyan>Button {i} pressed!</color>");
            }
        }

        // Test all axes (0-27)
        for (int i = 0; i < 28; i++)
        {
            try
            {
                float axisValue = Input.GetAxisRaw($"Axis {i}");

                // Initialize on first frame
                if (!axisInitialized[i])
                {
                    previousAxisValues[i] = axisValue;
                    axisInitialized[i] = true;
                    continue;
                }

                // Detect ANY significant change (works for -1 to 1 or 0 to 1 triggers)
                float change = Mathf.Abs(axisValue - previousAxisValues[i]);
                if (change > 0.5f)
                {
                    Debug.Log($"<color=yellow>Axis {i} CHANGED: {previousAxisValues[i]:F2} ? {axisValue:F2}</color>");
                }

                // Also show current value if non-zero
                if (Mathf.Abs(axisValue) > 0.1f && Mathf.Abs(axisValue - previousAxisValues[i]) < 0.1f)
                {
                    Debug.Log($"Axis {i} holding: {axisValue:F2}");
                }

                previousAxisValues[i] = axisValue;
            }
            catch
            {
                // Axis doesn't exist, skip it
            }
        }

        // Test standard axes
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(horizontal) > 0.5f || Mathf.Abs(vertical) > 0.5f)
        {
            Debug.Log($"<color=green>Standard - Horizontal: {horizontal:F2}, Vertical: {vertical:F2}</color>");
        }
    }
}
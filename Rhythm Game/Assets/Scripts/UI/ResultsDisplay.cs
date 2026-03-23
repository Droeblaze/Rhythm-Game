using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultsDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI greatCountText;
    public TextMeshProUGUI goodCountText;
    public TextMeshProUGUI okCountText;
    public TextMeshProUGUI missCountText;
    public TextMeshProUGUI totalNotesText;

    [Header("Navigation")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        DisplayResults();
    }

    void Update()
    {
        // Check for any button press
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || AnyJoystickButtonPressed())
        {
            ReturnToMainMenu();
        }
    }

    bool AnyJoystickButtonPressed()
    {
        // Check all 20 possible joystick buttons
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0 + i))
            {
                return true;
            }
        }
        return false;
    }

    void DisplayResults()
    {
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManager.Instance is null! Make sure ScoreManager exists in the gameplay scene.");
            return;
        }

        Debug.Log("ScoreManager found! Displaying results...");

        // Get data from ScoreManager
        float accuracy = ScoreManager.Instance.GetAccuracy();
        int perfectCount = ScoreManager.Instance.GetPerfectCount();
        int greatCount = ScoreManager.Instance.GetGreatCount();
        int goodCount = ScoreManager.Instance.GetGoodCount();
        int okCount = ScoreManager.Instance.GetOkCount();
        int missCount = ScoreManager.Instance.GetMissCount();
        int totalNotes = ScoreManager.Instance.GetTotalNotesProcessed();

        Debug.Log($"Results - Accuracy: {accuracy:F2}%, Perfect: {perfectCount}, Great: {greatCount}, Good: {goodCount}, OK: {okCount}, Miss: {missCount}, Total: {totalNotes}");

        // Display results
        if (accuracyText != null)
        {
            accuracyText.text = $"{accuracy:F2}%";
            Debug.Log($"Set accuracyText to: {accuracyText.text}");
        }
        else
        {
            Debug.LogError("accuracyText is null!");
        }

        if (perfectCountText != null)
        {
            perfectCountText.text = $"Perfect: {perfectCount}";
        }
        else
        {
            Debug.LogError("perfectCountText is null!");
        }

        if (greatCountText != null)
        {
            greatCountText.text = $"Great: {greatCount}";
        }
        else
        {
            Debug.LogError("greatCountText is null!");
        }

        if (goodCountText != null)
        {
            goodCountText.text = $"Good: {goodCount}";
        }
        else
        {
            Debug.LogError("goodCountText is null!");
        }

        if (okCountText != null)
        {
            okCountText.text = $"OK: {okCount}";
        }
        else
        {
            Debug.LogError("okCountText is null!");
        }

        if (missCountText != null)
        {
            missCountText.text = $"Miss: {missCount}";
        }
        else
        {
            Debug.LogError("missCountText is null!");
        }

        if (totalNotesText != null)
        {
            totalNotesText.text = $"Total Notes: {totalNotes}";
        }
        else
        {
            Debug.LogError("totalNotesText is null!");
        }
    }

    public void ReturnToMainMenu()
    {
        // Don't reset stats here - they'll be reset when starting a new gameplay session
        // This allows the results to persist until the next game starts
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
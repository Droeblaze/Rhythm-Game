using TMPro;
using UnityEngine;
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
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI chartDifficultyText;
    public TextMeshProUGUI maxComboText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI rankText;

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

    string GetRank(float accuracy)
    {
        if (accuracy >= 100f)  return "S+";
        if (accuracy >= 98f)   return "S";
        if (accuracy >= 95f)   return "A+";
        if (accuracy >= 90f)   return "A";
        if (accuracy >= 80f)   return "B";
        if (accuracy >= 70f)   return "C";
        return "F";
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
        int maxCombo = ScoreManager.Instance.GetMaxCombo();
        string songTitle = ScoreManager.Instance.GetSongTitle();
        float chartDifficulty = ScoreManager.Instance.GetChartDifficulty();
        float points = Mathf.Round((accuracy / 100f) * chartDifficulty * 100f) / 100f;
        string rank = GetRank(accuracy);

        Debug.Log($"Results - Accuracy: {accuracy:F2}%, Perfect: {perfectCount}, Great: {greatCount}, Good: {goodCount}, OK: {okCount}, Miss: {missCount}, Total: {totalNotes}, Max Combo: {maxCombo}, Song: {songTitle}, Difficulty: {chartDifficulty:F1}, Points: {points:F2}, Rank: {rank}");

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
            perfectCountText.text = $"Perfect: {perfectCount}";
        else
            Debug.LogError("perfectCountText is null!");

        if (greatCountText != null)
            greatCountText.text = $"Great: {greatCount}";
        else
            Debug.LogError("greatCountText is null!");

        if (goodCountText != null)
            goodCountText.text = $"Good: {goodCount}";
        else
            Debug.LogError("goodCountText is null!");

        if (okCountText != null)
            okCountText.text = $"OK: {okCount}";
        else
            Debug.LogError("okCountText is null!");

        if (missCountText != null)
            missCountText.text = $"Miss: {missCount}";
        else
            Debug.LogError("missCountText is null!");

        if (totalNotesText != null)
            totalNotesText.text = $"Total Notes: {totalNotes}";
        else
            Debug.LogError("totalNotesText is null!");

        if (songNameText != null)
            songNameText.text = songTitle;
        else
            Debug.LogError("songNameText is null!");

        if (chartDifficultyText != null)
            chartDifficultyText.text = $"Difficulty: {chartDifficulty:F1}";
        else
            Debug.LogError("chartDifficultyText is null!");

        if (maxComboText != null)
            maxComboText.text = $"Max Combo: {maxCombo}";
        else
            Debug.LogError("maxComboText is null!");

        if (pointsText != null)
            pointsText.text = $"Points: {points:F2}";
        else
            Debug.LogError("pointsText is null!");

        if (rankText != null)
            rankText.text = rank;
        else
            Debug.LogError("rankText is null!");
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
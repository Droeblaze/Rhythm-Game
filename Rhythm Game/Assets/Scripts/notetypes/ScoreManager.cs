using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References (Optional - for gameplay scene)")]
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI greatCountText;
    public TextMeshProUGUI goodCountText;
    public TextMeshProUGUI okCountText;
    public TextMeshProUGUI missCountText;
    public TextMeshProUGUI totalNotesText;

    [Header("Judgement Weights")]
    public float perfectWeight = 100f;
    public float greatWeight = 90f;
    public float goodWeight = 80f;
    public float okWeight = 70f;
    public float missWeight = 0f;

    // Judgement counts
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int okCount = 0;
    private int missCount = 0;

    // Total notes processed (including both parts of hold notes)
    private int totalNotesProcessed = 0;

    // Current accuracy
    private float currentAccuracy = 0f;

    // Singleton instance
    public static ScoreManager Instance { get; private set; }

    void Awake()
    {
        // Implement singleton pattern and persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Records a judgement and updates the accuracy calculation.
    /// </summary>
    /// <param name="judgement">The judgement string (e.g., "Perfect!", "Great!", etc.)</param>
    public void RecordJudgement(string judgement)
    {
        // Increment the appropriate counter
        switch (judgement)
        {
            case "Perfect!":
                perfectCount++;
                break;
            case "Great!":
                greatCount++;
                break;
            case "Good":
                goodCount++;
                break;
            case "OK":
                okCount++;
                break;
            case "Miss":
                missCount++;
                break;
        }

        totalNotesProcessed++;
        CalculateAccuracy();
        UpdateUI();
    }

    /// <summary>
    /// Calculates the current accuracy as a weighted mean of all judgements.
    /// </summary>
    void CalculateAccuracy()
    {
        if (totalNotesProcessed == 0)
        {
            currentAccuracy = 0f;
            return;
        }

        float totalScore = (perfectCount * perfectWeight) +
                          (greatCount * greatWeight) +
                          (goodCount * goodWeight) +
                          (okCount * okWeight) +
                          (missCount * missWeight);

        currentAccuracy = totalScore / totalNotesProcessed;
    }

    /// <summary>
    /// Updates all UI elements with current statistics (only if references exist).
    /// </summary>
    void UpdateUI()
    {
        if (accuracyText != null)
            accuracyText.text = $"{currentAccuracy:F2}%";

        if (perfectCountText != null)
            perfectCountText.text = $"Perfect: {perfectCount}";

        if (greatCountText != null)
            greatCountText.text = $"Great: {greatCount}";

        if (goodCountText != null)
            goodCountText.text = $"Good: {goodCount}";

        if (okCountText != null)
            okCountText.text = $"OK: {okCount}";

        if (missCountText != null)
            missCountText.text = $"Miss: {missCount}";

        if (totalNotesText != null)
            totalNotesText.text = $"Notes: {totalNotesProcessed}";
    }

    /// <summary>
    /// Clears UI references when changing scenes (called by OnDestroy of scene-specific objects).
    /// </summary>
    public void ClearUIReferences()
    {
        accuracyText = null;
        perfectCountText = null;
        greatCountText = null;
        goodCountText = null;
        okCountText = null;
        missCountText = null;
        totalNotesText = null;
    }

    /// <summary>
    /// Resets all statistics.
    /// </summary>
    public void ResetStats()
    {
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        okCount = 0;
        missCount = 0;
        totalNotesProcessed = 0;
        currentAccuracy = 0f;
        UpdateUI();
    }

    // Getters for final score screen
    public int GetPerfectCount() => perfectCount;
    public int GetGreatCount() => greatCount;
    public int GetGoodCount() => goodCount;
    public int GetOkCount() => okCount;
    public int GetMissCount() => missCount;
    public int GetTotalNotesProcessed() => totalNotesProcessed;
    public float GetAccuracy() => currentAccuracy;
}
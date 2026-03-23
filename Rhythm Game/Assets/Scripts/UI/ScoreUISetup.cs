using UnityEngine;
using TMPro;

/// <summary>
/// Sets up ScoreManager UI references when the gameplay scene loads.
/// Attach this to a GameObject in your gameplay scene and assign the UI references.
/// </summary>
public class ScoreUISetup : MonoBehaviour
{
    [Header("Score UI References")]
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI greatCountText;
    public TextMeshProUGUI goodCountText;
    public TextMeshProUGUI okCountText;
    public TextMeshProUGUI missCountText;
    public TextMeshProUGUI totalNotesText;

    void Awake()
    {
        // Reset stats at the START of gameplay, not at the end
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetStats();
            Debug.Log("ScoreManager stats reset for new gameplay session!");
        }
    }

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetUIReferences(
                accuracyText,
                perfectCountText,
                greatCountText,
                goodCountText,
                okCountText,
                missCountText,
                totalNotesText
            );
            Debug.Log("ScoreManager UI references set successfully!");
        }
        else
        {
            Debug.LogError("ScoreManager.Instance is null! Make sure ScoreManager exists in the scene.");
        }
    }
}   
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Scene Names — must match Build Settings exactly")]
    [Tooltip("The first-run input setup scene name.")]
    public string firstRunSetupScene = "FirstRunSetup";   // ← change if different

    [Tooltip("The main menu scene name.")]
    public string mainMenuScene      = "MainMenu";         // ← change if different

    [Header("Debug")]
    [Tooltip("Force first-run setup to appear even if already completed. Useful during development.")]
    public bool forceShowSetup = false;

    void Start()
    {
        bool setupComplete = PlayerPrefs.GetInt(ControlProfileApplicator.KEY_SETUP_COMPLETE, 0) == 1;

        if (!setupComplete || forceShowSetup)
        {
            Debug.Log("[Bootstrap] First-run setup not complete. Loading setup scene.");
            SceneManager.LoadScene(firstRunSetupScene);
        }
        else
        {
            Debug.Log("[Bootstrap] Setup already complete. Loading main menu.");
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    // Dev utility: call this to clear all input prefs and re-trigger first-run ──
    // Hook to a button in your debug menu or call from the Console.
    [ContextMenu("Clear Input Setup (force first-run next launch)")]
    public void ClearInputSetup()
    {
        PlayerPrefs.DeleteKey(ControlProfileApplicator.KEY_SETUP_COMPLETE);
        PlayerPrefs.DeleteKey(ControlProfileApplicator.KEY_PROFILE);
        PlayerPrefs.Save();
        Debug.Log("[Bootstrap] Input setup cleared. First-run will show on next launch.");
    }
}

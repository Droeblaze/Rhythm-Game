using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Scroll Speed Settings")]
    public TextMeshProUGUI scrollSpeedText;
    public Button scrollSpeedIncreaseButton;
    public Button scrollSpeedDecreaseButton;
    
    [Header("Audio Offset Settings")]
    public TextMeshProUGUI audioOffsetText;
    public Button audioOffsetIncreaseButton;
    public Button audioOffsetDecreaseButton;

    [Header("Value Settings")]
    public float scrollSpeedMin = 1f;
    public float scrollSpeedMax = 15f;
    public float audioOffsetMin = -1f;
    public float audioOffsetMax = 1f;
    public float adjustmentIncrement = 0.05f;

    [Header("Default Values")]
    public float defaultScrollSpeed = 5f;
    public float defaultAudioOffset = 0f;

    // PlayerPrefs keys
    private const string SCROLL_SPEED_KEY = "ScrollSpeed";
    private const string AUDIO_OFFSET_KEY = "AudioOffset";

    // Current values
    private float currentScrollSpeed;
    private float currentAudioOffset;

    void Start()
    {
        LoadSettings();
        SetupButtons();
        UpdateUI();
    }

    void SetupButtons()
    {
        if (scrollSpeedIncreaseButton != null)
            scrollSpeedIncreaseButton.onClick.AddListener(IncreaseScrollSpeed);
        
        if (scrollSpeedDecreaseButton != null)
            scrollSpeedDecreaseButton.onClick.AddListener(DecreaseScrollSpeed);
        
        if (audioOffsetIncreaseButton != null)
            audioOffsetIncreaseButton.onClick.AddListener(IncreaseAudioOffset);
        
        if (audioOffsetDecreaseButton != null)
            audioOffsetDecreaseButton.onClick.AddListener(DecreaseAudioOffset);
    }

    void LoadSettings()
    {
        currentScrollSpeed = PlayerPrefs.GetFloat(SCROLL_SPEED_KEY, defaultScrollSpeed);
        currentAudioOffset = PlayerPrefs.GetFloat(AUDIO_OFFSET_KEY, defaultAudioOffset);
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat(SCROLL_SPEED_KEY, currentScrollSpeed);
        PlayerPrefs.SetFloat(AUDIO_OFFSET_KEY, currentAudioOffset);
        PlayerPrefs.Save();
    }

    public void IncreaseScrollSpeed()
    {
        currentScrollSpeed = Mathf.Min(currentScrollSpeed + adjustmentIncrement, scrollSpeedMax);
        currentScrollSpeed = Mathf.Round(currentScrollSpeed * 100f) / 100f;
        SaveSettings();
        UpdateUI();
    }

    public void DecreaseScrollSpeed()
    {
        currentScrollSpeed = Mathf.Max(currentScrollSpeed - adjustmentIncrement, scrollSpeedMin);
        currentScrollSpeed = Mathf.Round(currentScrollSpeed * 100f) / 100f;
        SaveSettings();
        UpdateUI();
    }

    public void IncreaseAudioOffset()
    {
        currentAudioOffset = Mathf.Min(currentAudioOffset + adjustmentIncrement, audioOffsetMax);
        currentAudioOffset = Mathf.Round(currentAudioOffset * 1000f) / 1000f;
        SaveSettings();
        UpdateUI();
    }

    public void DecreaseAudioOffset()
    {
        currentAudioOffset = Mathf.Max(currentAudioOffset - adjustmentIncrement, audioOffsetMin);
        currentAudioOffset = Mathf.Round(currentAudioOffset * 1000f) / 1000f;
        SaveSettings();
        UpdateUI();
    }

    public void ResetToDefaults()
    {
        currentScrollSpeed = defaultScrollSpeed;
        currentAudioOffset = defaultAudioOffset;
        SaveSettings();
        UpdateUI();
    }

    public void OpenControls()
    {
        SceneManager.LoadScene("Controls");
    }

    void UpdateUI()
    {
        if (scrollSpeedText != null)
            scrollSpeedText.text = currentScrollSpeed.ToString("F2");
        
        if (audioOffsetText != null)
            audioOffsetText.text = $"{(currentAudioOffset >= 0 ? "+" : "")}{currentAudioOffset.ToString("F3")}s";
    }

    // Static methods to access settings from other scripts
    public static float GetScrollSpeed()
    {
        return PlayerPrefs.GetFloat(SCROLL_SPEED_KEY, 5f);
    }

    public static float GetAudioOffset()
    {
        return PlayerPrefs.GetFloat(AUDIO_OFFSET_KEY, 0f);
    }
}
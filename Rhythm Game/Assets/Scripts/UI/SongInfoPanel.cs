using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays detailed song information on the right side of the song select screen.
/// Updated by SongSelectManager whenever the highlighted song changes.
/// </summary>
public class SongInfoPanel : MonoBehaviour
{
    [Header("Song Info")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI bpmText;
    public TextMeshProUGUI durationText;
    public Image songArtImage;

    [Header("Background Art")]
    [Tooltip("A second image placed behind the panel, showing the same album art zoomed in.")]
    public Image backgroundArtImage;
    [Tooltip("Scale multiplier for the zoomed-in background image.")]
    public float backgroundZoomScale = 1.5f;

    [Header("Difficulty List")]
    public Transform difficultyListContainer;
    public GameObject difficultyLabelPrefab; // Simple text prefab (no button, just a label)

    /// <summary>
    /// Populates the panel with data from the given SongContainer.
    /// </summary>
    public void Display(SongContainer song)
    {
        if (song == null)
        {
            Clear();
            return;
        }

        // Title & Artist
        if (titleText != null)
            titleText.text = song.title;

        if (artistText != null)
            artistText.text = song.artist;

        // BPM
        if (bpmText != null)
            bpmText.text = $"BPM: {song.bpm}";

        // Duration from audio clip
        if (durationText != null)
        {
            if (song.songAudio != null)
            {
                float seconds = song.songAudio.length;
                int minutes = Mathf.FloorToInt(seconds / 60f);
                int secs = Mathf.FloorToInt(seconds % 60f);
                durationText.text = $"Duration: {minutes}:{secs:D2}";
            }
            else
            {
                durationText.text = "Duration: --:--";
            }
        }

        // Album art
        if (songArtImage != null)
        {
            if (song.songArt != null)
            {
                songArtImage.sprite = song.songArt;
                songArtImage.color = Color.white;
            }
            else
            {
                songArtImage.sprite = null;
                songArtImage.color = new Color(0.2f, 0.2f, 0.2f);
            }
        }

        // Background art (zoomed-in copy of album art)
        if (backgroundArtImage != null)
        {
            backgroundArtImage.preserveAspect = true;
            if (song.songArt != null)
            {
                backgroundArtImage.sprite = song.songArt;
                backgroundArtImage.color = new Color(0.5f, 0.5f, 0.5f);
                backgroundArtImage.rectTransform.localScale = Vector3.one * backgroundZoomScale;
            }
            else
            {
                backgroundArtImage.sprite = null;
                backgroundArtImage.color = new Color(0.2f, 0.2f, 0.2f);
                backgroundArtImage.rectTransform.localScale = Vector3.one * backgroundZoomScale;
            }
        }

        // Difficulty list (display only — not selectable here)
        PopulateDifficultyList(song);
    }

    void PopulateDifficultyList(SongContainer song)
    {
        if (difficultyListContainer == null || difficultyLabelPrefab == null) return;

        // Clear old entries
        foreach (Transform child in difficultyListContainer)
        {
            Destroy(child.gameObject);
        }

        if (song.charts == null) return;

        ChartData[] sorted = song.GetSortedByDifficulty();
        foreach (ChartData chart in sorted)
        {
            if (chart == null) continue;

            GameObject labelObj = Instantiate(difficultyLabelPrefab, difficultyListContainer);
            TextMeshProUGUI label = labelObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = $"{chart.difficulty:F1}";
            }
        }
    }

    public void Clear()
    {
        if (titleText != null) titleText.text = "";
        if (artistText != null) artistText.text = "";
        if (bpmText != null) bpmText.text = "";
        if (durationText != null) durationText.text = "";
        if (songArtImage != null)
        {
            songArtImage.sprite = null;
            songArtImage.color = new Color(0.2f, 0.2f, 0.2f);
        }

        if (backgroundArtImage != null)
        {
            backgroundArtImage.sprite = null;
            backgroundArtImage.color = new Color(0.2f, 0.2f, 0.2f);
        }

        if (difficultyListContainer != null)
        {
            foreach (Transform child in difficultyListContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
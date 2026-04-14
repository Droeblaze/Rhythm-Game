using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Main controller for the song selection scene.
/// Loads all SongContainers from Resources/Songs, populates the scroll list,
/// plays audio preview on scroll, updates the info panel, and opens the difficulty panel.
/// </summary>
public class SongSelectManager : MonoBehaviour
{
    [Header("Left Panel — Song List")]
    public ScrollRect songScrollRect;
    public Transform songListContent;
    public GameObject songListItemPrefab; // Prefab with a TextMeshProUGUI child

    [Header("Right Panel — Song Info")]
    public SongInfoPanel songInfoPanel;

    [Header("Difficulty Overlay")]
    public DifficultyPanel difficultyPanel;

    [Header("Audio Preview")]
    public AudioSource previewAudioSource;
    [Range(0f, 1f)]
    public float previewVolume = 0.7f;
    public float previewFadeTime = 0.5f;
    public float previewStartPercent = 0.25f; // Start preview at 25% into the song

    [Header("Input Settings")]
    public float stickDeadzone = 0.5f;

    private List<SongContainer> songs = new List<SongContainer>();
    private int currentIndex = -1;
    private int previousVertDir = 0;

    // Fade state
    private bool isFadingOut = false;
    private float fadeTimer = 0f;
    private SongContainer pendingPreviewSong;

    void Start()
    {
        LoadSongs();
        PopulateSongList();

        if (songs.Count > 0)
        {
            SelectSong(0);
        }
    }

    void Update()
    {
        HandleFade();

        // Don't process scroll input while difficulty panel is open
        if (difficultyPanel != null && difficultyPanel.IsOpen()) return;

        HandleNavigationInput();
        HandleSelectInput();
        HandleBackInput();
    }

    // ??? Loading ?????????????????????????????????????????????

    void LoadSongs()
    {
        songs.Clear();

        SongContainer[] loaded = Resources.LoadAll<SongContainer>("Songs");
        if (loaded.Length == 0)
        {
            Debug.LogWarning("No SongContainer assets found in Resources/Songs!");
            return;
        }

        // Sort alphabetically by title
        System.Array.Sort(loaded, (a, b) => a.title.CompareTo(b.title));
        songs.AddRange(loaded);

        Debug.Log($"Loaded {songs.Count} songs.");
    }

    void PopulateSongList()
    {
        // Clear existing items
        foreach (Transform child in songListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < songs.Count; i++)
        {
            GameObject itemObj = Instantiate(songListItemPrefab, songListContent);

            TextMeshProUGUI label = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = songs[i].title;
            }
        }

        // Force layout rebuild so spacing calculations are correct
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(songListContent.GetComponent<RectTransform>());
    }

    // ??? Input ???????????????????????????????????????????????

    void HandleNavigationInput()
    {
        if (InputBindingManager.Instance == null) return;
        if (songs.Count == 0) return;

        int vertDir = InputBindingManager.Instance.GetVerticalDir(stickDeadzone);

        if (vertDir != previousVertDir && vertDir != 0)
        {
            if (vertDir == 1)
            {
                SelectSong(currentIndex - 1);
            }
            else if (vertDir == -1)
            {
                SelectSong(currentIndex + 1);
            }
        }

        previousVertDir = vertDir;
    }

    void HandleSelectInput()
    {
        if (InputBindingManager.Instance == null) return;

        bool selectPressed = InputBindingManager.Instance.GetBindingDown(
            InputBindingManager.Instance.Bindings.button1);

        if (selectPressed && currentIndex >= 0 && currentIndex < songs.Count)
        {
            OpenDifficultyPanel();
        }
    }

    void HandleBackInput()
    {
        if (InputBindingManager.Instance == null) return;

        bool backPressed = InputBindingManager.Instance.GetBindingDown(
            InputBindingManager.Instance.Bindings.button2);

        if (backPressed)
        {
            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    // ??? Song Selection ??????????????????????????????????????

    void SelectSong(int index)
    {
        if (songs.Count == 0) return;

        // Wrap around
        if (index < 0) index = songs.Count - 1;
        if (index >= songs.Count) index = 0;

        currentIndex = index;

        // Highlight in scroll list
        HighlightListItem(currentIndex);

        // Update right panel
        SongContainer song = songs[currentIndex];
        if (songInfoPanel != null)
        {
            songInfoPanel.Display(song);
        }

        // Start audio preview with crossfade
        StartPreview(song);
    }

    void HighlightListItem(int index)
    {
        for (int i = 0; i < songListContent.childCount; i++)
        {
            Transform child = songListContent.GetChild(i);
            TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                // Brighten the selected item, dim the rest
                label.color = (i == index) ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        // Scroll to keep the selected item visible
        ScrollToItem(index);
    }

    void ScrollToItem(int index)
    {
        if (songListContent.childCount == 0) return;

        // Calculate normalized scroll position
        float totalItems = songListContent.childCount;
        if (totalItems <= 1)
        {
            songScrollRect.verticalNormalizedPosition = 1f;
            return;
        }

        float normalizedPos = 1f - ((float)index / (totalItems - 1f));
        songScrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
    }

    // ??? Audio Preview ???????????????????????????????????????

    void StartPreview(SongContainer song)
    {
        if (previewAudioSource == null) return;

        if (song.songAudio == null)
        {
            previewAudioSource.Stop();
            return;
        }

        // If something is already playing, fade out first then swap
        if (previewAudioSource.isPlaying)
        {
            pendingPreviewSong = song;
            isFadingOut = true;
            fadeTimer = 0f;
        }
        else
        {
            PlayPreview(song);
        }
    }

    void PlayPreview(SongContainer song)
    {
        if (song.songAudio == null) return;

        previewAudioSource.clip = song.songAudio;
        previewAudioSource.volume = previewVolume;
        previewAudioSource.loop = true;

        // Start partway into the song so you hear the good part
        float startTime = song.songAudio.length * previewStartPercent;
        previewAudioSource.time = startTime;
        previewAudioSource.Play();
    }

    void HandleFade()
    {
        if (!isFadingOut) return;

        fadeTimer += Time.deltaTime;
        float t = fadeTimer / previewFadeTime;

        if (t >= 1f)
        {
            // Fade complete — swap to pending song
            isFadingOut = false;
            previewAudioSource.Stop();

            if (pendingPreviewSong != null)
            {
                PlayPreview(pendingPreviewSong);
                pendingPreviewSong = null;
            }
        }
        else
        {
            previewAudioSource.volume = Mathf.Lerp(previewVolume, 0f, t);
        }
    }

    // ??? Difficulty Panel ????????????????????????????????????

    void OpenDifficultyPanel()
    {
        if (difficultyPanel == null) return;

        SongContainer song = songs[currentIndex];
        if (song.charts == null || song.charts.Length == 0)
        {
            Debug.LogWarning($"Song '{song.title}' has no charts assigned.");
            return;
        }

        difficultyPanel.Show(song);
    }
}
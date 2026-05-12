using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Modal overlay that lets the player pick a difficulty, then starts gameplay.
/// Hidden by default. Shown when a song is confirmed from the scroll list.
/// </summary>
public class DifficultyPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public TextMeshProUGUI headerText;
    public Transform buttonContainer;
    public GameObject difficultyButtonPrefab;

    [Header("Input Settings")]
    public float stickDeadzone = 0.5f;

    [Header("Scene")]
    public string gameplaySceneName = "Gameplay";

    private SongContainer currentSong;
    private List<ChartData> currentCharts = new List<ChartData>();
    private List<Button> buttons = new List<Button>();
    private int currentIndex = 0;
    private int previousVertDir = 0;
    private bool isOpen = false;

    void Start()
    {
        Hide();
    }

    void Update()
    {
        if (!isOpen) return;
        if (InputBindingManager.Instance == null) return;

        // Navigate difficulties
        int vertDir = InputBindingManager.Instance.GetVerticalDir(stickDeadzone);
        if (vertDir != previousVertDir && vertDir != 0)
        {
            if (vertDir == 1) NavigateUp();
            else if (vertDir == -1) NavigateDown();
        }
        previousVertDir = vertDir;

        // Confirm
        if (InputBindingManager.Instance.GetBindingDown(InputBindingManager.Instance.Bindings.button1))
        {
            ConfirmSelection();
        }

        // Back (button2)
        if (InputBindingManager.Instance.GetBindingDown(InputBindingManager.Instance.Bindings.button2))
        {
            Hide();
        }
    }

    public void Show(SongContainer song)
    {
        currentSong = song;
        currentCharts.Clear();
        buttons.Clear();

        // Clear old buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        if (headerText != null)
            headerText.text = $"Select Difficulty — {song.title}";

        // Build sorted difficulty list
        ChartData[] sorted = song.GetSortedByDifficulty();
        foreach (ChartData chart in sorted)
        {
            if (chart == null) continue;
            currentCharts.Add(chart);

            GameObject btnObj = Instantiate(difficultyButtonPrefab, buttonContainer);
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = $"{chart.difficulty:F1}";

            Button btn = btnObj.GetComponent<Button>();
            int captured = currentCharts.Count - 1;
            btn.onClick.AddListener(() =>
            {
                currentIndex = captured;
                ConfirmSelection();
            });
            buttons.Add(btn);
        }

        panelRoot.SetActive(true);
        isOpen = true;
        currentIndex = 0;
        previousVertDir = 0;

        if (buttons.Count > 0)
            buttons[0].Select();
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        isOpen = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    void NavigateUp()
    {
        if (buttons.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = buttons.Count - 1;
        buttons[currentIndex].Select();
    }

    void NavigateDown()
    {
        if (buttons.Count == 0) return;
        currentIndex++;
        if (currentIndex >= buttons.Count) currentIndex = 0;
        buttons[currentIndex].Select();
    }

    void ConfirmSelection()
    {
        if (currentIndex < 0 || currentIndex >= currentCharts.Count) return;

        ChartSelectionManager.SetSelection(currentSong, currentCharts[currentIndex]);
        SceneManager.LoadScene(gameplaySceneName);
    }
}
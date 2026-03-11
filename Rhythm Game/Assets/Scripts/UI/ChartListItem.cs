using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChartListItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI bpmText;
    public TextMeshProUGUI difficultyText;
    public Button selectButton;

    private ChartData chartData;
    private ChartSelectionManager selectionManager;

    public void Initialize(ChartData data, ChartSelectionManager manager)
    {
        chartData = data;
        selectionManager = manager;

        // Display chart info
        songNameText.text = data.songAudio != null ? data.songAudio.name : "Unknown Song";
        bpmText.text = $"BPM: {data.bpm}";
        difficultyText.text = $"Difficulty: {data.difficulty:F1}";

        // Setup button click
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnChartSelected);
    }

    private void OnChartSelected()
    {
        if (selectionManager != null && chartData != null)
        {
            selectionManager.SelectChart(chartData);
        }
    }
}
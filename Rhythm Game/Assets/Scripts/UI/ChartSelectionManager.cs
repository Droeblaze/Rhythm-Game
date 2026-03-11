using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ChartSelectionManager : MonoBehaviour
{
    [Header("Chart Loading")]
    [Tooltip("Folder path inside Resources folder (e.g., 'Charts')")]
    public string chartsFolderPath = "Charts";

    [Header("UI References")]
    public ScrollRect scrollRect;
    public Transform contentContainer;
    public GameObject chartListItemPrefab;

    [Header("Selected Chart")]
    public string gameplaySceneName = "notetypes"; // Just the scene name without path

    private List<ChartData> availableCharts = new List<ChartData>();
    private ChartData selectedChart;

    void Start()
    {
        LoadAllCharts();
        PopulateScrollView();
    }

    private void LoadAllCharts()
    {
        availableCharts.Clear();

        // Load all ChartData assets from Resources folder
        ChartData[] charts = Resources.LoadAll<ChartData>(chartsFolderPath);

        if (charts.Length == 0)
        {
            Debug.LogWarning($"No charts found in Resources/{chartsFolderPath}");
            return;
        }

        availableCharts.AddRange(charts);

        // Sort by difficulty
        availableCharts.Sort((a, b) => a.difficulty.CompareTo(b.difficulty));

        Debug.Log($"Loaded {availableCharts.Count} charts");
    }

    private void PopulateScrollView()
    {
        // Clear existing items
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Create list item for each chart
        foreach (ChartData chart in availableCharts)
        {
            GameObject itemObj = Instantiate(chartListItemPrefab, contentContainer);
            ChartListItem item = itemObj.GetComponent<ChartListItem>();

            if (item != null)
            {
                item.Initialize(chart, this);
            }
        }
        
        // Force layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer.GetComponent<RectTransform>());
    }

    public void SelectChart(ChartData chart)
    {
        selectedChart = chart;
        Debug.Log($"Selected chart: {chart.songAudio?.name ?? "Unknown"}");

        // Store selected chart for gameplay scene
        PlayerPrefs.SetString("SelectedChartPath", $"{chartsFolderPath}/{chart.name}");
        PlayerPrefs.Save();

        Debug.Log($"Attempting to load scene: {gameplaySceneName}");
        
        // Load gameplay scene
        SceneManager.LoadScene(gameplaySceneName);
    }

    // Optional: Get the selected chart in the gameplay scene
    public static ChartData LoadSelectedChart()
    {
        string chartPath = PlayerPrefs.GetString("SelectedChartPath", "");
        if (string.IsNullOrEmpty(chartPath))
        {
            Debug.LogError("No chart selected!");
            return null;
        }

        Debug.Log($"Loading chart from path: {chartPath}");
        
        ChartData chart = Resources.Load<ChartData>(chartPath);
        if (chart == null)
        {
            Debug.LogError($"Failed to load chart at path: {chartPath}");
        }
        else
        {
            Debug.Log($"Successfully loaded chart: {chart.name}");
        }

        return chart;
    }
}
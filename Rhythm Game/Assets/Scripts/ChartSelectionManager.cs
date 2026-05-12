using UnityEngine;

/// <summary>
/// Stores the selected SongContainer and ChartData between scene loads.
/// Assets must live inside a Resources folder (e.g. Resources/Songs/, Resources/Charts/).
/// </summary>
public static class ChartSelectionManager
{
    private static string selectedSongPath;
    private static string selectedChartPath;

    public static void SetSelection(SongContainer song, ChartData chart)
    {
        selectedSongPath = "Songs/" + song.name;
        selectedChartPath = "Charts/" + chart.name;
    }

    public static SongContainer LoadSelectedSong()
    {
        if (string.IsNullOrEmpty(selectedSongPath))
        {
            Debug.LogWarning("No song selected!");
            return null;
        }
        return Resources.Load<SongContainer>(selectedSongPath);
    }

    public static ChartData LoadSelectedChart()
    {
        if (string.IsNullOrEmpty(selectedChartPath))
        {
            Debug.LogWarning("No chart selected!");
            return null;
        }
        return Resources.Load<ChartData>(selectedChartPath);
    }
}
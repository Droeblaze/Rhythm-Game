using UnityEditor;
using UnityEngine;

public class Tools_RemoveMissingScripts
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Scene")]
    static void RemoveMissingScriptsInScene()
    {
        var all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removed = 0;

        foreach (var go in all)
        {
            // Removes missing scripts from THIS GameObject
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }

        Debug.Log($"Removed {removed} missing script component(s) from the scene.");
    }
}
using UnityEditor;
using UnityEngine;

public class RemoveMissingScripts
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts (Scene)")]
    static void RemoveAllMissingScripts()
    {
        var all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removed = 0;

        foreach (var go in all)
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        Debug.Log($"Removed {removed} missing script component(s).");
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;

// Adds an "Import From JSON" button to WordSet's inspector so all pairs can be
// filled in one go (spreadsheet -> json -> click) instead of adding rows by hand.
// Expected file shape: { "pairs": [ { "term": "aš", "ru": "я", "en": "I" }, ... ] }
[CustomEditor(typeof(WordSet))]
public class WordSetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        if (GUILayout.Button("Import From JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Import Word Set", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, target);
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

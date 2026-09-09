using System.IO;
using UnityEditor;
using UnityEngine;

// Adds an "Import From JSON" button to ExampleSet's inspector — same pattern as
// WordSetEditor, so example pools can be authored the same way as word sets.
// Expected file shape: { "key": "vardininkas_as", "examples": [ { "from": "vyras", "to": "vyrai" }, ... ] }
[CustomEditor(typeof(ExampleSet))]
public class ExampleSetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        if (GUILayout.Button("Import From JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Import Example Set", Application.dataPath, "json");
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

using System.IO;
using UnityEditor;
using UnityEngine;

// Adds an "Import From JSON" button to ExampleBank's inspector, same pattern as
// WordSetEditor/VerbSetEditor.
// Expected file shape: { "entries": [ { "key": "Koks?", "sentences": ["Koks tavo vardas?", "Koks šiandien oras?"] }, ... ] }
[CustomEditor(typeof(ExampleBank))]
public class ExampleBankEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        if (GUILayout.Button("Import From JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Import Example Bank", Application.dataPath, "json");
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

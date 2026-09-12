using System.IO;
using UnityEditor;
using UnityEngine;

// Adds an "Import From JSON" button to VerbSet's inspector, mirroring WordSetEditor.
// Expected file shape:
// { "blocks": [ { "id": "...", "infinitive": {"target":"dirbti","var01":"работать","var02":"to work"},
//                 "presentTense": {...}, "pastTense": {...} }, ... ] }
[CustomEditor(typeof(VerbSet))]
public class VerbSetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        if (GUILayout.Button("Import From JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Import Verb Set", Application.dataPath, "json");
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

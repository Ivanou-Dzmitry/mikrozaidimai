using System.IO;
using UnityEditor;
using UnityEngine;

// Adds an "Import From JSON" button to WordSet's inspector so all pairs can be
// filled in one go (spreadsheet -> json -> click) instead of adding rows by hand.
// Expected file shape: { "pairs": [ { "id": "...", "target": "aš", "var01": "я", "var02": "I" }, ... ] }
// example (an ExampleSet reference) is asset wiring, not JSON content — it isn't
// in the file at all, you set it by hand per pair afterward. FromJsonOverwrite
// preserves each pair's already-set example when re-importing the SAME pairs in
// the SAME order, but adding/removing/reordering pairs can shift that index-based
// matching and misplace hand-set example references — re-check them after any
// re-import that changes the pair count or order.
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

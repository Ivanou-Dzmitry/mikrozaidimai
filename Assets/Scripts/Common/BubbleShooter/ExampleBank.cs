using System.Collections.Generic;
using UnityEngine;

// One game's pool of example sentences, referenced from GameInfo. Resolution
// is by plain text match against the word itself (WordPair.target) — no manual
// per-pair wiring, so it can't go stale when pairs are added/reordered/re-imported.
[CreateAssetMenu(fileName = "ExampleBank", menuName = "Games/Example Bank")]
public class ExampleBank : ScriptableObject
{
    public List<ExampleEntry> entries = new List<ExampleEntry>();

    // Returns a random example sentence for key, or null if there's no entry
    // for it (or it has no sentences).
    public string GetExample(string key)
    {
        foreach (var entry in entries)
        {
            if (entry.key != key) continue;
            if (entry.sentences.Count == 0) return null;
            return entry.sentences[Random.Range(0, entry.sentences.Count)];
        }

        return null;
    }
}

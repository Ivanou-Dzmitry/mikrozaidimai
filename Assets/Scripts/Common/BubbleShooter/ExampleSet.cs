using System.Collections.Generic;
using UnityEngine;

// A pool of example word transformations for one grammar rule/pattern (e.g. the
// "-as -> -ai" nominative plural ending), referenced by WordPair.example via key.
// GetRandom() picks a different one each time, so the same rule doesn't always
// show the same example word.
[CreateAssetMenu(fileName = "ExampleSet", menuName = "Games/Example Set")]
public class ExampleSet : ScriptableObject
{
    public string key;
    public List<ExamplePair> examples = new List<ExamplePair>();

    public ExamplePair GetRandom()
    {
        if (examples.Count == 0) return null;
        return examples[Random.Range(0, examples.Count)];
    }
}

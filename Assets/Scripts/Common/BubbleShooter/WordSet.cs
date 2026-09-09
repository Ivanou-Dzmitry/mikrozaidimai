using System.Collections.Generic;
using UnityEngine;

// One lesson's worth of word pairs (e.g. "Pronouns.asset", "Greetings.asset").
// A game's BubbleField is configured with one of these.
[CreateAssetMenu(fileName = "WordSet", menuName = "Games/Word Set")]
public class WordSet : ScriptableObject
{
    public List<WordPair> pairs = new List<WordPair>();
}

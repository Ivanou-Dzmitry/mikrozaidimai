using System;
using System.Collections.Generic;

// One word's pool of example sentences, e.g. key "Koks?" -> ["Koks tavo vardas?",
// "Koks šiandien oras?"]. ExampleBank.GetExample(key) picks one at random.
[Serializable]
public class ExampleEntry
{
    public string key;
    public List<string> sentences = new List<string>();
}

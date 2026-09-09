using System;
using UnityEngine.Serialization;

// One entry in a WordSet. target is always shown as-is (the thing being learned —
// a word, a grammatical ending, whatever); var01/var02 are two generic data slots,
// picked by UI language via GetVariant() — for real vocabulary they hold the RU/EN
// translation, but for non-translation content (e.g. grammar endings) they can just
// hold the same value, since not every WordSet is about translating between languages.
// example is optional: a direct reference to an ExampleSet asset for a "here's
// how it's used" hint — drag it in the Inspector, not a typed key, so it can't
// go stale or mismatch a typo (JSON import can't set this; it's asset wiring,
// not content data, so it's set by hand per pair that needs it).
[Serializable]
public class WordPair
{
    public string id;

    [FormerlySerializedAs("lt")]
    public string target;

    [FormerlySerializedAs("ru")]
    public string var01;

    [FormerlySerializedAs("eng")]
    public string var02;

    public ExampleSet example;

    public string GetVariant(string languageCode)
    {
        return languageCode == "ru" ? var01 : var02;
    }
}

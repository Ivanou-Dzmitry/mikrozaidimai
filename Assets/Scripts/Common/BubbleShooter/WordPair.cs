using System;
using UnityEngine.Serialization;

// One entry in a WordSet. target is always shown as-is (the thing being learned —
// a word, a grammatical ending, whatever); var01/var02 are two generic data slots,
// picked by UI language via GetVariant() — for real vocabulary they hold the RU/EN
// translation, but for non-translation content (e.g. grammar endings) they can just
// hold the same value, since not every WordSet is about translating between languages.
// An example sentence (if any) is resolved at runtime by looking target up in the
// game's ExampleBank (GameInfo.exampleBank) — no per-pair wiring here to go stale.
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

    public string GetVariant(string languageCode)
    {
        return languageCode == "ru" ? var01 : var02;
    }
}

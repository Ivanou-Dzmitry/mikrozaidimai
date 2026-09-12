using System;

// One form of a verb (e.g. the infinitive, or the present-tense form) — the
// Lithuanian word to build (target) plus its translation, shown as the clue.
// var01/var02 follow the same convention as WordPair: picked by UI language
// via GetVariant(), typically RU/EN.
[Serializable]
public class VerbForm
{
    public string target;
    public string var01;
    public string var02;

    public string GetVariant(string languageCode)
    {
        return languageCode == "ru" ? var01 : var02;
    }
}

using System;

// One vocabulary entry: lt is always shown as-is (that's the language being
// taught); ru/eng are plain strings so the whole set can be authored/bulk-edited
// externally (spreadsheet, script) and imported as json via WordSet's
// "Import From JSON" button, instead of clicking through a table UI per word.
[Serializable]
public class WordPair
{
    public string id;
    public string lt;
    public string ru;
    public string eng;

    public string GetTranslation(string languageCode)
    {
        return languageCode == "ru" ? ru : eng;
    }
}

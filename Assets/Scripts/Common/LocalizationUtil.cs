using UnityEngine.Localization;

// Resolves a LocalizedString to plain text, falling back gracefully instead of
// throwing when the reference is empty (never assigned) or points at a table
// entry that doesn't exist yet (e.g. a new GameInfo whose Title/Description
// keys were typed but not yet created in the table).
public static class LocalizationUtil
{
    public static string Resolve(LocalizedString localizedString, string fallback)
    {
        if (localizedString == null || localizedString.IsEmpty) return fallback;

        try
        {
            string result = localizedString.GetLocalizedString();
            return string.IsNullOrEmpty(result) ? fallback : result;
        }
        catch
        {
            return fallback;
        }
    }
}

using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;

// Applies the saved language once at startup, independent of any UI. Before this,
// the saved language was only applied by LanguageSelector.OnEnable — which lives on
// the Settings panel, so until that panel was opened the interface stayed on the
// default locale even though Settings.Data.language was already correct.
public static class LanguageBootstrap
{
    const string DefaultLanguageCode = "en";

    // AfterSceneLoad (not BeforeSceneLoad) so Settings.AutoLoad has definitely run first.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Apply()
    {
        var init = LocalizationSettings.InitializationOperation;
        if (init.IsDone) ApplySavedLocale();
        else init.Completed += _ => ApplySavedLocale();
    }

    static void ApplySavedLocale()
    {
        string code = string.IsNullOrEmpty(Settings.Data.language) ? DefaultLanguageCode : Settings.Data.language;
        var locale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault(l => l.Identifier.Code == code);
        if (locale != null) LocalizationSettings.SelectedLocale = locale;
    }
}

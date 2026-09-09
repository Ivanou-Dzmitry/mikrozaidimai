using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;

// Wires the 2 language buttons in the shared Settings panel to Unity Localization,
// persists the chosen language via Settings, and keeps the buttons' selected
// state (and therefore GameSession.Description and every other LocalizedString) in sync.
public class LanguageSelector : MonoBehaviour
{
    const string DefaultLanguageCode = "en";

    [SerializeField] SelectableButton englishButton;
    [SerializeField] SelectableButton russianButton;

    void OnEnable()
    {
        englishButton.Clicked += SelectEnglish;
        russianButton.Clicked += SelectRussian;
        StartCoroutine(ApplySavedLanguageWhenReady());
    }

    void OnDisable()
    {
        englishButton.Clicked -= SelectEnglish;
        russianButton.Clicked -= SelectRussian;
    }

    void SelectEnglish() => SelectLanguage("en");
    void SelectRussian() => SelectLanguage("ru");

    IEnumerator ApplySavedLanguageWhenReady()
    {
        yield return LocalizationSettings.InitializationOperation;
        ApplyLocale(string.IsNullOrEmpty(Settings.Data.language) ? DefaultLanguageCode : Settings.Data.language);
        RefreshButtons();
    }

    void SelectLanguage(string code)
    {
        ApplyLocale(code);
        Settings.Data.language = code;
        Settings.Save();
        RefreshButtons();
    }

    void ApplyLocale(string code)
    {
        var locale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault(l => l.Identifier.Code == code);
        if (locale != null) LocalizationSettings.SelectedLocale = locale;
    }

    void RefreshButtons()
    {
        string current = string.IsNullOrEmpty(Settings.Data.language) ? DefaultLanguageCode : Settings.Data.language;
        englishButton.SetSelected(current == "en");
        russianButton.SetSelected(current == "ru");
    }
}

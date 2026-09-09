using UnityEngine;

// Toggle for Settings.Data.helperEnabled — reuses SelectableButton's
// selected/normal visuals as the on/off state, same pattern as LanguageSelector.
[RequireComponent(typeof(SelectableButton))]
public class HelperSettingToggle : MonoBehaviour
{
    SelectableButton button;

    void Awake()
    {
        button = GetComponent<SelectableButton>();
    }

    void OnEnable()
    {
        button.Clicked += Toggle;
        Refresh();
    }

    void OnDisable()
    {
        button.Clicked -= Toggle;
    }

    void Toggle()
    {
        Settings.Data.helperEnabled = !Settings.Data.helperEnabled;
        Settings.Save();
        Refresh();
    }

    void Refresh()
    {
        button.SetSelected(Settings.Data.helperEnabled);
    }
}

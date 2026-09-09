using UnityEngine;

// Toggle for Settings.Data.helperDuration: click flips between 0 (off) and 1
// (on, minimum duration). Stays in sync with HelperDurationSlider — each
// refreshes the other after a change, since they edit the same value.
[RequireComponent(typeof(SelectableButton))]
public class HelperSettingToggle : MonoBehaviour
{
    [SerializeField] HelperDurationSlider durationSlider;

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
        Settings.Data.helperDuration = Settings.Data.helperDuration > 0 ? 0 : 1;
        Settings.Save();
        Refresh();
        if (durationSlider != null) durationSlider.Refresh();
    }

    public void Refresh()
    {
        button.SetSelected(Settings.Data.helperDuration > 0);
    }
}

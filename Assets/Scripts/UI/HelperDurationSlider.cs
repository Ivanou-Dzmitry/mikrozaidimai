using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 0-6 second whole-number slider for Settings.Data.helperDuration. 0 mirrors
// HelperSettingToggle's off state; moving off 0 turns the helper back on.
[RequireComponent(typeof(Slider))]
public class HelperDurationSlider : MonoBehaviour
{
    [SerializeField] HelperSettingToggle toggle;
    [SerializeField] TMP_Text durationLabel;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.wholeNumbers = true;
        slider.minValue = 0;
        slider.maxValue = 6;
    }

    void OnEnable()
    {
        Refresh();
        slider.onValueChanged.AddListener(HandleValueChanged);
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(HandleValueChanged);
    }

    void HandleValueChanged(float value)
    {
        Settings.Data.helperDuration = Mathf.RoundToInt(value);
        Settings.Save();
        if (toggle != null) toggle.Refresh();
        UpdateLabel();
    }

    public void Refresh()
    {
        slider.SetValueWithoutNotify(Settings.Data.helperDuration);
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (durationLabel == null) return;
        durationLabel.text = Settings.Data.helperDuration > 0 ? $"{Settings.Data.helperDuration}s" : "Off";
    }
}

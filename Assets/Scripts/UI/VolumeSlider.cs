using UnityEngine;
using UnityEngine.UI;

// Binds a Slider to one of the AudioManager's volume channels: loads the saved
// value on start and pushes every change to AudioManager (which applies it live
// and persists it via Settings). Volume is always 0..1 regardless of whatever
// Min/Max the Slider itself is configured with (0..1, 0..100, whatever) — the
// slider's own range is normalized to/from that 0..1 value.
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    public enum Channel { Music, Sfx }

    [SerializeField] Channel channel;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        float savedVolume = channel == Channel.Music ? Settings.Data.musicVolume : Settings.Data.sfxVolume;
        slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, savedVolume);
    }

    void OnEnable()
    {
        slider.onValueChanged.AddListener(HandleValueChanged);
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(HandleValueChanged);
    }

    void HandleValueChanged(float value)
    {
        float volume = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
        if (channel == Channel.Music) AudioManager.Instance.SetMusicVolume(volume);
        else AudioManager.Instance.SetSfxVolume(volume);
    }
}

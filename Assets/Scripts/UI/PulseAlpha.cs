using UnityEngine;

// Smoothly pulses a CanvasGroup's alpha between 1 and minAlpha (fading down and
// back up, not an on/off flash) while pulsing is on — an "this is active" cue for
// things like a paused state. Drive it with SetPulsing(); alpha snaps back to 1 when off.
[RequireComponent(typeof(CanvasGroup))]
public class PulseAlpha : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] float minAlpha = 0.25f;
    [Tooltip("Seconds for one full fade-down-and-back-up cycle.")]
    [SerializeField] float period = 1f;

    CanvasGroup group;
    bool pulsing;
    float startTime;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    void OnDisable()
    {
        pulsing = false;
        group.alpha = 1f;
    }

    public void SetPulsing(bool on)
    {
        pulsing = on;
        startTime = Time.unscaledTime;
        if (!on) group.alpha = 1f;
    }

    void Update()
    {
        if (!pulsing) return;

        // cosine from the moment pulsing started, so it eases down from full
        // alpha instead of jumping to some mid-cycle value
        float phase = (Time.unscaledTime - startTime) * Mathf.PI * 2f / period;
        float t = (Mathf.Cos(phase) + 1f) * 0.5f; // 1 -> 0 -> 1
        group.alpha = Mathf.Lerp(minAlpha, 1f, t);
    }
}

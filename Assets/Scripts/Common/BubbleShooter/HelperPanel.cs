using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Normally invisible. BubbleField calls ShowHint() on every wrong shot; if the
// player has the helper enabled in Settings, this flashes "translation -> correct
// term" for a bit, then fades out on its own. It's killed immediately (not left
// to finish its own timer) the moment the session leaves Play — otherwise it can
// still be visibly fading out on top of the Settings/Win panel the player just opened.
//
// Optionally has Pause/Skip buttons (Game 2). Only when at least one is assigned
// does the panel become clickable — and only while a hint is actually showing —
// so scenes without them (Game 1) keep the panel fully click-through.
public class HelperPanel : MonoBehaviour
{
    public static HelperPanel Instance { get; private set; }

    // Fires once the hint is no longer visible (fade finished, or cut short by
    // HideImmediately/Skip) — lets callers (e.g. WordBuilderField) hold off
    // showing the next word until the hint pointing at the previous one is gone.
    public event Action Hidden;

    [SerializeField] GameSession gameSession;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_Text exampleText;
    [Tooltip("How fast it fades out once its visible time (Settings.Data.helperDuration) is up. Not player-configurable, unlike the duration itself.")]
    [SerializeField] float fadeDuration = 0.3f;

    [Header("Optional controls")]
    [Tooltip("Toggle: freezes the visible-time countdown so the hint stays up until pressed again (or Skip). Leave empty if this scene doesn't need it.")]
    [SerializeField] Button pauseButton;
    [Tooltip("Hides the hint right away even if its time isn't up — whatever is waiting on Hidden (e.g. the next word) proceeds immediately. Leave empty if this scene doesn't need it.")]
    [SerializeField] Button skipButton;
    [Tooltip("Display-only countdown: full when the hint appears, drains to empty over its visible time, and freezes while paused. Leave empty if not needed.")]
    [SerializeField] Slider timerSlider;

    Coroutine activeRoutine;
    bool paused;
    PulseAlpha pausePulse;

    bool HasControls => pauseButton != null || skipButton != null;

    void Awake()
    {
        Instance = this;
        // optional: if the pause button has a PulseAlpha on it, it blinks while paused
        if (pauseButton != null) pausePulse = pauseButton.GetComponent<PulseAlpha>();
        if (timerSlider != null)
        {
            // display only — the player must not be able to drag the countdown
            timerSlider.interactable = false;

            // A slider copied from the volume settings (the _Slider prefab) carries a
            // VolumeSlider, which would turn this countdown into the SFX volume (and
            // save it) — silencing every UI sound once the timer reaches 0.
            var volumeSlider = timerSlider.GetComponent<VolumeSlider>();
            if (volumeSlider != null)
            {
                Debug.LogWarning("[HelperPanel] Timer Slider has a VolumeSlider on it, which would overwrite the sound volume. Disabled it — remove that component from the timer slider.", timerSlider);
                volumeSlider.enabled = false;
            }
        }
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (gameSession == null) gameSession = FindFirstObjectByType<GameSession>();
        canvasGroup.alpha = 0f;

        // A CanvasGroup blocks raycasts to whatever's underneath even at alpha 0
        // unless told not to, which silently ate clicks on UI under/behind it
        // (letter buttons, etc.). Off by default; SetInteractive(true) only turns
        // it on while a hint is visible AND this panel actually has buttons.
        SetInteractive(false);
    }

    void OnEnable()
    {
        if (gameSession != null) gameSession.StateChanged += HandleStateChanged;
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (skipButton != null) skipButton.onClick.AddListener(Skip);
    }

    void OnDisable()
    {
        if (gameSession != null) gameSession.StateChanged -= HandleStateChanged;
        if (pauseButton != null) pauseButton.onClick.RemoveListener(TogglePause);
        if (skipButton != null) skipButton.onClick.RemoveListener(Skip);
    }

    void HandleStateChanged(GameState state)
    {
        if (state != GameState.Play) HideImmediately();
    }

    void TogglePause()
    {
        SetPaused(!paused);
    }

    void SetPaused(bool value)
    {
        paused = value;
        if (pausePulse != null) pausePulse.SetPulsing(value);
    }

    void Skip()
    {
        HideImmediately();
    }

    public void HideImmediately()
    {
        bool wasShowing = activeRoutine != null;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        canvasGroup.alpha = 0f;
        SetPaused(false);
        SetInteractive(false);

        if (wasShowing) Hidden?.Invoke();
    }

    // example is optional — pass null/empty for none. Returns whether a hint
    // will actually be shown (false if the player has the helper turned off
    // in Settings) — callers waiting on Hidden should skip waiting entirely
    // when this comes back false, since it never fires.
    public bool ShowHint(string translation, string correctTerm, string example = null)
    {
        SetExampleText(example);
        return Show($"{translation} → {correctTerm}");
    }

    // Plain one-line message (e.g. a localized "Great") instead of the
    // translation-→-answer hint format — same show/fade/Hidden behavior.
    public bool ShowMessage(string message, string example = null)
    {
        SetExampleText(example);
        return Show(message);
    }

    void SetExampleText(string example)
    {
        if (exampleText == null) return;
        bool hasExample = !string.IsNullOrEmpty(example);
        exampleText.gameObject.SetActive(hasExample);
        if (hasExample) exampleText.text = example;
    }

    bool Show(string message)
    {
        if (Settings.Data.helperDuration <= 0) return false;

        text.text = message;

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        SetPaused(false);
        SetInteractive(true);
        activeRoutine = StartCoroutine(ShowThenFade(Settings.Data.helperDuration));
        return true;
    }

    // fill 1 = full, 0 = empty, mapped onto the slider's own min/max so it
    // works even if they aren't 0..1
    void SetTimerFill(float fill)
    {
        if (timerSlider != null) timerSlider.value = Mathf.Lerp(timerSlider.minValue, timerSlider.maxValue, fill);
    }

    void SetInteractive(bool on)
    {
        // no buttons -> stay fully click-through no matter what
        bool enable = on && HasControls;
        canvasGroup.blocksRaycasts = enable;
        canvasGroup.interactable = enable;
    }

    IEnumerator ShowThenFade(float visibleDuration)
    {
        canvasGroup.alpha = 1f;

        // manual timer instead of WaitForSeconds so Pause can freeze it
        // (and the slider, which just mirrors it, freezes with it)
        float visibleTime = 0f;
        SetTimerFill(1f);
        while (visibleTime < visibleDuration)
        {
            if (!paused) visibleTime += Time.deltaTime;
            SetTimerFill(1f - Mathf.Clamp01(visibleTime / visibleDuration));
            yield return null;
        }
        SetTimerFill(0f);

        float t = 0f;
        while (t < fadeDuration)
        {
            if (paused)
            {
                // paused mid-fade: pop back to fully readable and restart the fade on resume
                canvasGroup.alpha = 1f;
                t = 0f;
                yield return null;
                continue;
            }

            t += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        activeRoutine = null;
        SetInteractive(false);
        Hidden?.Invoke();
    }
}

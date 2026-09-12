using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Normally invisible. BubbleField calls ShowHint() on every wrong shot; if the
// player has the helper enabled in Settings, this flashes "translation -> correct
// term" for a bit, then fades out on its own. It's killed immediately (not left
// to finish its own timer) the moment the session leaves Play — otherwise it can
// still be visibly fading out on top of the Settings/Win panel the player just opened.
public class HelperPanel : MonoBehaviour
{
    public static HelperPanel Instance { get; private set; }

    // Fires once the hint is no longer visible (fade finished, or cut short by
    // HideImmediately) — lets callers (e.g. WordBuilderField) hold off showing
    // the next word until the hint pointing at the previous one is gone.
    public event Action Hidden;

    [SerializeField] GameSession gameSession;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_Text exampleText;
    [Tooltip("How fast it fades out once its visible time (Settings.Data.helperDuration) is up. Not player-configurable, unlike the duration itself.")]
    [SerializeField] float fadeDuration = 0.3f;

    Coroutine activeRoutine;

    void Awake()
    {
        Instance = this;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (gameSession == null) gameSession = FindFirstObjectByType<GameSession>();
        canvasGroup.alpha = 0f;

        // Purely a text overlay — never has anything to click, but a CanvasGroup
        // still blocks raycasts to whatever's underneath even at alpha 0 unless
        // told not to, which was silently eating clicks on UI under/behind it
        // (letter buttons, etc.) for its whole visible+fade duration.
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    void OnEnable()
    {
        if (gameSession != null) gameSession.StateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        if (gameSession != null) gameSession.StateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState state)
    {
        if (state != GameState.Play) HideImmediately();
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
        activeRoutine = StartCoroutine(ShowThenFade(Settings.Data.helperDuration));
        return true;
    }

    IEnumerator ShowThenFade(float visibleDuration)
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(visibleDuration);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        activeRoutine = null;
        Hidden?.Invoke();
    }
}

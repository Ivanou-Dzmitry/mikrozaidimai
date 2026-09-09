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
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        canvasGroup.alpha = 0f;
    }

    // example is optional — pass null/empty when the WordPair has no ExampleSet.
    public void ShowHint(string translation, string correctTerm, string example = null)
    {
        if (Settings.Data.helperDuration <= 0) return;

        text.text = $"{translation} → {correctTerm}";

        if (exampleText != null)
        {
            bool hasExample = !string.IsNullOrEmpty(example);
            exampleText.gameObject.SetActive(hasExample);
            if (hasExample) exampleText.text = example;
        }

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowThenFade(Settings.Data.helperDuration));
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
    }
}

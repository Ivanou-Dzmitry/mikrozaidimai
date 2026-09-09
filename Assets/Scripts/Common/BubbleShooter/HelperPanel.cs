using System.Collections;
using TMPro;
using UnityEngine;

// Normally invisible. BubbleField calls ShowHint() on every wrong shot; if the
// player has the helper enabled in Settings, this flashes "translation -> correct
// term" for a bit, then fades out on its own.
public class HelperPanel : MonoBehaviour
{
    public static HelperPanel Instance { get; private set; }

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TMP_Text text;
    [SerializeField] float visibleDuration = 1f;
    [SerializeField] float fadeDuration = 0.3f;

    Coroutine activeRoutine;

    void Awake()
    {
        Instance = this;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void ShowHint(string translation, string correctTerm)
    {
        if (!Settings.Data.helperEnabled) return;

        text.text = $"{translation} → {correctTerm}";

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowThenFade());
    }

    IEnumerator ShowThenFade()
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

using System.Collections;
using TMPro;
using UnityEngine;

// The zone under the translation where letters accumulate as the player taps
// letter buttons. Plain text-based (not per-letter boxes) — swap the display
// logic later if boxes are wanted, CurrentText/Append/RemoveLast/Clear stay the API.
// Shows a blinking "_" after the last typed letter, console-style.
public class AnswerZone : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [Tooltip("Seconds between cursor on/off toggles.")]
    [SerializeField] float cursorBlinkInterval = 0.5f;

    public string CurrentText { get; private set; } = "";

    bool cursorVisible = true;
    Coroutine blinkRoutine;

    void OnEnable()
    {
        blinkRoutine = StartCoroutine(BlinkCursor());
    }

    void OnDisable()
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
    }

    public void Clear()
    {
        CurrentText = "";
        ResetBlink();
    }

    public void Append(string letter)
    {
        CurrentText += letter;
        ResetBlink();
    }

    public void RemoveLast()
    {
        if (CurrentText.Length == 0) return;
        CurrentText = CurrentText.Substring(0, CurrentText.Length - 1);
        ResetBlink();
    }

    // Snap the cursor back to visible and restart its timer so it doesn't
    // happen to be mid-blink-off right after the player types.
    void ResetBlink()
    {
        cursorVisible = true;
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkCursor());
        Refresh();
    }

    IEnumerator BlinkCursor()
    {
        while (true)
        {
            yield return new WaitForSeconds(cursorBlinkInterval);
            cursorVisible = !cursorVisible;
            Refresh();
        }
    }

    void Refresh()
    {
        if (label != null) label.text = CurrentText + (cursorVisible ? "_" : " ");
    }
}

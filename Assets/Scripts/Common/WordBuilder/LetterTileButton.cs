using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One letter tile in the letter panel. Stays enabled and clickable after use —
// per spec the same button can be pressed repeatedly to add the same letter
// more than once (it is never consumed/disabled). Flashes a color and a small
// scale punch on press, then eases both back to normal, so the player can see
// which tile they hit last.
[RequireComponent(typeof(Button))]
public class LetterTileButton : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Image background;

    [Header("Press Feedback")]
    [SerializeField] Color pressedColor = Color.yellow;
    [Tooltip("Scale multiplier applied on press (1 = no scale change).")]
    [SerializeField] float pressedScale = 1.15f;
    [SerializeField] float returnDuration = 0.3f;

    [Header("Hint Feedback")]
    [SerializeField] Color hintColor = Color.cyan;
    [SerializeField] float hintBlinkInterval = 0.2f;
    [SerializeField] int hintBlinkCount = 4;

    Button button;
    Color normalColor;
    Vector3 normalScale;
    Coroutine flashRoutine;
    Coroutine hintRoutine;
    string letter;
    Action<string> onClicked;

    public string Letter => letter;

    void Awake()
    {
        button = GetComponent<Button>();
        if (background == null) background = GetComponent<Image>();
        if (background != null) normalColor = background.color;
        normalScale = transform.localScale;

        button.onClick.AddListener(HandleClicked);
    }

    public void Setup(string letter, Action<string> onClicked)
    {
        this.letter = letter;
        this.onClicked = onClicked;
        if (label != null) label.text = letter;
    }

    // Blinks this tile a few times so the player can spot which letter comes
    // next, without saying anything else about the word.
    public void PlayHint()
    {
        if (hintRoutine != null) StopCoroutine(hintRoutine);
        hintRoutine = StartCoroutine(BlinkHint());
    }

    void HandleClicked()
    {
        onClicked?.Invoke(letter);

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashThenReturn());
    }

    IEnumerator BlinkHint()
    {
        for (int i = 0; i < hintBlinkCount; i++)
        {
            if (background != null) background.color = hintColor;
            yield return new WaitForSeconds(hintBlinkInterval);
            if (background != null) background.color = normalColor;
            yield return new WaitForSeconds(hintBlinkInterval);
        }

        hintRoutine = null;
    }

    IEnumerator FlashThenReturn()
    {
        if (background != null) background.color = pressedColor;
        transform.localScale = normalScale * pressedScale;

        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float lerp = t / returnDuration;
            if (background != null) background.color = Color.Lerp(pressedColor, normalColor, lerp);
            transform.localScale = Vector3.Lerp(normalScale * pressedScale, normalScale, lerp);
            yield return null;
        }

        if (background != null) background.color = normalColor;
        transform.localScale = normalScale;
        flashRoutine = null;
    }
}

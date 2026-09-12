using TMPro;
using UnityEngine;

// Put on the TMP text that shows "<blocks fully correct> / <total blocks>"
// (e.g. "1 / 80") — subscribes to WordBuilderField.ProgressChanged and stays
// in sync automatically, same pattern as ScoreText.
[RequireComponent(typeof(TMP_Text))]
public class StatText : MonoBehaviour
{
    [SerializeField] WordBuilderField field;

    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        if (field == null) field = FindFirstObjectByType<WordBuilderField>();
    }

    void OnEnable()
    {
        field.ProgressChanged += HandleProgressChanged;
        HandleProgressChanged(field.BlocksCompleted, field.TotalBlocks);
    }

    void OnDisable()
    {
        field.ProgressChanged -= HandleProgressChanged;
    }

    void HandleProgressChanged(int completed, int total)
    {
        label.text = $"{completed} / {total}";
    }
}

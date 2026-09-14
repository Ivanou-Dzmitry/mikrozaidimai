using TMPro;
using UnityEngine;

// Put on the TMP text that shows how many 3-word blocks have been resolved
// so far, pass or fail combined — a block that fails and comes back later
// counts again each retry. Subscribes to WordBuilderField.AttemptsChanged,
// same pattern as ScoreText/StatText.
[RequireComponent(typeof(TMP_Text))]
public class StepsText : MonoBehaviour
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
        field.AttemptsChanged += HandleAttemptsChanged;
        HandleAttemptsChanged(field.TotalAttempts);
    }

    void OnDisable()
    {
        field.AttemptsChanged -= HandleAttemptsChanged;
    }

    void HandleAttemptsChanged(int attempts)
    {
        label.text = attempts.ToString();
    }
}

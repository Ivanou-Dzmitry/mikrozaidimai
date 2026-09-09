using TMPro;
using UnityEngine;
using UnityEngine.Localization;

// Generic replacement for Unity's native Localize String Event when that
// component isn't cooperating — same resolve pattern already proven to work
// for GameSession.Description and GameInfo.title in this project.
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] LocalizedString text;

    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (Application.isPlaying) label.text = text.GetLocalizedString();
    }
}

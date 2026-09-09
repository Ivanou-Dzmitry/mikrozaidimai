using TMPro;
using UnityEngine;

// Put on the TMP text inside the info/help panel — pulls its content from the
// current GameSession's Description (GameLogic in the scene).
[RequireComponent(typeof(TMP_Text))]
public class GameDescriptionText : MonoBehaviour
{
    [SerializeField] GameSession gameSession;

    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        if (gameSession == null) gameSession = FindFirstObjectByType<GameSession>();
    }

    void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        if (gameSession != null) label.text = gameSession.Description;
    }
}

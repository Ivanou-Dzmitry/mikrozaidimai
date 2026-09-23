using TMPro;
using UnityEngine;

// Put on the TMP text that shows how many attempts the player has made this
// session (GameSession.Attempts) — subscribes to AttemptsChanged and stays in
// sync automatically, same pattern as ScoreText. Works in any game, since
// each game just calls GameSession.AddAttempt() for whatever counts as a try.
[RequireComponent(typeof(TMP_Text))]
public class StepsText : MonoBehaviour
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
        gameSession.AttemptsChanged += HandleAttemptsChanged;
        HandleAttemptsChanged(gameSession.Attempts);
    }

    void OnDisable()
    {
        gameSession.AttemptsChanged -= HandleAttemptsChanged;
    }

    void HandleAttemptsChanged(int attempts)
    {
        label.text = attempts.ToString();
    }
}

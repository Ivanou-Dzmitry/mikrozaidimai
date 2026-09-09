using TMPro;
using UnityEngine;

// Put on the TMP text that shows the current score — subscribes to
// GameSession.ScoreChanged and keeps itself in sync automatically.
[RequireComponent(typeof(TMP_Text))]
public class ScoreText : MonoBehaviour
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
        gameSession.ScoreChanged += HandleScoreChanged;
        HandleScoreChanged(gameSession.Score);
    }

    void OnDisable()
    {
        gameSession.ScoreChanged -= HandleScoreChanged;
    }

    void HandleScoreChanged(int score)
    {
        label.text = score.ToString("000");
    }
}

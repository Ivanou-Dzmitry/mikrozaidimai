using UnityEngine;

// Plays a sound whenever the current GameSession's score changes — decoupled
// from BubbleField's own correct/wrong feedback, so any game using GameSession
// gets a "score went up" sound for free just by adding this component.
public class ScoreSound : MonoBehaviour
{
    [SerializeField] GameSession gameSession;
    [SerializeField] AudioClip sound;

    void Awake()
    {
        if (gameSession == null) gameSession = FindFirstObjectByType<GameSession>();
    }

    void OnEnable()
    {
        gameSession.ScoreChanged += HandleScoreChanged;
    }

    void OnDisable()
    {
        gameSession.ScoreChanged -= HandleScoreChanged;
    }

    void HandleScoreChanged(int score)
    {
        if (sound != null) AudioManager.Instance.PlayUiSfx(sound);
    }
}

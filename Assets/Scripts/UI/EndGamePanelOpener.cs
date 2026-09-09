using UnityEngine;

// Opens the end-of-game panel (same panel for both outcomes) as soon as the
// current GameSession reaches Win or Lose.
public class EndGamePanelOpener : MonoBehaviour
{
    [SerializeField] GameSession gameSession;
    [SerializeField] GameObject winPanel;

    void Awake()
    {
        if (gameSession == null) gameSession = FindFirstObjectByType<GameSession>();
    }

    void OnEnable()
    {
        gameSession.StateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        gameSession.StateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState state)
    {
        if (state == GameState.Win || state == GameState.Lose)
        {
            PanelManager.Instance.Open(winPanel);
        }
    }
}

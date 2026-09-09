using TMPro;
using UnityEngine;

// Put on the TMP text that shows the session's elapsed play time (mm:ss) —
// GameSession.ElapsedTime only ticks while State == Play, so this already
// reflects actual play time, not time spent paused.
[RequireComponent(typeof(TMP_Text))]
public class TimeText : MonoBehaviour
{
    [SerializeField] GameSession gameSession;

    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        if (gameSession == null) gameSession = FindFirstObjectByType<GameSession>();
    }

    void Update()
    {
        if (gameSession == null) return;

        int totalSeconds = Mathf.FloorToInt(gameSession.ElapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        label.text = $"{minutes:00}:{seconds:00}";
    }
}

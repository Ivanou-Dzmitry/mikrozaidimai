using TMPro;
using UnityEngine;

// Put on the TMP text that shows how many attempts failed this session
// (GameSession.Misses) — same pattern as StepsText/ScoreText.
[RequireComponent(typeof(TMP_Text))]
public class MissesText : MonoBehaviour
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
        gameSession.MissesChanged += HandleMissesChanged;
        HandleMissesChanged(gameSession.Misses);
    }

    void OnDisable()
    {
        gameSession.MissesChanged -= HandleMissesChanged;
    }

    void HandleMissesChanged(int misses)
    {
        label.text = misses.ToString();
    }
}

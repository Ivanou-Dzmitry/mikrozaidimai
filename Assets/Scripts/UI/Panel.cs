using UnityEngine;

// Attach to a panel's root GameObject to describe how PanelManager should treat it.
// Whether opening/closing this panel pauses/resumes gameplay lives here, on the panel
// itself, not on whatever button happens to open or close it.
public class Panel : MonoBehaviour
{
    [SerializeField] bool pausesGame;
    [SerializeField] GameSession gameSession;

    void Awake()
    {
        if (pausesGame && gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }
    }

    // Self-registers with PanelManager whenever this panel becomes active, no matter
    // how it got turned on (OpenPanelButton, active by default in the scene, toggled
    // from the inspector while testing, etc.) — so ClosePanelButton always has
    // something correct to close.
    void OnEnable()
    {
        PanelManager.Instance.Open(gameObject);
    }

    public void NotifyOpened()
    {
        if (pausesGame && gameSession != null) gameSession.Pause();
    }

    public void NotifyClosed()
    {
        if (pausesGame && gameSession != null) gameSession.Resume();
    }
}

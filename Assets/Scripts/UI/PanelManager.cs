using UnityEngine;

// Keeps track of which panel is currently open in the scene and enforces that only
// one is ever open at a time: opening a new one closes whatever was open first.
// Scene-scoped on purpose (not DontDestroyOnLoad) — panels live under a scene's own
// Canvas, so each scene gets its own PanelManager, created lazily on first use.
public class PanelManager : MonoBehaviour
{
    static PanelManager instance;

    public static PanelManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("PanelManager");
                instance = go.AddComponent<PanelManager>();
            }
            return instance;
        }
    }

    [Header("Startup (optional)")]
    [Tooltip("All top-level panels in this scene's canvas — used only to force a clean startup state.")]
    [SerializeField] GameObject[] panels;
    [Tooltip("Shown at scene start regardless of what was left active while working on the scene in the editor.")]
    [SerializeField] GameObject defaultPanel;

    GameObject currentPanelObject;
    Panel currentPanel;
    GameObject previousPanelObject;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Runs after every panel's own Awake/OnEnable, so this always wins over
        // whatever got left active in the editor while iterating on a panel.
        if (panels != null)
        {
            foreach (GameObject panel in panels)
            {
                if (panel != null && panel != defaultPanel) panel.SetActive(false);
            }
        }

        if (defaultPanel != null) Open(defaultPanel);
    }

    public void Open(GameObject panelObject)
    {
        if (currentPanelObject == panelObject) return;

        previousPanelObject = currentPanelObject;
        CloseCurrentInternal();

        currentPanelObject = panelObject;
        currentPanel = panelObject != null ? panelObject.GetComponent<Panel>() : null;

        if (panelObject != null)
        {
            panelObject.SetActive(true);
            if (currentPanel != null) currentPanel.NotifyOpened();
        }
    }

    public void Close(GameObject panelObject)
    {
        if (currentPanelObject != panelObject) return;
        CloseCurrent();
    }

    // Closes whatever is open and reveals whatever was open right before it
    // (one level back), falling back to defaultPanel if there was nothing before.
    public void CloseCurrent()
    {
        if (currentPanelObject == null) return;

        CloseCurrentInternal();

        GameObject fallback = previousPanelObject != null ? previousPanelObject : defaultPanel;
        previousPanelObject = null;

        if (fallback != null) Open(fallback);
    }

    void CloseCurrentInternal()
    {
        if (currentPanel != null) currentPanel.NotifyClosed();
        if (currentPanelObject != null) currentPanelObject.SetActive(false);

        currentPanelObject = null;
        currentPanel = null;
    }
}

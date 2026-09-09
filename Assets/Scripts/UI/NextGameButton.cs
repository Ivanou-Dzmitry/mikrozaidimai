using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Loads whatever GameCatalog says comes after GameLaunchConfig.Current into
// the same shared scene. Hides itself entirely if there's no next game (last
// in the catalog, or no config/catalog at all) — same pattern as
// ClosePanelButton/RunGameButton's other conditional UI elements.
[RequireComponent(typeof(Button))]
public class NextGameButton : MonoBehaviour
{
    [SerializeField] GameCatalog catalog;

    Button button;
    GameInfo nextGame;

    void Awake()
    {
        button = GetComponent<Button>();

        nextGame = catalog != null && GameLaunchConfig.Current != null
            ? catalog.GetNext(GameLaunchConfig.Current)
            : null;

        gameObject.SetActive(nextGame != null);
    }

    void OnEnable()
    {
        button.onClick.AddListener(LoadNext);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(LoadNext);
    }

    void LoadNext()
    {
        GameLaunchConfig.Current = nextGame;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

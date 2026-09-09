using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Reloads the current scene as-is — GameLaunchConfig.Current is left untouched
// (it's static, survives the reload on its own), so the same game restarts.
[RequireComponent(typeof(Button))]
public class RetryButton : MonoBehaviour
{
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(Retry);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(Retry);
    }

    void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

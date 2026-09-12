using TMPro;
using UnityEngine;

// Put on the TMP text showing the game's title in-game (e.g. the header at the
// top) — pulls from GameLaunchConfig.Current (set by whichever RunGameButton
// launched this scene), so the same shared scene shows the right name per game.
[RequireComponent(typeof(TMP_Text))]
public class GameTitleText : MonoBehaviour
{
    [Tooltip("Shown when no GameLaunchConfig is set (e.g. testing this scene directly in the editor).")]
    [SerializeField] string fallbackTitle;

    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // GetLocalizedString() needs Localization to have initialized (SelectedLocale
        // resolved), which only happens in real Play/build — guard against edit-time
        // activation (e.g. dragging a prefab with this into the scene) throwing.
        label.text = Application.isPlaying && GameLaunchConfig.Current != null
            ? LocalizationUtil.Resolve(GameLaunchConfig.Current.title, "NoName")
            : fallbackTitle;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RunGameButton : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] SceneReference scene;

    [Header("Content (optional)")]
    [Tooltip("Turn off for a plain navigation button whose label/icon is managed elsewhere (e.g. Localize String Event, SelectableButton) — RunGameButton then only handles the click/scene load, never touching the label or icon GameObjects.")]
    [SerializeField] bool manageContent = true;
    [SerializeField] string label;
    [SerializeField] Sprite icon;
    [SerializeField] Color iconColor = Color.white;

    [Header("Game (optional)")]
    [Tooltip("If set, this button launches a specific game in a shared gameplay scene: its title overrides Label, and it's passed via GameLaunchConfig before the scene loads.")]
    [SerializeField] GameInfo gameInfo;

    Button button;
    TMP_Text labelText;
    Image iconImage;

    void Awake()
    {
        button = GetComponent<Button>();
        CacheContentTargets();
        ApplyContent();
    }

    void OnEnable()
    {
        button.onClick.AddListener(LoadScene);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(LoadScene);
    }

    void LoadScene()
    {
        if (string.IsNullOrEmpty(scene.SceneName))
        {
            Debug.LogError("[RunGameButton] Scene is not assigned.", this);
            return;
        }

        // Always assign (even null) — GameLaunchConfig.Current is static and
        // survives scene loads, so a plain button (no gameInfo) must clear
        // whatever a previous game's button left behind, or the next scene's
        // GameSession picks up stale identity from an unrelated game.
        GameLaunchConfig.Current = gameInfo;

        SceneManager.LoadScene(scene.SceneName);
    }

    void CacheContentTargets()
    {
        if (labelText == null) labelText = GetComponentInChildren<TMP_Text>(true);
        if (iconImage == null) iconImage = FindIconImage();
    }

    // the button's own Image (background/target graphic) lives on this GameObject,
    // the icon is expected to be a separate child Image
    Image FindIconImage()
    {
        foreach (var img in GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject != gameObject) return img;
        }
        return null;
    }

    void ApplyContent()
    {
        if (!manageContent) return;

        // GetLocalizedString() needs a resolved SelectedLocale, which only exists
        // once Localization has initialized (real Play/build) — calling it from
        // OnValidate's edit-time preview throws, so show the asset name instead.
        string displayLabel = label;
        if (gameInfo != null)
        {
            displayLabel = Application.isPlaying ? LocalizationUtil.Resolve(gameInfo.title, "NoName") : gameInfo.name;
        }

        if (labelText != null)
        {
            bool hasLabel = !string.IsNullOrEmpty(displayLabel);
            labelText.gameObject.SetActive(hasLabel);
            if (hasLabel) labelText.text = displayLabel;
        }

        if (iconImage != null)
        {
            bool hasIcon = icon != null;
            iconImage.gameObject.SetActive(hasIcon);
            if (hasIcon)
            {
                iconImage.sprite = icon;
                iconImage.color = iconColor;
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            CacheContentTargets();
            ApplyContent();
        };
    }
#endif
}

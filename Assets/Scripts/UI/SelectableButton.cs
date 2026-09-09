using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Generic 3-state button used across the whole game (language buttons, settings
// toggles, etc.): Normal / Selected / Disabled, each with its own background
// and text color. Selection is external — call SetSelected from whatever
// manages the group (e.g. only one language button selected at a time).
[RequireComponent(typeof(Button))]
public class SelectableButton : MonoBehaviour
{
    enum VisualState { Normal, Selected, Disabled }

    [Header("Targets")]
    [SerializeField] Image background;
    [SerializeField] TMP_Text label;

    [Header("Background colors")]
    [SerializeField] Color normalColor = Color.black;
    [SerializeField] Color selectedColor = new Color32(0, 114, 178, 255);
    [SerializeField] Color disabledColor = new Color32(180, 180, 180, 255);

    [Header("Text colors")]
    [SerializeField] Color normalTextColor = Color.white;
    [SerializeField] Color selectedTextColor = Color.white;
    [SerializeField] Color disabledTextColor = Color.white;

    public event System.Action Clicked;

    Button button;
    public bool IsSelected { get; private set; }

    void Awake()
    {
        button = GetComponent<Button>();
        CacheTargets();
        Refresh();
    }

    void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
    }

    void HandleClick()
    {
        Clicked?.Invoke();
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        Refresh();
    }

    public void SetInteractable(bool interactable)
    {
        GetComponent<Button>().interactable = interactable;
        Refresh();
    }

    void CacheTargets()
    {
        if (background == null) background = GetComponent<Image>();
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
    }

    void Refresh()
    {
        Button btn = button != null ? button : GetComponent<Button>();
        bool interactable = btn == null || btn.interactable;

        VisualState state = !interactable
            ? VisualState.Disabled
            : (IsSelected ? VisualState.Selected : VisualState.Normal);

        Color bg;
        Color text;
        switch (state)
        {
            case VisualState.Selected:
                bg = selectedColor;
                text = selectedTextColor;
                break;
            case VisualState.Disabled:
                bg = disabledColor;
                text = disabledTextColor;
                break;
            default:
                bg = normalColor;
                text = normalTextColor;
                break;
        }

        if (background != null) background.color = bg;
        if (label != null) label.color = text;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            CacheTargets();
            Refresh();
        };
    }
#endif
}

using UnityEngine;
using UnityEngine.UI;

// The script for "most buttons" that open a panel (Settings, Help, etc).
// Drag the panel's root GameObject into the Panel field.
[RequireComponent(typeof(Button))]
public class OpenPanelButton : MonoBehaviour
{
    [SerializeField] GameObject panel;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(Open);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(Open);
    }

    void Open()
    {
        PanelManager.Instance.Open(panel);
    }
}

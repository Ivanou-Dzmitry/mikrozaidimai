using UnityEngine;
using UnityEngine.UI;

// Goes on a panel's own close/back button. No panel reference needed — it always
// closes whichever panel is currently open (i.e. itself).
[RequireComponent(typeof(Button))]
public class ClosePanelButton : MonoBehaviour
{
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(Close);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(Close);
    }

    void Close()
    {
        PanelManager.Instance.CloseCurrent();
    }
}

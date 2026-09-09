using UnityEngine;
using UnityEngine.UI;

// Drop this on any Button to give it a click sound. Plays through AudioManager's
// UI sfx channel — exclusive with other UI sounds (won't overlap another click),
// but separate from the gameplay sfx channel, so it never cuts off a hit/score sound.
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] AudioClip clickSound;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(PlayClickSound);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        if (clickSound != null) AudioManager.Instance.PlayUiSfx(clickSound);
    }
}

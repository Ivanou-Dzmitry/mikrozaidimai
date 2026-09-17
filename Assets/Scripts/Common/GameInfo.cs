using System;
using UnityEngine;
using UnityEngine.Localization;

// Identity + content for one game that shares the common BubbleShooter scene:
// which WordSet to play, its save-key, and its title/description shown in the
// Lobby button and in-game UI. One GameInfo asset per game (Game01, Game02, ...).
[CreateAssetMenu(fileName = "GameInfo", menuName = "Games/Game Info")]
public class GameInfo : ScriptableObject
{
    public string gameId;
    public LocalizedString title;
    public LocalizedString description;
    public WordSet wordSet;
    [Tooltip("Optional pool of example sentences shown in the hint panel on a wrong answer — resolved by looking the word up in here, not wired per-pair.")]
    public ExampleBank exampleBank;
    [Tooltip("Only needed if this game's bubble MESH/shape differs from the default — for just a different color, use Appearance.bubbleGradient on the same shared prefab instead.")]
    public WordBubble bubblePrefab;
    public AppearanceSettings appearance = new AppearanceSettings();

    // Purely visual knobs that depend on the bubble prefab's look, grouped so a
    // new game can restyle without needing its own prefab variant.
    [Serializable]
    public class AppearanceSettings
    {
        public Gradient bubbleGradient;
        [Tooltip("Vertical row spacing (multiple of cell size). 1 = square, ~0.87 = tight hex packing.")]
        public float rowSpacingMultiplier = 1f;
    }
}

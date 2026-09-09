using UnityEngine;

// A single UI color palette shared across the whole game. Components hold a reference
// to an asset of this type instead of hardcoding colors individually.
// Only 2 colors for now, add more as needed.
[CreateAssetMenu(fileName = "MainPalette", menuName = "UI/Main Palette")]
public class MainPalette : ScriptableObject
{
    public Color black = Color.black;
    public Color white = Color.white;

    public Color blue = new Color32(0, 114, 178, 255);
}

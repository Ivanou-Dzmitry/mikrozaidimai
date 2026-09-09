using System.Collections.Generic;
using UnityEngine;

// Single ordered list of every game — the source of truth for "what comes
// after this one" (NextGameButton) and, later, for auto-generating the
// Lobby's game buttons instead of wiring each one by hand.
[CreateAssetMenu(fileName = "GameCatalog", menuName = "Games/Game Catalog")]
public class GameCatalog : ScriptableObject
{
    public List<GameInfo> games = new List<GameInfo>();

    public GameInfo GetNext(GameInfo current)
    {
        int index = games.IndexOf(current);
        if (index < 0 || index + 1 >= games.Count) return null;
        return games[index + 1];
    }
}

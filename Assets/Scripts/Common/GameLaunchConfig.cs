// Carries which GameInfo to run into the shared gameplay scene, set by the
// launcher button right before SceneManager.LoadScene. Plain static class —
// survives the scene load same as Settings/Records, no DontDestroyOnLoad object
// needed. Left null when a scene is opened directly (e.g. testing in the editor),
// in which case each reader falls back to whatever it has serialized locally.
public static class GameLaunchConfig
{
    public static GameInfo Current { get; set; }
}

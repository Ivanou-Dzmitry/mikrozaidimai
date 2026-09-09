using UnityEngine;

// Lets you drag a scene file in the Inspector instead of typing its name by hand.
// sceneAsset only exists in the editor; SceneReferenceDrawer (Assets/Editor) draws
// the field and keeps sceneName in sync with it.
[System.Serializable]
public class SceneReference
{
#if UNITY_EDITOR
    [SerializeField] Object sceneAsset;
#endif
    [SerializeField] string sceneName;

    public string SceneName => sceneName;
}

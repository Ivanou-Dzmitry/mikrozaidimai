using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var sceneAssetProp = property.FindPropertyRelative("sceneAsset");
        var sceneNameProp = property.FindPropertyRelative("sceneName");

        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginChangeCheck();
        var newSceneAsset = EditorGUI.ObjectField(position, label, sceneAssetProp.objectReferenceValue, typeof(SceneAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            sceneAssetProp.objectReferenceValue = newSceneAsset;
        }

        // keep sceneName in sync even if the scene asset was renamed on disk
        string currentName = sceneAssetProp.objectReferenceValue != null ? sceneAssetProp.objectReferenceValue.name : string.Empty;
        if (sceneNameProp.stringValue != currentName)
        {
            sceneNameProp.stringValue = currentName;
        }

        EditorGUI.EndProperty();
    }
}

using UnityEditor;
using UnityEngine;

// Draws each WordPair as one compact line (Id | Target | Var01 | Var02)
// instead of Unity's default expanded 4-line block per element — for a list of
// 10+ pairs this turns 40+ lines of scrolling into 10 tight rows.
[CustomPropertyDrawer(typeof(WordPair))]
public class WordPairDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty id = property.FindPropertyRelative("id");
        SerializedProperty target = property.FindPropertyRelative("target");
        SerializedProperty var01 = property.FindPropertyRelative("var01");
        SerializedProperty var02 = property.FindPropertyRelative("var02");

        position.height = EditorGUIUtility.singleLineHeight;

        const float gap = 3f;
        float idW = position.width * 0.2f;
        float targetW = position.width * 0.27f;
        float varW = (position.width - idW - targetW - gap * 3f) * 0.5f;

        float x = position.x;
        Rect idRect = NextRect(ref x, position.y, idW, position.height, gap);
        Rect targetRect = NextRect(ref x, position.y, targetW, position.height, gap);
        Rect var01Rect = NextRect(ref x, position.y, varW, position.height, gap);
        Rect var02Rect = new Rect(x, position.y, varW, position.height);

        EditorGUI.PropertyField(idRect, id, GUIContent.none);
        EditorGUI.PropertyField(targetRect, target, GUIContent.none);
        EditorGUI.PropertyField(var01Rect, var01, GUIContent.none);
        EditorGUI.PropertyField(var02Rect, var02, GUIContent.none);

        EditorGUI.EndProperty();
    }

    static Rect NextRect(ref float x, float y, float width, float height, float gap)
    {
        Rect rect = new Rect(x, y, width, height);
        x += width + gap;
        return rect;
    }
}

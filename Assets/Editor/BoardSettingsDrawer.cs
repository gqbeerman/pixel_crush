using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BoardSettings))]
public class BoardSettingsDrawer : PropertyDrawer
{
    int rowCount;
    int width = 7;
    int height = 7;

    override public void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Rect pos = position;
        pos.height = 16f;
        pos = EditorGUI.PrefixLabel(pos, new GUIContent("Width"));
        width = EditorGUI.IntField(pos, width);

        pos = position;
        pos.height = 16f;
        pos.y += 18f;
        pos = EditorGUI.PrefixLabel(pos, new GUIContent("Height"));
        height = EditorGUI.IntField(pos, height);


        SerializedProperty starters = property.FindPropertyRelative("starters");
        starters.arraySize = width;
        float sX = position.x;
        float sY = position.y + (18f * 2);
        float rWidth = position.width / width;
        for (int i = 0; i < width; i++) {
            SerializedProperty rowData = starters.GetArrayElementAtIndex(i).FindPropertyRelative("rowData");
            rowData.arraySize = height;
            for (int j = 0; j < height; j++) {
                pos = new Rect(sX + (i * rWidth), sY + (j * 18f), rWidth, 18f);
                EditorGUI.ObjectField(pos, rowData.GetArrayElementAtIndex(j), typeof(GameObject), GUIContent.none);
            }
        }

        EditorGUI.EndProperty();
    }

    override public float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return 18f * (height + 2);
    }
}

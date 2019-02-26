using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BoardSettings))]
public class BoardSettingsDrawer : PropertyDrawer
{
    SerializedProperty starters;
    SerializedProperty starterTiles;

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

        pos.y += 18f;
        Rect foldout = new Rect(position.x, pos.y, position.width, 18f);

        float sX = position.x;
        float rWidth = position.width / width;

        starters = property.FindPropertyRelative("starters");
        if (starters.isExpanded = EditorGUI.Foldout(foldout, starters.isExpanded, "Starting Pieces", true)) {
            if (starters.arraySize != width) {
                starters.arraySize = width;
            }
            float sY = pos.y + 18f;
            for (int i = 0; i < width; i++) {
                SerializedProperty rowData = starters.GetArrayElementAtIndex(i).FindPropertyRelative("rowData");
                if (rowData.arraySize != height) {
                    rowData.arraySize = height;
                }
                for (int j = 0; j < height; j++) {
                    pos = new Rect(sX + (i * rWidth), sY + (j * 18f), rWidth, 18f);
                    EditorGUI.ObjectField(pos, rowData.GetArrayElementAtIndex(j), typeof(GamePiece), GUIContent.none);
                }
            }
        }
        
        foldout.y += (starters.isExpanded) ? 18f * (height + 1) : 18f;
        starterTiles = property.FindPropertyRelative("starterTiles");
        if (starterTiles.isExpanded = EditorGUI.Foldout(foldout, starterTiles.isExpanded, "Starting Tiles", true)) {
            if (starterTiles.arraySize != width) {
                starterTiles.arraySize = width;
            }
            float sY = foldout.y + 18f;
            for (int i = 0; i < width; i++) {
                SerializedProperty rowData = starterTiles.GetArrayElementAtIndex(i).FindPropertyRelative("rowData");
                if (rowData.arraySize != height) {
                    rowData.arraySize = height;
                }
                for (int j = 0; j < height; j++) {
                    pos = new Rect(sX + (i * rWidth), sY + (j * 18f), rWidth, 18f);
                    EditorGUI.ObjectField(pos, rowData.GetArrayElementAtIndex(j), typeof(TilePiece), GUIContent.none);
                }
            }
        }

        EditorGUI.EndProperty();
    }

    override public float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // width, height, pieces foldout, tiles foldout
        int totalRows = 4;

        if (starters != null && starters.isExpanded) {
            totalRows += height;
        }
        if (starterTiles != null && starterTiles.isExpanded) {
            totalRows += height;
        }

        return 18f * totalRows;
    }
}

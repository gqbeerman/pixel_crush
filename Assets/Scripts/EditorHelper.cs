using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorHelper {

    public static void HorizontalIntArray (int[] data, string label = "", bool editable = false) {
        EditorGUILayout.BeginHorizontal();

        if (label.Length > 0) {
            EditorGUILayout.LabelField(label);
        }
        
        for (int i = 0; i < data.Length; i++) {
            if (editable) {
                data[i] = EditorGUILayout.IntField(data[i]);
            } else {
                EditorGUILayout.LabelField(data[i].ToString());
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
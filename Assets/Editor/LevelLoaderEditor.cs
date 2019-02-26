using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(LevelLoader))]
public class LevelLoaderEditor : Editor {

    LevelDataSO toLoad;
    string fName = "";

    LevelLoader lvlLoader;

    void OnEnable () {
        lvlLoader = target as LevelLoader;
    }

    override public void OnInspectorGUI() {
// Save
        EditorGUILayout.BeginHorizontal();
        fName = EditorGUILayout.TextField("Save As", fName);

        EditorGUI.BeginDisabledGroup(fName.Length == 0);
        if (GUILayout.Button("Save")) {
            lvlLoader.Save(fName);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
// Load
        EditorGUILayout.BeginHorizontal();
        toLoad = EditorGUILayout.ObjectField("Level Data SO", toLoad, typeof(LevelDataSO), false) as LevelDataSO;
        EditorGUI.BeginDisabledGroup(toLoad == null);
        if (GUILayout.Button("Load")) {
            lvlLoader.Load(toLoad);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }
}
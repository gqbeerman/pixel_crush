using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelDataSO))]
public class LevelDataEditor : Editor {
    LevelDataSO data;

    bool starterFoldout;
    bool tilesFoldout;

    public void OnEnable () {
        data = target as LevelDataSO;
    }

    public override void OnInspectorGUI() {
        
        int width, height;
        width = data.boardSettings.starters.Length;
        height = data.boardSettings.starters[0].rowData.Length;
        EditorGUILayout.LabelField("BoardSize", width.ToString() + "x" + height.ToString());

        if (starterFoldout = EditorGUILayout.Foldout(starterFoldout, "Starting Pieces", true)) {
            for (int y = 0; y < height; y++) {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < width; x++) {
                    data.boardSettings.starters[x].rowData[y] = (GamePiece)EditorGUILayout.ObjectField(data.boardSettings.starters[x].rowData[y], typeof(GamePiece), false);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        if (tilesFoldout = EditorGUILayout.Foldout(tilesFoldout, "Starting Tiles", true)) {
            for (int y = 0; y < height; y++) {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < width; x++) {
                    data.boardSettings.starterTiles[x].rowData[y] = (TilePiece)EditorGUILayout.ObjectField(data.boardSettings.starterTiles[x].rowData[y], typeof(TilePiece), false);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        if (data.goalSettings != null) {
            data.goalSettings.OnGui();
        }

        EditorUtility.SetDirty(data);
    }

}
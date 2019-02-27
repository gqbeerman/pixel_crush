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

        string[] strs = new string[3] {"Collection Goal", "Timed Goal", "Score Goal"};
        int selected = data.collectedEnabled ? 0 : data.timedEnabled ? 1 : 2;
        int index = GUILayout.SelectionGrid(selected, strs,strs.Length, EditorStyles.radioButton); 

        //data.collectedEnabled = EditorGUILayout.BeginToggleGroup("Collection Goal", data.collectedEnabled);
        //if (EditorGUILayout.Foldout(data.collectedEnabled, GUIContent.none)) {
        if (index == 0) {
            data.collectedEnabled = true;
            data.timedEnabled = data.scoreEnabled = false;
            data.collectedGoal.OnGui(); 
        }
        //EditorGUILayout.EndToggleGroup();

        //data.timedEnabled = EditorGUILayout.BeginToggleGroup("Timed Goal", data.timedEnabled);
        //if (EditorGUILayout.Foldout(data.timedEnabled, GUIContent.none))  {
        if (index == 1) {
            data.timedEnabled = true;
            data.scoreEnabled = data.collectedEnabled = false;
            data.timedGoal.OnGui();
        }
        //EditorGUILayout.EndToggleGroup();

        //data.scoreEnabled = EditorGUILayout.BeginToggleGroup("Score Goal", data.scoreEnabled);
        //if (EditorGUILayout.Foldout(data.scoreEnabled, GUIContent.none)) {
        if (index == 2) {
            data.scoreEnabled = true;
            data.collectedEnabled = data.timedEnabled = false;
            data.scoreGoal.OnGui();
        }
        //EditorGUILayout.EndToggleGroup();

        EditorUtility.SetDirty(data);
    }

}
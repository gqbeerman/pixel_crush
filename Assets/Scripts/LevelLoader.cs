using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[RequireComponent(typeof(Board))]
[ExecuteInEditMode]
public class LevelLoader : MonoBehaviour
{
    Board _board;
    
    // Start is called before the first frame update
    void Start()
    {
        _board = GetComponent<Board>();
    }

    public void Load (LevelDataSO toLoad) {
        if (toLoad) {
            _board.settings = toLoad.boardSettings;
            
            LevelGoal oldGoal, goal;
            System.Type goalType = toLoad.goalSettings.GetType();
            GameObject gO = GameManager.Instance.gameObject;
            oldGoal = gO.GetComponent<LevelGoal>();
            if (goalType == typeof(TimedGoalData)) {
                goal = gO.AddComponent(typeof(LevelGoalTimed)) as LevelGoal;
            }
            else if (goalType == typeof(CollectedGoalData)) {
                goal = gO.AddComponent(typeof(LevelGoalCollected)) as LevelGoal;
            }
            else {
                goal = gO.AddComponent(typeof(LevelGoalScored)) as LevelGoal;
            }
            goal.Load(toLoad.goalSettings);
            DestroyImmediate(oldGoal);
        }
    }

    public void Save (string fileName) {
        LevelGoal lG = GameManager.Instance.GetComponent<LevelGoal>();
        LevelDataSO data = CreateAsset<LevelDataSO>(fileName);
        data.boardSettings = _board.settings;
        data.goalSettings = lG.ForSave();
        EditorUtility.SetDirty(data);
    }

    T CreateAsset<T> (string fName = "") where T : ScriptableObject {
        T asset = ScriptableObject.CreateInstance<T> ();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "") {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "") {
            path = path.Replace(Path.GetFileName(path), "");
        }

        if (fName == "") {
            fName = "New " + typeof(T).ToString();
        }
        path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + fName + ".asset");
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        return asset;
    }
}

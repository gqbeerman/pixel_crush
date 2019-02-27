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
    GameManager _gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        _board = GetComponent<Board>();
        _gameManager = GameManager.Instance;
    }

    public void Load (LevelDataSO toLoad) {
        if (toLoad) {
            _board.settings = toLoad.boardSettings;
            
            LevelGoal oldGoal, goal;
            oldGoal = _gameManager.GetComponent<LevelGoal>();
            
            if (toLoad.timedEnabled) {
                goal = _gameManager.gameObject.AddComponent<LevelGoalTimed>() as LevelGoal;
                goal.Load(toLoad.timedGoal);
            }
            else if (toLoad.collectedEnabled) {
                goal = _gameManager.gameObject.AddComponent<LevelGoalCollected>() as LevelGoal;
                goal.Load(toLoad.collectedGoal);
            }
            else {
                goal = _gameManager.gameObject.AddComponent<LevelGoalScored>() as LevelGoal;
                goal.Load(toLoad.scoreGoal);
            }

            DestroyImmediate(oldGoal);
        }
    }

    public void Save (string fileName) {
        LevelGoalData goalData = _gameManager.GetComponent<LevelGoal>().ForSave();
        LevelDataSO data = CreateAsset<LevelDataSO>(fileName);
        data.boardSettings = _board.settings;

        if (goalData.GetType() == typeof(CollectedGoalData)) {
            data.collectedGoal = (CollectedGoalData)goalData;
            data.collectedEnabled = true;
        }
        else if (goalData.GetType() == typeof(TimedGoalData)) {
            data.timedGoal = (TimedGoalData)goalData;
            data.timedEnabled = true;
        }
        else {
            data.scoreGoal = goalData;
            data.scoreEnabled = true;
        }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class CollectedGoalData : LevelGoalData {
    public List<GamePiece> goalPrefabs;
    public List<int> goalQuantities;

    public override void OnGui () {
        base.OnGui();

        EditorGUILayout.LabelField("Collections");
        int count = Mathf.Min(goalPrefabs.Count, goalQuantities.Count);
        for (int i = 0; i < count; i++) {
            string str = goalPrefabs[i] ? goalPrefabs[i].name : "";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(str);
            goalPrefabs[i] = (GamePiece)EditorGUILayout.ObjectField(goalPrefabs[i], typeof(GamePiece), false);
            goalQuantities[i] = EditorGUILayout.IntField(goalQuantities[i]);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add")) {
            goalPrefabs.Add(null);
            goalQuantities.Add(0);
        }

        EditorGUI.BeginDisabledGroup(goalPrefabs.Count == 0);
        if (GUILayout.Button("Remove")) {
            goalPrefabs.RemoveAt(goalPrefabs.Count-1);
            goalQuantities.RemoveAt(goalPrefabs.Count-1);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
    }
}

public class LevelGoalCollected : LevelGoal {
    public CollectionGoal[] collectionGoals;
    public CollectionGoalPanel[] uiPanels;

    public override string goalsText { get {
        string ret = "Collect";
        foreach (CollectionGoal goal in collectionGoals) {
            ret += "\n" + goal.numberToCollect.ToString() + " " + goal.prefabToCollect.name;
        }
        return ret;
    } }

    override public LevelGoalData ForSave () {
        CollectedGoalData data = new CollectedGoalData();
        data.goalPrefabs = new List<GamePiece>();
        data.goalQuantities = new List<int>();

        if (collectionGoals == null || collectionGoals.Length == 0) {
            collectionGoals = GetComponentsInChildren<CollectionGoal>();
        }
        for (var i = 0; i < collectionGoals.Length; i++) {
            data.goalPrefabs.Add(collectionGoals[i].prefabToCollect);
            data.goalQuantities.Add(collectionGoals[i].numberToCollect);
        }
        data = BaseForSave(data) as CollectedGoalData;
        return data;
    }

    override public void Load(LevelGoalData data) {
        CollectedGoalData casted = data as CollectedGoalData;
        if (casted != null) {
            int count = casted.goalPrefabs.Count;
            collectionGoals = new CollectionGoal[count];
            for (int i = 0; i < count; i++) {
                CollectionGoal g = new GameObject().AddComponent<CollectionGoal>();
                g.transform.parent = transform;
                g.prefabToCollect = casted.goalPrefabs[i];
                g.numberToCollect = casted.goalQuantities[i];
                collectionGoals[i] = g;
            }
        }
        base.Load(data);
    }

    void Start () {
        if (collectionGoals == null || collectionGoals.Length == 0) {
            collectionGoals = GetComponentsInChildren<CollectionGoal>();
        }
    }

    void OnDestroy () {
        foreach (CollectionGoal goal in collectionGoals) {
            DestroyImmediate(goal);
        }
    }

    public void UpdateGoals(GamePiece pieceToCheck) {
        if(pieceToCheck != null) {
            foreach(CollectionGoal goal in collectionGoals) {
                if(goal != null) {
                    goal.CollectPiece(pieceToCheck);
                }
            }
        }
        UpdateUI();
    }

    public void UpdateUI() {
        foreach(CollectionGoalPanel panel in uiPanels) {
            if(panel != null) {
                panel.UpdatePanel();
            }
        }
    }

    bool AreGoalsComplete(CollectionGoal[] goals) {
        foreach(CollectionGoal g in goals) {
            if(g == null) {
                return false;
            }

            if(goals.Length <= 0) {
                return false;
            }

            if(g.numberToCollect != 0) {
                return false;
            }
        }
        return true;
    }

    public override bool IsGameOver() {
        if (AreGoalsComplete(collectionGoals)) {
            return true;
        }

        /* if we want the game to end when both the collection goal AND  max score achieved
        if (AreGoalsComplete(collectGoals) && ScoreManager.Instance != null) {
            int maxScore = scoreGoals[scoreGoals.Length - 1];
            if(ScoreManager.Instance.CurrentScore >= maxScore) {
                return true;
            }
        }*/
        return (movesLeft <= 0);
    }

    public override bool IsWinner() {
        if(ScoreManager.Instance != null) {
            return (AreGoalsComplete(collectionGoals));
            //if we want to base the completion goal on collectibles AND score
            //return (ScoreManager.Instance.CurrentScore >= scoreGoals[0] && AreGoalsComplete(collectGoals));
        }
        return false;
    }
}

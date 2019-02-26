using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class LevelGoalData {
    public int[] scoreGoals;
    public int movesLeft;

    public virtual void OnGui () {
        movesLeft = EditorGUILayout.IntField("Moves Left", movesLeft);
        EditorHelper.HorizontalIntArray(scoreGoals, "Score", true);
    }
}

public abstract class LevelGoal : MonoBehaviour {

    public int scoreStars = 0;
    public int[] scoreGoals = new int[3] { 1000, 2000, 3000 };

    public int movesLeft = 30;
    virtual public string movesLeftText { get { return movesLeft.ToString(); } }

    abstract public string goalsText { get; }

    abstract public LevelGoalData ForSave ();
    protected LevelGoalData BaseForSave (LevelGoalData data) {
        data.scoreGoals = (int[])scoreGoals.Clone();
        data.movesLeft = movesLeft;
        return data;
    }

    public virtual void Load (LevelGoalData data) {
        movesLeft = data.movesLeft;
        scoreGoals = (int[])data.scoreGoals.Clone();
    }

    // Start is called before the first frame update
    void Start() {
        Init();
    }

    void Init() {
        scoreStars = 0;
        for (int i = 1; i < scoreGoals.Length; i++) {
            if(scoreGoals[i] < scoreGoals[i - 1]) {
                Debug.LogWarning("make sure your goals are increasing order");
            }
        }
    }

    int UpdateScore(int score) {
        for (int i = 0; i < scoreGoals.Length; i++) {
            if(score < scoreGoals[i]) {
                return i;
            }
        }
        return scoreGoals.Length;
    }

    public void UpdateScoreStars(int score) {
        scoreStars = UpdateScore(score);
    }

    public abstract bool IsWinner();
    public abstract bool IsGameOver();
}

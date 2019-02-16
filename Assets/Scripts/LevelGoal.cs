using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelGoal : Singleton<LevelGoal> {
    public int scoreStars = 0;
    public int[] scoreGoals = new int[3] { 1000, 2000, 3000 };

    public int movesLeft = 30;

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

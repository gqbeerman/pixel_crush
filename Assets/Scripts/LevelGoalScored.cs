using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalScored : LevelGoal {

    public override string goalsText { get { return "Score at least\n" + scoreGoals[0].ToString(); } }

    override public LevelGoalData ForSave () {
        return BaseForSave(new LevelGoalData());
    }

    public override bool IsWinner() {
        //scoring higher than the lowest score equals a win
        if(ScoreManager.Instance != null) {
            return (ScoreManager.Instance.CurrentScore >= scoreGoals[0]);
        }
        return false;
    }

    public override bool IsGameOver() {
        int maxScore = scoreGoals[scoreGoals.Length - 1];

        //scoring higher than the lowest score equals a win
        if(ScoreManager.Instance.CurrentScore >= maxScore) {
            return true;
        }
        return (movesLeft == 0);
    }
}

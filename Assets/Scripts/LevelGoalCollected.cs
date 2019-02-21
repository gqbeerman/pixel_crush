using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

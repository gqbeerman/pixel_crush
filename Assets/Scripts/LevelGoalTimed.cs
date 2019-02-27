using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class TimedGoalData : LevelGoalData {
    public int timeLeft;

    public override void OnGui () {
        base.OnGui();
        timeLeft = EditorGUILayout.IntField("Time", timeLeft);
    }
}

public class LevelGoalTimed : LevelGoal {
    public int timeLeft = 60;

    public Timer timer;
    int m_maxTime;

    override public string movesLeftText { get { return "\u221e"; } }

    override public string goalsText { get { 
        return "Score at least " + scoreGoals[0].ToString() + " in under " + timeLeft.ToString() + "s"; 
    } }

    override public LevelGoalData ForSave () {
        TimedGoalData data = new TimedGoalData();
        data = BaseForSave(data) as TimedGoalData;
        data.timeLeft = timeLeft;
        return data;
    }

    override public void Load (LevelGoalData data) { 
        TimedGoalData casted = data as TimedGoalData;
        if (casted != null) {
            timeLeft = casted.timeLeft;
        }
        base.Load(data);
    }

    override protected void Init () {
        if(timer != null) {
            timer.InitTimer(timeLeft);
        }
        m_maxTime = timeLeft;
        movesLeft = int.MaxValue;
    }

    public void StartCountdown() {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine() {
        while(timeLeft > 0) {
            yield return new WaitForSeconds(1f);
            timeLeft--;

            if(timer != null) {
                timer.UpdateTimer(timeLeft);
            }
        }
    }

    public override bool IsWinner() {
        //scoring higher than the lowest score equals a win
        if (ScoreManager.Instance != null) {
            return (ScoreManager.Instance.CurrentScore >= scoreGoals[0]);
        }
        return false;
    }

    public override bool IsGameOver() {
        int maxScore = scoreGoals[scoreGoals.Length - 1];

        //scoring higher than the lowest score equals a win
        if (ScoreManager.Instance.CurrentScore >= maxScore) {
            return true;
        }

        return (timeLeft <= 0);
    }

    public void AddTime(int timeValue) {
        timeLeft += timeValue;
        timeLeft = Mathf.Clamp(timeLeft, 0, m_maxTime);

        if(timer != null) {
            timer.UpdateTimer(timeLeft);
        }
    }
}

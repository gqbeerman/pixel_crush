using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal {
    public Timer timer;
    int m_maxTime;

    override public string movesLeftText { get { return "\u221e"; } }

    override public string goalsText { get { 
        return "Score at least\n" + scoreGoals[0].ToString() + "\nin under\n" + timeLeft.ToString() + "s"; 
    } }

    void Start() {
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

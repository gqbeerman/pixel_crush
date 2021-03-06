﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelGoal))]
public class GameManager : Singleton<GameManager> {
    Board m_board;

    bool m_isReadyToBegin = false;

    public bool IsGameOver { get; set; } = false;

    bool m_isWinner = false;
    bool m_isReadyToReload = false;

    LevelGoal m_levelGoal;
    LevelGoalCollected m_levelGoalCollected;
    LevelGoalTimed m_levelGoalTimed;
    LevelGoalScored m_levelGoalScored;

    public LevelGoal LevelGoal { get { return m_levelGoal; } }

    public override void Awake() {
        base.Awake();
        m_levelGoal = GetComponent<LevelGoal>();
        m_levelGoalTimed = GetComponent<LevelGoalTimed>();
        m_levelGoalCollected = GetComponent<LevelGoalCollected>();

        //cache reference to goal
        m_board = FindObjectOfType<Board>().GetComponent<Board>();
    }

    // Start is called before the first frame update
    void Start() {
        //setup stars
        if(UIManager.Instance != null) {
            if (UIManager.Instance.scoreMeter != null) {
                UIManager.Instance.scoreMeter.SetupStars(m_levelGoal);
            }

            //use screen name as level name
            if (UIManager.Instance.levelNameText != null) {
                //get reference to current scene
                Scene scene = SceneManager.GetActiveScene();
                UIManager.Instance.levelNameText.text = scene.name;
            }

            if (UIManager.Instance.movesLeftText && m_levelGoalTimed) {
                UIManager.Instance.movesLeftText.fontSize = 70;
            }
            bool useTimer = m_levelGoal is LevelGoalTimed;
            UIManager.Instance.EnableTimer(useTimer);
            UIManager.Instance.EnableMovesCounter(!useTimer);
        }

        m_levelGoal.movesLeft++;
        UpdateMoves();
        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves() {
        m_levelGoal.movesLeft--;
        if (UIManager.Instance.movesLeftText && UIManager.Instance != null) {
            UIManager.Instance.movesLeftText.text = m_levelGoal.movesLeftText;
        }
    }

    public void BeginGame() {
        m_isReadyToBegin = true;
    }

    IEnumerator ExecuteGameLoop() {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");

        //wait for board to refill before ending
        yield return StartCoroutine("WaitForBoardRoutine", 0.5f);
        yield return StartCoroutine("EndGameRoutine");
    }

    IEnumerator StartGameRoutine() {
        if(UIManager.Instance.messageWindow != null && UIManager.Instance != null) {
            UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
            int maxGoal = m_levelGoal.scoreGoals.Length - 1;
            UIManager.Instance.messageWindow.ShowScoreMessage(LevelGoal.goalsText);

            if(m_levelGoalTimed) {
                UIManager.Instance.messageWindow.ShowTimeGoal(m_levelGoalTimed.timeLeft); //(((LevelGoalTimed)m_levelGoal).timeLeft);
            } else if (m_levelGoalCollected) {
                UIManager.Instance.messageWindow.ShowMovesGoal(m_levelGoal.movesLeft, 0, 0);
            } else {
                UIManager.Instance.messageWindow.ShowMovesGoal(m_levelGoal.movesLeft, 0, 35);
            }

            if(m_levelGoalCollected != null) {
                UIManager.Instance.messageWindow.ShowCollectionGoal(true);
                GameObject goalLayout = UIManager.Instance.messageWindow.collectionGoalLayout;

                if(goalLayout != null) {
                    UIManager.Instance.SetupCollectionGoalLayout(m_levelGoalCollected.collectionGoals, goalLayout, 80);
                }
            } else {
                UIManager.Instance.messageWindow.ShowCollectionGoal(false);
            }
        }
        while (!m_isReadyToBegin) {
            yield return null;
        }

        if(UIManager.Instance.screenFader != null && UIManager.Instance != null) {
            UIManager.Instance.screenFader.FadeOff();
        }

        yield return new WaitForSeconds(1f);
        if(m_board != null) {
            m_board.SetupBoard();
        }
    }

    IEnumerator PlayGameRoutine() {
        if(m_levelGoalTimed) {
            m_levelGoalTimed.StartCountdown(); //(m_levelGoal as LevelGoalTimed).StartCountdown();
        }
        //while the end game condition is not true, keep playing
        //keep waiting a frame and check game condition
        while (!IsGameOver) {
            IsGameOver = m_levelGoal.IsGameOver();
            m_isWinner = m_levelGoal.IsWinner();
            yield return null;
        }
    }

    IEnumerator WaitForBoardRoutine(float delay = 0f) {
        if(m_levelGoal is LevelGoalTimed) {
            LevelGoalTimed levelGoalTimer = (LevelGoalTimed)m_levelGoal;
            if(levelGoalTimer.timer != null) {
                levelGoalTimer.timer.FadeOff();
                levelGoalTimer.timer.paused = true;
            }
        }

        if (m_board != null) {
            //wait for board class swap time
            yield return new WaitForSeconds(m_board.swapTime);

            while (m_board.isRefilling) {
                yield return null;
            }
        }
        yield return new WaitForSeconds(delay);
    }

    IEnumerator EndGameRoutine() {
        m_isReadyToReload = false;

        if (m_isWinner) {
            ShowWinScreen();
        } else {
            ShowLoseScreen();
        }

        yield return new WaitForSeconds(2f);
        if (UIManager.Instance.screenFader != null && UIManager.Instance != null) {
            UIManager.Instance.screenFader.FadeOn();
        }

        while (!m_isReadyToReload) {
            yield return null;
        }
        //just reload for now
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ShowLoseScreen() {
        //Debug.Log("YOU LOSE");
        if (UIManager.Instance.messageWindow != null && UIManager.Instance != null) {
            UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowLoseMessage();
            UIManager.Instance.messageWindow.ShowCollectionGoal(false);

            if(UIManager.Instance.messageWindow.goalFailIcon != null) {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalFailIcon);
            }

            string caption = "";
            if (m_levelGoalTimed) {
                caption = "out of time!";
            } else {
                caption = "out of moves!";
            }
            UIManager.Instance.messageWindow.ShowGoalCaption(caption, 0, 35);
        }

        //play lose sound
        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlayLoseSound();
        }
    }

    void ShowWinScreen() {
        //Debug.Log("YOU WIN");
        if (UIManager.Instance.messageWindow != null && UIManager.Instance != null) {
            UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowWinMessage();
            UIManager.Instance.messageWindow.ShowCollectionGoal(false);

            if(ScoreManager.Instance != null) {
                string scoreString = "you scored\n" + ScoreManager.Instance.CurrentScore.ToString() + " points";
                if (m_levelGoalCollected) {
                    UIManager.Instance.messageWindow.ShowGoalCaption(scoreString, 0, 35);
                } else {
                    UIManager.Instance.messageWindow.ShowGoalCaption(scoreString, 0, 0);
                }

            }

            if(UIManager.Instance.messageWindow.goalCompleteIcon != null) {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalCompleteIcon);
            }
        }

        //play win sound
        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlayWinSound();
        }
    }

    public void ReloadScene() {
        m_isReadyToReload = true;
    }

    public void ScorePoints(GamePiece piece, int multiplier = 1, int bonus = 0) {
        if(piece != null) {
            if (ScoreManager.Instance != null) {
                ScoreManager.Instance.AddScore(piece.scoreValue * multiplier + bonus);
                m_levelGoal.UpdateScoreStars(ScoreManager.Instance.CurrentScore);

                //update score meter
                if(UIManager.Instance.scoreMeter != null && UIManager.Instance != null) {
                    UIManager.Instance.scoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore, m_levelGoal.scoreStars);
                }
            }

            if (SoundManager.Instance != null && piece.clearSound != null) {
                SoundManager.Instance.PlayClipAtPoint(piece.clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
            }
        }
    }

    public void AddTime(int timeValue) {
        if(m_levelGoalTimed) {
            m_levelGoalTimed.AddTime(timeValue); //(m_levelGoal as LevelGoalTimed).AddTime(timeValue);
        }
    }

    public void UpdateCollectionGoals(GamePiece pieceToCheck) {
        if(m_levelGoalCollected) {
            m_levelGoalCollected.UpdateGoals(pieceToCheck); //(m_levelGoal as LevelGoalCollected).UpdateGoals(pieceToCheck);
        }
    }
}

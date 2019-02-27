using System.Collections;
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

    public Sprite loseIcon;
    public Sprite winIcon;
    public Sprite goalIcon;

    LevelGoal m_levelGoal;

    public LevelGoal LevelGoal { get { return m_levelGoal; } }

    public override void Awake() {
        base.Awake();
        m_levelGoal = GetComponent<LevelGoal>();

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

            if (UIManager.Instance.movesLeftText && m_levelGoal is LevelGoalTimed) {
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
            UIManager.Instance.messageWindow.ShowMessage(goalIcon, LevelGoal.goalsText, "start");
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
        if(m_levelGoal is LevelGoalTimed) {
            (m_levelGoal as LevelGoalTimed).StartCountdown();
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
            //Debug.Log("YOU WIN");
            if(UIManager.Instance.messageWindow != null && UIManager.Instance != null) {
                UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowMessage(winIcon, "YOU WIN!", "OK");
            }

            //play win sound
            if(SoundManager.Instance != null) {
                SoundManager.Instance.PlayWinSound();
            }
        } else {
            //Debug.Log("YOU LOSE");
            if (UIManager.Instance.messageWindow != null && UIManager.Instance != null) {
                UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowMessage(loseIcon, "YOU LOSE!", "OK");
            }

            //play lose sound
            if (SoundManager.Instance != null) {
                SoundManager.Instance.PlayLoseSound();
            }
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
        if(m_levelGoal is LevelGoalTimed) {
            (m_levelGoal as LevelGoalTimed).AddTime(timeValue);
        }
    }

    public void UpdateCollectionGoals(GamePiece pieceToCheck) {
        if(m_levelGoal is LevelGoalCollected) {
            (m_levelGoal as LevelGoalCollected).UpdateGoals(pieceToCheck);
        }
    }
}

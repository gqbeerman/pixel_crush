using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelGoal))]
public class GameManager : Singleton<GameManager> {
    //public int movesLeft = 30;
    //public int scoreGoal = 10000;

    public ScreenFader screenFader;
    public Text levelNameText;

    public Text movesLeftText;

    Board m_board;

    bool m_isReadyToBegin = false;

    public bool IsGameOver { get; set; } = false;

    bool m_isWinner = false;
    bool m_isReadyToReload = false;

    public MessageWindow messageWindow;

    public Sprite loseIcon;
    public Sprite winIcon;
    public Sprite goalIcon;
    public ScoreMeter scoreMeter;

    LevelGoal m_levelGoal;

    LevelGoalTimed m_levelGoalTimed;

    public override void Awake() {
        base.Awake();
        m_levelGoal = GetComponent<LevelGoal>();
        m_levelGoalTimed = GetComponent<LevelGoalTimed>();

        //cache reference to goal
        m_board = FindObjectOfType<Board>().GetComponent<Board>();
    }

    // Start is called before the first frame update
    void Start() {
        //setup stars
        if(scoreMeter != null) {
            scoreMeter.SetupStars(m_levelGoal);
        }
        //get reference to current scene
        Scene scene = SceneManager.GetActiveScene();

        //use screen name as level name
        if(levelNameText != null) {
            levelNameText.text = scene.name;
        }
        m_levelGoal.movesLeft++;
        UpdateMoves();
        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves() {
        if(m_levelGoalTimed == null) {
            m_levelGoal.movesLeft--;

            if (movesLeftText != null) {
                movesLeftText.text = m_levelGoal.movesLeft.ToString();
            }
        } else {
            if(movesLeftText != null) {
                movesLeftText.text = "\u221E";
                movesLeftText.fontSize = 70;
            }
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
        if(messageWindow != null) {
            messageWindow.GetComponent<RectXFormMover>().MoveOn();
            messageWindow.ShowMessage(goalIcon, "score goal\n" + m_levelGoal.scoreGoals[0].ToString(), "start");
        }
        while (!m_isReadyToBegin) {
            yield return null;
            //yield return new WaitForSeconds(2f);
            //m_isReadyToBegin = true;
        }

        if(screenFader != null) {
            screenFader.FadeOff();
        }

        yield return new WaitForSeconds(1f);
        if(m_board != null) {
            m_board.SetupBoard();
        }
    }

    IEnumerator PlayGameRoutine() {
        if(m_levelGoalTimed != null) {
            m_levelGoalTimed.StartCountdown();
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
        if(m_board != null) {
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
            if(messageWindow != null) {
                messageWindow.GetComponent<RectXFormMover>().MoveOn();
                messageWindow.ShowMessage(winIcon, "YOU WIN!", "OK");
            }

            //play win sound
            if(SoundManager.Instance != null) {
                SoundManager.Instance.PlayWinSound();
            }
        } else {
            //Debug.Log("YOU LOSE");
            if (messageWindow != null) {
                messageWindow.GetComponent<RectXFormMover>().MoveOn();
                messageWindow.ShowMessage(loseIcon, "YOU LOSE!", "OK");
            }

            //play lose sound
            if (SoundManager.Instance != null) {
                SoundManager.Instance.PlayLoseSound();
            }
        }

        yield return new WaitForSeconds(2f);
        if (screenFader != null) {
            screenFader.FadeOn();
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
                if(scoreMeter != null) {
                    scoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore, m_levelGoal.scoreStars);
                }
            }

            if (SoundManager.Instance != null && piece.clearSound != null) {
                SoundManager.Instance.PlayClipAtPoint(piece.clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
            }
        }
    }
}

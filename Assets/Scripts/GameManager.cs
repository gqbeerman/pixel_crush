using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {
    public int movesLeft = 30;
    public int scoreGoal = 10000;

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

    // Start is called before the first frame update
    void Start() {
        m_board = FindObjectOfType<Board>().GetComponent<Board>();
        Scene scene = SceneManager.GetActiveScene();

        if(levelNameText != null) {
            levelNameText.text = scene.name;
        }
        UpdateMoves();
        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves() {
        if(movesLeftText != null) {
            movesLeftText.text = movesLeft.ToString();
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
            messageWindow.ShowMessage(goalIcon, "score goal\n" + scoreGoal.ToString(), "start");
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
        while (!IsGameOver) {
            if(ScoreManager.Instance != null) {
                if(ScoreManager.Instance.CurrentScore >= scoreGoal) {
                    IsGameOver = true;
                    m_isWinner = true;
                }
            }
            if (movesLeft <= 0) {
                IsGameOver = true;
                m_isWinner = false;
            }
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
}

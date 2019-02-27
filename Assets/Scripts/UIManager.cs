using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager> {
    public ScreenFader screenFader;
    public Text levelNameText;
    public Text movesLeftText;
    public ScoreMeter scoreMeter;
    public MessageWindow messageWindow;
    public Timer timer;
    public GameObject movesCounter;
    public GameObject collectionGoalLayout;

    public override void Awake() {
        base.Awake();

        if(messageWindow != null) {
            messageWindow.gameObject.SetActive(true);
        }

        if(screenFader!= null) {
            screenFader.gameObject.SetActive(true);
        }
    }

    public void EnableTimer(bool state) {
        if(timer != null) {
            timer.gameObject.SetActive(state);
        }
    }

    public void EnableMovesCounter(bool state) {
        if(movesCounter != null) {
            movesCounter.SetActive(state);
        }
    }

    public void EnableCollectionGoalLayout(bool state) {
        if(collectionGoalLayout != null) {
            collectionGoalLayout.SetActive(state);
        }
    }
}

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

    public void SetupCollectionGoalLayout(CollectionGoal[] collectionGoals, GameObject goalLayout, int spacingWidth) {
        if(goalLayout != null && collectionGoals != null && collectionGoals.Length != 0) {
            RectTransform rectXform = goalLayout.GetComponent<RectTransform>();
            rectXform.sizeDelta = new Vector2(collectionGoals.Length * spacingWidth, rectXform.sizeDelta.y);

            CollectionGoalPanel[] panels = goalLayout.GetComponentsInChildren<CollectionGoalPanel>(true);

            Debug.Log("we here");
            for (int i = 0; i < panels.Length; i++) {
                if(i < collectionGoals.Length && collectionGoals[i] != null) {
                    panels[i].gameObject.SetActive(true);
                    panels[i].collectionGoal = collectionGoals[i];
                    //Debug.Log(panels[i].collectionGoal.prefabToCollect + " collection goal");
                    panels[i].SetupPanel();
                } else {
                    panels[i].gameObject.SetActive(false);
                }
            }
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

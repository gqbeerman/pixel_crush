using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour {
    public Image messageImage;
    public Text messageText;
    public Text buttonText;

    public Sprite loseIcon;
    public Sprite winIcon;
    public Sprite goalIcon;

    public Sprite collectIcon;
    public Sprite timerIcon;
    public Sprite movesIcon;

    public Image goalImage;
    public Text goalText;

    public GameObject collectionGoalLayout;

    public void ShowMessage(Sprite sprite = null, string message = "", string buttonMsg = "Start") {
        if(messageImage != null) {
            messageImage.sprite = sprite;
        }

        if(messageText != null) {
            messageText.text = message;
        }

        if(buttonText != null) {
            buttonText.text = buttonMsg;
        }
    }

    public void ShowScoreMessage(string message) {
        ShowMessage(goalIcon, message, "Start");
    }

    public void ShowWinMessage() {
        ShowMessage(winIcon, "Level\nComplete", "OK");
    }

    public void ShowLoseMessage() {
        ShowMessage(loseIcon, "Level\nFailed", "OK");
    }

    public void ShowGoal(string caption = "", Sprite icon = null) {
        if(goalText != null && caption != "") {
            goalText.text = caption;
        }

        if(goalImage != null && icon != null) {
            goalImage.sprite = icon;
        }
    }

    public void ShowTimeGoal (int time) {
        string caption = time.ToString() + " seconds";
        ShowGoal(caption, timerIcon);
    }

    public void ShowMovesGoal(int moves) {
        string caption = moves.ToString() + " moves";
        ShowGoal(caption, movesIcon);
    }

    public void ShowCollectionGoal(bool state = true) {
        if(collectionGoalLayout != null) {
            collectionGoalLayout.SetActive(state);
        }

        if (state) {
            ShowGoal("", collectIcon);
        }

    }
}

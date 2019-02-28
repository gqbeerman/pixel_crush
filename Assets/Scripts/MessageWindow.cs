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

    public Sprite goalCompleteIcon;
    public Sprite goalFailIcon;

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
        if(caption != "") {
            ShowGoalCaption(caption);
        }

        if(icon != null) {
            ShowGoalImage(icon);
        }
    }

    public void ShowGoalCaption(string caption = "", int offsetX = 0, int offsetY = 0) {
        if(goalText != null) {
            goalText.text = caption;
            RectTransform rectTransform = goalText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition += new Vector2(offsetX, offsetY);
        }
    }

    public void ShowGoalImage(Sprite icon = null) {
        if(goalImage != null) {
            goalImage.gameObject.SetActive(true);
            goalImage.sprite = icon;
        }

        if(icon == null) {
            goalImage.gameObject.SetActive(false);
        }
    }

    public void ShowTimeGoal (int time) {
        string caption = time.ToString() + " seconds";
        //ShowGoal(caption, timerIcon);
        ShowGoalImage(timerIcon);
        ShowGoalCaption(caption, 0, 35);
    }

    public void ShowMovesGoal(int moves, int xOffset = 0, int yOffset = 0) {
        string caption = moves.ToString() + " moves";
        //ShowGoal(caption, movesIcon);
        ShowGoalImage(movesIcon);
        ShowGoalCaption(caption, xOffset, yOffset);
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

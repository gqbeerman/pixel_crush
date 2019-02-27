using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour {
    public Image messageIcon;
    public Text messageText;
    public Text buttonText;

    public Sprite loseIcon;
    public Sprite winIcon;
    public Sprite goalIcon;

    public void ShowMessage(Sprite sprite = null, string message = "", string buttonMsg = "Start") {
        if(messageIcon != null) {
            messageIcon.sprite = sprite;
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
}

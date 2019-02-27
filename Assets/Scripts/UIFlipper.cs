using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFlipper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Timer timer = GetComponentInChildren<Timer>(true);
        CollectionPanelSpawner collection = GetComponentInChildren<CollectionPanelSpawner>(true);
        timer.gameObject.SetActive(false);
        collection.gameObject.SetActive(false);

        LevelGoal goal = GameManager.Instance.GetComponent<LevelGoal>();
        if (goal is LevelGoalCollected) {
            collection.gameObject.SetActive(true);
        }
        else if (goal is LevelGoalTimed) {
            timer.gameObject.SetActive(true);
        }
        
    }
}

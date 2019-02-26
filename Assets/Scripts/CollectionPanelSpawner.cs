using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionPanelSpawner : MonoBehaviour
{
    public CollectionGoalPanel prefab;

    // Start is called before the first frame update
    void Start()
    {
        CollectionGoalPanel[] children = GetComponentsInChildren<CollectionGoalPanel>();
        int i;
        for (i = 0; i < children.Length; i++) { // clean up any extras
            DestroyImmediate(children[i]);
        }

        LevelGoalCollected goal = GameManager.Instance.LevelGoal as LevelGoalCollected;
        if (goal != null) {
            children = new CollectionGoalPanel[goal.collectionGoals.Length];
            for (i = 0; i < goal.collectionGoals.Length; i++) {
                if (children[i] == null) {
                    children[i] = Instantiate<CollectionGoalPanel>(prefab, transform);
                }
                children[i].collectionGoal = goal.collectionGoals[i];
            }
        }
    }
}

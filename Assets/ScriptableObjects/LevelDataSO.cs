using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class LevelDataSO : ScriptableObject {
    
    public BoardSettings boardSettings;
    public LevelGoalData scoreGoal;
    public TimedGoalData timedGoal;
    public CollectedGoalData collectedGoal;
    public bool scoreEnabled;
    public bool timedEnabled;
    public bool collectedEnabled;
}

[Serializable]
public class BoardSettings {
    public Rows[] starters;
    public TileRows[] starterTiles;

    [Serializable]
    public struct Rows {
        public GamePiece[] rowData;
    }

    [Serializable]
    public struct TileRows {
        public TilePiece[] rowData;
    }
}

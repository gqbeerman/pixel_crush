using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class LevelDataSO : ScriptableObject {
    
    [HideInInspector] public BoardSettings boardSettings;
    public LevelGoalData goalSettings;
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

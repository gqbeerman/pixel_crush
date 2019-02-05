using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GamePiece {
    public bool clearedByBomb = false;
    public bool clearedAtBottom = false;

    // Start is called before the first frame update
    void Start() {
        matchValue = MatchValue.none;
    }

    // Update is called once per frame
    void Update() {
        
    }
}

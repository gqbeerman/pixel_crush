﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour {
    [Header("Board Attributes")]
    public int width;
    public int height;
    public int borderSize;

    [Header("Tiling")]
    public GameObject tileNormalPrefab;

    [Header("Color Pieces")]
    public GameObject[] gamePiecePrefabs;

    [Header("Obstacles")]
    public GameObject tileObstaclePrefab;

    [Header("Collectibles")]
    public GameObject[] collectiblePrefabs;
    public int maxCollectibles = 3;
    public int collectibleCount = 0;
    [Range(0, 1)]
    public float chanceForCollectible = 0.1f;

    [Header("Power Ups")]
    public GameObject adjacentBombPrefab;
    public GameObject columnBombPrefab;
    public GameObject rowBombPrefab;
    public GameObject ColorBomPrefab;

    GameObject m_clickedTileBomb;
    GameObject m_targetTileBomb;

    [Header("Misc Values")]
    public float swapTime = 0.25f;
    public int fillYOffset = 10;
    public float fillMoveTime = 0.5f;

    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    Tile m_clickedTile;
    Tile m_targetTile;

    bool m_playerInputEnabled = true;

    [Header("Seed Board")]
    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;

    ParticleManager m_particleManager;

    int m_scoreMultiplier = 0;

    public bool isRefilling = false;

    [System.Serializable]
    public class StartingObject {
        public GameObject prefab;
        public int x;
        public int y;
        public int z;
    }

    // Start is called before the first frame update
    void Start() {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_particleManager = GameObject.FindWithTag("Particle Manager").GetComponent<ParticleManager>();
    }

    public void SetupBoard() {
        SetupTiles();
        SetupGamePieces();

        List<GamePiece> startingCollectibles = FindAllCollectibles();
        collectibleCount = startingCollectibles.Count;

        SetupCamera();
        FillBoard(fillYOffset, fillMoveTime);
    }

    void MakeTile(GameObject prefab, int x, int y, int z = 0) {
        if (prefab != null && IsWithinBounds(x, y)) {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";

            m_allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            m_allTiles[x, y].Init(x, y, this);
        }
    }

    void MakeGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f) {
        if (prefab != null && IsWithinBounds(x, y)) {
            prefab.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0) {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
            }
            prefab.transform.parent = transform;
        }
    }

    GameObject MakeBomb(GameObject prefab, int x, int y) {
        if(prefab != null && IsWithinBounds(x, y)) {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
            bomb.GetComponent<Bomb>().Init(this);
            bomb.GetComponent<Bomb>().SetCoord(x, y);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }

    void SetupTiles() {
        foreach(StartingObject sTile in startingTiles) {
            if(sTile != null) {
                MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
            }
        }
        for (int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(m_allTiles[i, j] == null) {
                    MakeTile(tileNormalPrefab, i, j);
                }
            }
        }
    }

    void SetupGamePieces() {
        foreach(StartingObject sPiece in startingGamePieces) {
            if(sPiece != null) {
                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;
                MakeGamePiece(piece, sPiece.x, sPiece.y, fillYOffset, fillMoveTime);
            }
        }
    }

    //camera shifts to fit board
    void SetupCamera() {
        Camera.main.transform.position = new Vector3((width - 1) / 2.0f, (height - 1) / 2.0f, -10f);
        float aspectRatio = (float) Screen.width / (float) Screen.height;
        float verticalSize = (height / 2.0f) + borderSize;
        float horizontalSize = ((width / 2.0f) + borderSize) / aspectRatio;
        //Debug.Log("aspect ratio = " + aspectRatio + " vertical = " + verticalSize + " horizontal = " + horizontalSize);
        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

    }

    GameObject GetRandomObject(GameObject[] objectArray) {
        int randomIdx = Random.Range(0, objectArray.Length);

        if(objectArray[randomIdx] == null) {
            Debug.Log("Board.GetRandomObject at index " + randomIdx + " does not contain a valid Gameobject");
        }
        return objectArray[randomIdx];
    }

    GameObject GetRandomGamePiece() {
        return GetRandomObject(gamePiecePrefabs);
    }

    GameObject GetRandomCollectible() {
        return GetRandomObject(collectiblePrefabs);
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y) {
        if(gamePiece == null) {
            Debug.Log("BOARD: Invalid Gamepiece");
            return;
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if(IsWithinBounds(x, y)) {
            m_allGamePieces[x, y] = gamePiece;
        }

        gamePiece.SetCoord(x, y);
    }

    bool IsWithinBounds(int x, int y) {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    void FillBoard(int falseYOffset = 0, float moveTime = 0.2f) {
        int maxIterations = 100;
        int iterations = 0;
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle) {
                    //GamePiece piece = null; 

                    if(j == height - 1 && CanAddCollectible()) {
                        FillRandomCollectibleAt(i, j, falseYOffset, moveTime);
                        collectibleCount++;
                    } else {
                        FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                        iterations = 0;

                        while (HasMatchOnFill(i, j)) {
                            ClearPieceAt(i, j);
                            FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                            iterations++;

                            if (iterations >= maxIterations) {
                                //Debug.Log("break ============= infinite loop");
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f) {
        if(IsWithinBounds(x, y)) {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    GamePiece FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f) {
        if(IsWithinBounds(x, y)) {
            GameObject randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    bool HasMatchOnFill(int x, int y, int minLength = 3) {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if (leftMatches == null) {
            leftMatches = new List<GamePiece>();
        }
        if(downwardMatches == null) {
            downwardMatches = new List<GamePiece>();
        }
        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }

    public void ClickTile(Tile tile) {
        if(m_clickedTile == null) {
            m_clickedTile = tile;
            //Debug.Log("clicked tile: " + tile.name);
        }
    }

    public void DragToTile(Tile tile) {
        if(m_clickedTile != null && IsNextTo(tile, m_clickedTile)) {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile() {
        if(m_clickedTile != null && m_targetTile != null) {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }

    void SwitchTiles(Tile clickedTile, Tile targetTile) {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile) {
        if (m_playerInputEnabled && !GameManager.Instance.IsGameOver) {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

            if(targetPiece != null && clickedPiece != null) {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickePieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                List<GamePiece> colorMatches = new List<GamePiece>();

                //matching rainbow to regular color
                if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece)) {
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorMatches = FindAllMatchValue(clickedPiece.matchValue);
                //match regular piece to rainbow piec
                } else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece)) {
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                //match rainbow to rainbow
                } else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece)) {
                    foreach (GamePiece piece in m_allGamePieces) {
                        if (!colorMatches.Contains(piece)) {
                            colorMatches.Add(piece);
                        }
                    }
                // match powerup to powerup
                } else if (clickedPiece is Bomb && targetPiece is Bomb) {
                    List<Bomb> combo = new List<Bomb>(){clickedPiece as Bomb, targetPiece as Bomb};
                    colorMatches = GetComboPieces(combo, targetTile.xIndex, targetTile.yIndex);
                    
                }

                //block move from happening if there is no match and not a rainbow piece
                if (targetPieceMatches.Count == 0 && clickePieceMatches.Count == 0 && colorMatches.Count == 0) {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                } else {
                    if(GameManager.Instance != null) {
                        GameManager.Instance.movesLeft--;
                        GameManager.Instance.UpdateMoves();
                    }

                    yield return new WaitForSeconds(swapTime);

                    //get direction and placement of bomb during swap
                    Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);
                    m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swipeDirection, clickePieceMatches);
                    m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swipeDirection, targetPieceMatches);

                    if (m_clickedTileBomb != null && targetPiece != null) {

                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(clickedBombPiece)) {
                            clickedBombPiece.ChangeColor(targetPiece);
                        }
                    }
                    if (m_targetTileBomb != null && clickedPiece != null) {

                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(targetBombPiece)) {
                            targetBombPiece.ChangeColor(clickedPiece);
                        }
                    }
                    ClearAndRefillBoard(clickePieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList());
                }
            }
        }
    }

    bool IsNextTo(Tile start, Tile end) {
        if(Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex) {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex) {
            return true;
        }
        return false;
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3) {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if(IsWithinBounds(startX, startY)) {
            startPiece = m_allGamePieces[startX, startY];
        }

        if(startPiece != null) {
            matches.Add(startPiece);
        } else {
            return null;
        }
        int nextX;
        int nextY;
        int maxValue = (width > height) ? width : height;

        for(int i = 1; i < maxValue - 1; i++) {
            nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if(!IsWithinBounds(nextX, nextY)) {
                break;
            }
            GamePiece nextPiece = m_allGamePieces[nextX, nextY];
            if(nextPiece == null) {
                break;
            } else {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.none) {
                    matches.Add(nextPiece);
                } else {
                    break;
                }
            }
        }
        if(matches.Count >= minLength) {
            return matches;
        }
        return null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3) {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if(upwardMatches == null) {
            upwardMatches = new List<GamePiece>();
        }
        if(downwardMatches == null) {
            downwardMatches = new List<GamePiece>();
        }
        //combine list using system.linq (no more foreach)
        var combineMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combineMatches.Count >= minLength) ? combineMatches : null;

    }

    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3) {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null) {
            rightMatches = new List<GamePiece>();
        }
        if (leftMatches == null) {
            leftMatches = new List<GamePiece>();
        }
        //combine list using system.linq (no more foreach)
        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;

    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3) {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);

        if (horizMatches == null) {
            horizMatches = new List<GamePiece>();
        }
        if (vertMatches == null) {
            vertMatches = new List<GamePiece>();
        }
        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }

    List<GamePiece> FindMatchesWith(List<GamePiece> gamePieces, int minLength = 3) {
        List<GamePiece> totalMatches = new List<GamePiece>();

        foreach(GamePiece piece in gamePieces) {
            List<GamePiece> matched = FindMatchesAt(piece.xIndex, piece.yIndex, minLength);

            totalMatches = totalMatches.Union(matched).ToList();
        }
        return totalMatches;
    }

    List<GamePiece> FindAllMatches() {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                var matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    void HighlightTileOff(int x, int y) {
        if(m_allTiles[x, y].tileType != TileType.Breakable) {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    void HighlightTileOn(int x, int y, Color col) {
        if (m_allTiles[x, y].tileType != TileType.Breakable) {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = col;
        }
    }

    void HighlightMatchesAt(int x, int y) {
        HighlightTileOff(x, y);

        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0) {
            foreach (GamePiece piece in combinedMatches) {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightMatches() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                HighlightMatchesAt(i, j);
            }
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces) {
        foreach(GamePiece piece in gamePieces) {
            if (piece != null) {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void ClearPieceAt(int x, int y) {
        GamePiece pieceToClear = m_allGamePieces[x, y];
        if(pieceToClear != null) {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        HighlightTileOff(x, y);
    }

    void ClearBoard() {
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                ClearPieceAt(i, j);
            }
        }
    }

    void ClearPiecseAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces) {
        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                ClearPieceAt(piece.xIndex, piece.yIndex);
                int bonus = 0;
                if(gamePieces.Count >= 4) {
                    bonus = 20;
                }
                piece.ScorePoints(m_scoreMultiplier, bonus);

                if(m_particleManager != null) {
                    if (bombedPieces.Contains(piece)) {
                        m_particleManager.BombFXAt(piece.xIndex, piece.yIndex);
                    } else {
                        m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                    }
                }
            }
        }
    }

    void BreakTileAt(int x, int y) {
        Tile tileToBreak = m_allTiles[x, y];
        if(tileToBreak != null && tileToBreak.tileType == TileType.Breakable) {
            if(m_particleManager != null) {
                m_particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y, 0);
            }
            tileToBreak.BreakTile();
        }
    }

    void BreakTilesAt(List<GamePiece> gamePieces) {
        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f) {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for(int i = 0; i < height - 1; i++) {
            if(m_allGamePieces[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle) {
                for(int j = i + 1; j < height; j++) {
                    if(m_allGamePieces[column, j] != null) {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

                        if(!movingPieces.Contains(m_allGamePieces[column, i])) {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }
                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    // OLD: CollapseColumn
    List<GamePiece> StartCollapse(List<GamePiece> gamePieces) {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);
        foreach(int column in columnsToCollapse) {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        return movingPieces;
    }

    List<GamePiece> StartCollapse(List<int> columnsToCOllapse) {
        List<GamePiece> movingPieces = new List<GamePiece>();
        foreach(int column in columnsToCOllapse) {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        return movingPieces;

    }

    List<int> GetColumns(List<GamePiece> gamePieces) {
        List<int> columns = new List<int>();
        foreach(GamePiece piece in gamePieces) {
            if (!columns.Contains(piece.xIndex)) {
                columns.Add(piece.xIndex);
            }
        }
        return columns;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces) {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces) {
        m_playerInputEnabled = false;
        isRefilling = true;

        List<GamePiece> matches = gamePieces;
        //score multiplier
        m_scoreMultiplier = 0;

        do {
            m_scoreMultiplier++;

            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;

            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();

            yield return new WaitForSeconds(0.2f);
        } while (matches.Count != 0);

        m_playerInputEnabled = true;
        isRefilling = false;
    }

    IEnumerator RefillRoutine() {
        FillBoard(fillYOffset, fillMoveTime);
        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces) {
        //need to create bombs during collapse in this section
        //check the entire board for matches call DropBomb method

        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlightPieces(gamePieces);
        yield return new WaitForSeconds(0.2f);
        bool isFinished = false;

        while (!isFinished) {
            //find pieces hit by bomb
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            //bombs hit by other bombs
            bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            //collectible pieces that have hit the bottom of the board
            List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);

            //find blockers destroyed by bombs
            List<GamePiece> allCollectibles = FindAllCollectibles();
            List<GamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();
            //add blockers to list of collected pieces
            collectedPieces = collectedPieces.Union(blockers).ToList();
            //decrement cleared collectibles/blockers
            collectibleCount -= collectedPieces.Count;
            //add these collectibles to the list of game pieces to clear
            gamePieces = gamePieces.Union(collectedPieces).ToList();

            //fix for null reference error in GetColumns
            List<int> columnsToCollapse = GetColumns(gamePieces);

            ClearPiecseAt(gamePieces, bombedPieces);
            BreakTilesAt(gamePieces);

            //add bomb to collapsing pieces
            if(m_clickedTileBomb != null) {
                ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }
            if(m_targetTileBomb != null) {
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(0.25f);
            movingPieces = StartCollapse(columnsToCollapse);
            while (!FinishedCollapsing(movingPieces)) {
                yield return null;              // wait to respawn
            }
            yield return new WaitForSeconds(0.2f);

            //check for extra matches as result of collapse
            matches = FindMatchesWith(movingPieces);

            collectedPieces = FindCollectiblesAt(0, true);
            matches = matches.Union(collectedPieces).ToList();

            if((matches.Count == 0)) {
                isFinished = true;
                break;
            } else {
                m_scoreMultiplier++;
                if(SoundManager.Instance != null) {
                    SoundManager.Instance.PlayBonusSound();
                }
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    // OLD: IsCollapsed
    bool FinishedCollapsing(List<GamePiece> gamePieces) {
        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                if(piece.transform.position.y - (float)piece.yIndex > 0.001f) {
                    return false;
                }
            }
        }
        return true;
    }

    //for bombs
    List<GamePiece> GetRowPieces(int row) {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for(int i = 0; i < width; i++) {
            if(m_allGamePieces[i, row] != null) {
                gamePieces.Add(m_allGamePieces[i, row]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetColumnPieces(int column) {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++) {
            if (m_allGamePieces[column, i] != null) {
                gamePieces.Add(m_allGamePieces[column, i]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1) {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for(int i = x - offset; i <= x + offset; i++) {
            for(int j = y - offset; j <= y + offset; j++) {
                if (IsWithinBounds(i, j)) {
                    gamePieces.Add(m_allGamePieces[i, j]);
                }
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetComboPieces (List<Bomb> combo, int targetX, int targetY) {
        List<GamePiece> toClear = new List<GamePiece>();
        List<BombType> fired = new List<BombType>();

        foreach (Bomb piece in combo) {
            switch(piece.bombType) {
                case BombType.Column:
                    if (fired.IndexOf(piece.bombType) >= 0) {
                        toClear = toClear.Union(GetRowPieces(targetY)).ToList();
                    } else {
                        toClear = toClear.Union(GetColumnPieces(targetX)).ToList();
                        fired.Add(piece.bombType);
                    }
                    break;
                case BombType.Row:
                    if (fired.IndexOf(piece.bombType) >= 0) {
                        toClear = toClear.Union(GetColumnPieces(targetX)).ToList();
                    } else {
                        toClear = toClear.Union(GetRowPieces(targetY)).ToList();
                        fired.Add(piece.bombType);
                    }
                    break;
                case BombType.Adjacent:
                    toClear = toClear.Union(GetAdjacentPieces(targetX, targetY)).ToList();
                    break;
            }
        }
        return toClear;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces) {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                List<GamePiece> piecesToClear = new List<GamePiece>();
                Bomb bomb = piece.GetComponent<Bomb>();

                if(bomb != null) {
                    switch (bomb.bombType) {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:

                            break;
                    }
                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                    allPiecesToClear = RemoveCollectibles(allPiecesToClear);
                }
            }
        }
        return allPiecesToClear;
    }

    bool IsCornerMatch(List<GamePiece> gamePieces) {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                if(xStart == -1 || yStart == -1) {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }
            }

            if(piece.xIndex != xStart && piece.yIndex == yStart) {
                horizontal = true;
            }

            if(piece.xIndex == xStart && piece.yIndex != yStart) {
                vertical = true;
            }
        }
        return (horizontal && vertical);
    }

    GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces) {
        GameObject bomb = null;

        if(gamePieces.Count >= 4) {
            if (IsCornerMatch(gamePieces)) {
                if(adjacentBombPrefab != null) {
                    bomb = MakeBomb(adjacentBombPrefab, x, y);
                }
            } else {
                if(gamePieces.Count >= 5) {
                    if(columnBombPrefab != null) {
                        bomb = MakeBomb(ColorBomPrefab, x, y);
                    }
                } else {
                    if (swapDirection.x != 0) {
                        if (rowBombPrefab != null) {
                            bomb = MakeBomb(rowBombPrefab, x, y);
                        }
                    } else {
                        if (columnBombPrefab != null) {
                            bomb = MakeBomb(columnBombPrefab, x, y);
                        }
                    }
                }
            }
        }
        return bomb;
    }

    void ActivateBomb(GameObject bomb) {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if(IsWithinBounds(x, y)) {
            m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    List<GamePiece> FindAllMatchValue(MatchValue mValue) {
        List<GamePiece> foundPieces = new List<GamePiece>();

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(m_allGamePieces[i, j] != null) {
                    if(m_allGamePieces[i, j].matchValue == mValue) {
                        foundPieces.Add(m_allGamePieces[i, j]);
                    }
                }
            }
        }
        return foundPieces;
    }

    bool IsColorBomb(GamePiece gamePiece) {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if(bomb != null) {
            return bomb.bombType == BombType.Color;
        }
        return false;
    }

    List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false) {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for(int i = 0; i < width; i++) {
            if (m_allGamePieces[i, row] != null) {
                Collectible collectibleComponent = m_allGamePieces[i, row].GetComponent<Collectible>();
                if (collectibleComponent != null) {
                    if (!clearedAtBottomOnly || clearedAtBottomOnly && collectibleComponent.clearedAtBottom) {
                        foundCollectibles.Add(m_allGamePieces[i, row]);
                    }
                }
            }
        }
        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles() {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for(int i = 0; i < height; i++) {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }
        return foundCollectibles;
    }

    bool CanAddCollectible() {
        return Random.Range(0f, 1f) <= chanceForCollectible && collectiblePrefabs.Length > 0 && collectibleCount < maxCollectibles;
    }

    //remove collectibles from list of destroyable pieces
    List<GamePiece> RemoveCollectibles(List <GamePiece> bombedPieces) {
        List<GamePiece> collectiblePieces = FindAllCollectibles();
        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach(GamePiece piece in collectiblePieces) {
            Collectible collectibleComponent = piece.GetComponent<Collectible>();
            if(collectibleComponent != null) {
                if (!collectibleComponent.clearedByBomb) {
                    //Debug.Log("Pieces to remove " + piecesToRemove);
                    piecesToRemove.Add(piece);
                }
            }
        }
        return bombedPieces.Except(piecesToRemove).ToList();
    }
}

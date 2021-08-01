using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI
{
    public int teamNumber = 1;

    [Space(10)]

    public GameObject[] tokens;
    public Gameboard gameboard;
    public GameManager gameManager;
    public IGamePiece currentGamePiece;

    public EnemyAI(Gameboard gameboard, GameManager gameManager)
    {
        //this.tokens = tokens;
        this.gameboard = gameboard;
        this.gameManager = gameManager;
    }

    public void PlayTurn(IGamePiece currentGamePiece)
    {
        this.currentGamePiece = currentGamePiece;
        MoveToken();
    }

    public void MoveToken()
    {
        List<Tile> tiles = gameManager.movementArea;
        Tile target = tiles[Random.Range(0, tiles.Count)];
        gameManager.MovePlayer(target.gameObject);
    }
}

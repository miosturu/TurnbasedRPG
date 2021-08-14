using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI
{
    public int teamNumber = 1;

    [Space(10)]

    public Gameboard gameboard;
    public GameManager gameManager;
    public IGamePiece currentGamePiece;

    public EnemyAI(Gameboard gameboard, GameManager gameManager)
    {
        this.gameboard = gameboard;
        this.gameManager = gameManager;
    }

    public void PlayTurn(IGamePiece currentGamePiece)
    {
        this.currentGamePiece = currentGamePiece;
        MoveToken();
        gameManager.EndTurn();
    }

    public void MoveToken()
    {
        List<Tile> tiles = gameManager.movementArea;
        Tile target = tiles[Random.Range(0, tiles.Count)];
        gameManager.MovePlayer(target.gameObject);
    }

    public IEnumerator AITurn(IGamePiece currentGamePiece)
    {
        this.currentGamePiece = currentGamePiece;
        yield return new WaitForSeconds(.75f);
        MoveToken();
        yield return new WaitForSeconds(1f);
        gameManager.EndTurn();
    }
}

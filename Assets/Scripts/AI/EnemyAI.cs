using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI
{
    public int teamNumber = 1;

    [Space(10)]

    private Gameboard gameboard;
    private GameManager gameManager;
    private IGamePiece currentGamePiece;


    [Header("Evaluation")]
    private float[,] evaluation;
    [SerializeField] private float evalWeight_tank = -8f;
    [SerializeField] private float evalWeight_offence = -5f;
    [SerializeField] private float evalWeight_support = 10f;


    public EnemyAI(Gameboard gameboard, GameManager gameManager)
    {
        this.gameboard = gameboard;
        this.gameManager = gameManager;
        evaluation = new float[6, 9];
    }


    private void MoveToken()
    {
        Dictionary<Tile, int> tiles = gameManager.movementArea;

        Tile[] foo = new Tile[tiles.Count];
        tiles.Keys.CopyTo(foo, 0);
        Tile target = foo[UnityEngine.Random.Range(0, foo.Length)];
        gameManager.MovePlayer(target.gameObject);
    }



    public IEnumerator AITurn(IGamePiece currentGamePiece)
    {
        this.currentGamePiece = currentGamePiece;

        EvaluatePlayers(GetPlayers());

        yield return new WaitForSeconds(.75f);
        MoveToken();
        yield return new WaitForSeconds(1f);
        gameManager.EndTurn();
    }



    private Dictionary<Tile, PlayerType> GetPlayers()
    {
        Dictionary<Tile, PlayerType> enemies = new Dictionary<Tile, PlayerType>();

        for (int i = 0; i < gameboard.mapW; i++)
        {
            for (int j = 0; j < gameboard.mapH; j++)
            {
                GameObject curretObject = gameboard.map[i, j].GetComponent<Tile>().currentObject;

                if (curretObject != null)
                    enemies.Add(gameboard.map[i,j].GetComponent<Tile>(), curretObject.GetComponent<IGamePiece>().GetPlayerType());
            }
        }

        return enemies;
    }


    private void EvaluatePlayers(Dictionary<Tile, PlayerType> players)
    {
        float[,,] evaluations = new float[gameboard.mapW, gameboard.mapH, players.Count];
        MovementArea movementAreaGenerator = new MovementArea();
        Dictionary<Tile, int> eval;
        Dictionary<Tile, float> result;

        foreach (Tile origin in players.Keys)
        {
            eval = movementAreaGenerator.GenerateMovementArea(origin, 9);
            result = new Dictionary<Tile, float>();
            result[origin] = FallOffFucntion( eval[origin] );
        }
    }


    private float FallOffFucntion(float a)
    {
        return  -0.4f * (float)System.Math.Tanh(0.6f * (a - 5.2f)) + 0.6f;
    }
}

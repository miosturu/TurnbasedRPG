using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Very basic chase AI. At the movement the AI can't think furter than its own turn. Maybe in the future one could implement Alpha-Beta-pruning.
/// The AI can't think of a way to get around a corner.
/// Also one needs to figure out how to take in the account different kinds of units, such as ranged and melee, because it would be silly if ranged player tries to do melee.
/// </summary>
public class EnemyAI
{
    public int teamNumber = 1;

    [Space(10)]

    private Gameboard gameboard;
    private GameManager gameManager;
    private IGamePiece currentGamePiece;


    [Header("Evaluation")]
    private float[,] evaluation;
    [SerializeField] private float evalWeight_tank = 1f;
    [SerializeField] private float evalWeight_offence = 2f;
    [SerializeField] private float evalWeight_support = 10f;


    public EnemyAI(Gameboard gameboard, GameManager gameManager)
    {
        this.gameboard = gameboard;
        this.gameManager = gameManager;
        evaluation = new float[6, 9];
    }


    /// <summary>
    /// Move token to random tile within the movement area.
    /// </summary>
    private void MoveToken()
    {
        Dictionary<Tile, int> tiles = gameManager.movementArea;

        Tile[] foo = new Tile[tiles.Count];
        tiles.Keys.CopyTo(foo, 0);
        Tile target = foo[UnityEngine.Random.Range(0, foo.Length)];
        gameManager.MovePlayer(target.gameObject);
    }


    /// <summary>
    /// Move token to spesific tile
    /// </summary>
    /// <param name="target">Target tile</param>
    private void MoveToken(Tile target)
    {
        gameManager.MovePlayer(target.gameObject);
    }


    /// <summary>
    /// Coroutine for AI to play the turn
    /// </summary>
    /// <param name="currentGamePiece">Game piece that will be moved</param>
    /// <returns>Nothing</returns>
    public IEnumerator AITurn(IGamePiece currentGamePiece)
    {
        this.currentGamePiece = currentGamePiece;

        EvaluatePlayers(GetPlayers());

        Tile target = GetHighestValuedTile();

        //Debug.Log(target.name);

        Tile destination = GetNearestTile(target);

        //Debug.Log("Target tile: " + destination.name);

        yield return new WaitForSeconds(.75f);
        MoveToken(destination);
        yield return new WaitForSeconds(1f);
        gameManager.EndTurn();
    }


    /// <summary>
    /// Get enemy players on the gameboard.
    /// </summary>
    /// <returns>Enemy players on gameboard</returns>
    private Dictionary<Tile, PlayerType> GetPlayers()
    {
        Dictionary<Tile, PlayerType> enemies = new Dictionary<Tile, PlayerType>();

        foreach (GameObject tile in gameboard.map)
        {
            if (tile.GetComponent<Tile>().currentObject != null && (tile.GetComponent<Tile>().currentObject.GetComponent<IGamePiece>().GetPlayerTeam() != currentGamePiece.GetPlayerTeam()))
            {
                try
                {
                    enemies.Add(tile.GetComponent<Tile>(), tile.GetComponentInChildren<IGamePiece>().GetPlayerType());
                }
                catch
                {
                    Debug.Log("Error: " + tile.name + " has no player");
                }
            }
        }

        return enemies;
    }


    /// <summary>
    /// Evaluate whole gameboard with the given players. Each class type has its own weight.
    /// </summary>
    /// <param name="players">Players that will be evaluated</param>
    private void EvaluatePlayers(Dictionary<Tile, PlayerType> players)
    {
        Dictionary<Tile, int[]> playerPositions = new Dictionary<Tile, int[]>();

        foreach(Tile t in players.Keys) // Get positions of the players
        {
            playerPositions.Add(t, new int[] { t.xCoord, t.zCoord });
        }


        // for each tile we calcualte the sum of weight
        for (int x = 0; x < gameboard.mapW; x++)
        {
            for (int z = 0; z < gameboard.mapH; z++)
            {
                float sum = 0.0f;

                foreach (Tile t in players.Keys)
                {
                    float classTypeWeight = 1.0f;

                    switch (players[t])
                    {
                        case PlayerType.Offence:
                            classTypeWeight = evalWeight_offence;
                            break;
                        case PlayerType.Support:
                            classTypeWeight = evalWeight_support;
                            break;
                        case PlayerType.Tank:
                            classTypeWeight = evalWeight_tank;
                            break;
                        default:
                            classTypeWeight = 1.0f;
                            Debug.Log("Error on evaluation: No class type");
                            break;
                    }

                    sum += classTypeWeight * FallOffFucntion( DiagonalDistance(t, gameboard.map[x, z].GetComponent<Tile>()) );
                }

                //Debug.Log(x + " " + z + ": " + sum);
                evaluation[x, z] = sum;
            }
        }
    }


    /// <summary>
    /// Get the tile which has the highest value.
    /// </summary>
    /// <returns>Tile with highest value</returns>
    private Tile GetHighestValuedTile()
    {
        float biggest = evaluation[0, 0];
        GameObject returnTile = gameboard.map[0,0];

        for (int x = 0; x < gameboard.mapW; x++)
        {
            for (int z = 0; z < gameboard.mapH; z++)
            {
                if (evaluation[x, z] > biggest)
                {
                    biggest = evaluation[x, z];
                    returnTile = gameboard.map[x, z];
                }
            }
        }

        return returnTile.GetComponent<Tile>();
    }


    /// <summary>
    /// Get the nearest tile of the target tile within the movement area.
    /// </summary>
    /// <param name="target">Target tile</param>
    /// <returns>Nearest tile of the target</returns>
    private Tile GetNearestTile(Tile target)
    {
        Tile currentTile = currentGamePiece.GetGameObject().GetComponentInParent<Tile>();

        Dictionary<Tile, int> movementArea = new MovementArea().GenerateMovementArea(currentTile, 
                                                                                     currentGamePiece.GetCurrentMovementLeft());

        float smallestDistance = DiagonalDistance(currentTile, target);
        Tile potentialTile = currentTile;

        foreach (Tile t in movementArea.Keys)
        {
            float distance = DiagonalDistance(t, target);

            //Debug.Log( t.name + " Distance: " + distance );

            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                potentialTile = t;
            }
        }

        return potentialTile;
    }


    /// <summary>
    /// Distance between two tiles
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="destination">Destination tile</param>
    /// <returns>Distance between the tiles</returns>
    private float DiagonalDistance(Tile origin, Tile destination)
    {
        float dx = Mathf.Abs(destination.xCoord - origin.xCoord);
        float dz = Mathf.Abs(destination.zCoord - origin.zCoord);
        //Debug.Log("Diagonal distance is " + dx + " " + dz);

        return Mathf.Max(dx, dz);
    }


    /// <summary>
    /// Fall off fucntion for calculating the spread of weight from players.
    /// This function could be any kind of functions as long as it returns float.
    /// At the movement the fdunction is sigmoid function but more tests need to be made the get the best fit.
    /// </summary>
    /// <param name="a">Input</param>
    /// <returns>Output of the fucntion</returns>
    private float FallOffFucntion(float a)
    {
        return  -0.4f * (float)System.Math.Tanh(0.6f * (a - 5.2f)) + 0.6f;
    }
}
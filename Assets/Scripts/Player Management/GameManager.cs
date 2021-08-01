using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Debug variables")]
    public int maxDistance = 1;

    public int originX, originZ, destinationX, destinationZ = 0;

    [Header("Actual variables")]
    public GameObject playerToken;
    public Material[] teamColors;
    public TurnManager turnManager;

    [SerializeField]
    private Gameboard gameboard;
    public IGamePiece currentPlayer;

    [Header("Player variables")]
    public List<Tile> movementArea = new List<Tile>(); // Stores all the tiles where current player can go

    [Header("AI")]
    [SerializeField]
    private EnemyAI ai; // Needs to be gameobject?

    private void Start()
    {
        turnManager = new TurnManager();
        ai = new EnemyAI(gameboard, this);

        AddPlayer("A", 0, 0, 0);
        //AddPlayer("B", 0, 2, 0);

        AddPlayer("C", 1, 5, 8);
        //AddPlayer("D", 1, 3, 8);

        InitializeFirstTurn();
    }


    public void InitializeFirstTurn()
    {
        currentPlayer = turnManager.firstPlayer.player;
        currentPlayer.HighlightSetActive(true);
        GenerateMovementArea();
        ShowMovementArea();
    }


    public void AddPlayer(string name, int materialIndex, int x, int z)
    {
        GameObject p = Instantiate(playerToken);
        p.name = name;
        p.GetComponentInChildren<Renderer>().material = teamColors[materialIndex];
        PlacePlayer(p, x, z);

        turnManager.AddPlayerToList(p.GetComponent<PlayerGamePiece>(), materialIndex);
    }


    public void PlacePlayer(GameObject player, int x, int z)
    {
        Tile wantedTile = gameboard.map[x, z].GetComponent<Tile>();
        wantedTile.currentObject = player;

        player.transform.SetParent(wantedTile.gameObject.transform, false);
    }


    public void EndTurn()
    {
        ResetMovementArea(); // Disable previous player's movent area

        currentPlayer.ResetMovement();
        currentPlayer.HighlightSetActive(false); // Highlight current player and dehighlight previous player
        currentPlayer = turnManager.NextTurn();
        currentPlayer.HighlightSetActive(true);

        GenerateMovementArea();

        if (turnManager.currentPlayer.teamNumber == 0)
        {
            ShowMovementArea();
        }
        else // It's AI's turn
        {
            ai.PlayTurn(currentPlayer);
            EndTurn();
        }

    }


    public void RemovePlayer()
    {
        turnManager.RemovePlayer(currentPlayer);
    }


    public void MovePlayer(GameObject tile)
    {
        //Debug.Log(tile.name + " " + movementArea.Contains(tile.GetComponent<Tile>()));

        if (!movementArea.Contains(tile.GetComponent<Tile>()) || tile.GetComponent<Tile>().currentObject != null)  // If wanted tile has no other object on it and is in range
            return;

        ResetMovementArea();

        List<Tile> path = new BreadthFirstSearch().GeneratePath(currentPlayer.GetGameObject().GetComponentInParent<Tile>(), 
                                                                                                 tile.GetComponent<Tile>()); // TODO: actually show the player the path when hovering ovet the tile

        int distance = -1;

        foreach (Tile t in path) distance++;
        currentPlayer.ReduceMovement(distance);

        GameObject playerObject = currentPlayer.GetGameObject(); // Player game object
        playerObject.GetComponentInParent<Tile>().currentObject = null; // Remove previous parent
        playerObject.transform.SetParent(tile.gameObject.transform, false); // Move player
        tile.GetComponent<Tile>().currentObject = playerObject; // Set new tile's current object to be player

        GenerateMovementArea();
        ShowMovementArea();
    }


    public void GenerateMovementArea()
    {
        movementArea = new MovementArea().GenerateMovementArea(currentPlayer.GetGameObject().GetComponentInParent<Tile>(), 
                                                               currentPlayer.GetCurrentMovementLeft());
    }


    public void ShowMovementArea()
    {
        foreach (Tile t in movementArea)
        {
            t.highlight.SetActive(true);
        }
    }


    public void ResetMovementArea()
    {
        foreach (Tile t in movementArea)
        {
            t.highlight.SetActive(false);
        }
    }
}

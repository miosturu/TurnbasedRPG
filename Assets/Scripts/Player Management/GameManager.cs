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

    private Gameboard gameboard;
    public IGamePiece currentPlayer;

    private void Start()
    {
        gameboard = GameObject.Find("MapGenerator").GetComponent<Gameboard>();
        turnManager = new TurnManager();

        AddPlayer("A", 0, 0, 0);
        AddPlayer("B", 0, 2, 0);

        AddPlayer("C", 1, 5, 8);
        AddPlayer("D", 1, 3, 8);

        currentPlayer = turnManager.firstPlayer.player;
        currentPlayer.HighlightSetActive(true);
    }


    public void AddPlayer(string name, int materialIndex, int x, int z)
    {
        GameObject p = Instantiate(playerToken);
        p.name = name;
        p.GetComponentInChildren<Renderer>().material = teamColors[materialIndex];
        PlacePlayer(p, x, z);

        turnManager.AddPlayerToList(p.GetComponent<PlayerGamePiece>());
    }


    public void PlacePlayer(GameObject player, int x, int z)
    {
        Tile wantedTile = gameboard.map[x, z].GetComponent<Tile>();
        wantedTile.currentObject = player;

        player.transform.SetParent(wantedTile.gameObject.transform, false);
    }


    public void EndTurn()
    {
        currentPlayer.HighlightSetActive(false);
        currentPlayer = turnManager.NextTurn();
        currentPlayer.HighlightSetActive(true);
    }


    public void RemovePlayer()
    {
        turnManager.RemovePlayer(currentPlayer);
    }


    public void MovePlayer(GameObject tile)
    {
        if (tile.GetComponent<Tile>().currentObject != null)  // If wanted tile has no other object on it
            return;

        GameObject playerObject = currentPlayer.GetGameObject(); // Player game object
        playerObject.GetComponentInParent<Tile>().currentObject = null; // Remove previous parent
        playerObject.transform.SetParent(tile.gameObject.transform, false); // Move player
        tile.GetComponent<Tile>().currentObject = playerObject; // Set new tile's current object to be player
    }
}

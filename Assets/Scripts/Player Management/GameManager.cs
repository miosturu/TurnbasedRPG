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
    [SerializeField]
    private PartyManager partyManager;

    [Header("AI")]
    [SerializeField]
    private EnemyAI ai; // Needs to be gameobject?

    private void Start()
    {
        turnManager = new TurnManager();
        ai = new EnemyAI(gameboard, this);


        //TODO player party management

        SetPlayerTokens();

        //AddPlayer("A", 0, 0, 0);
        //AddPlayer("B", 0, 2, 0);

        //AddPlayer("C", 1, 5, 8);
        //AddPlayer("D", 1, 3, 8);

        InitializeFirstTurn();
    }


    public void SetPlayerTokens()
    {
        HeroScriptableObject[,] party = partyManager.GetMarchingOrder();

        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (party[x, z] != null)
                    AddPlayer(party[x, z], 0, x, z);
            }
        }

    }


    /// <summary>
    /// Initialize first turn:
    /// 1) Set first player
    /// 2) Generate player's allowed movement area
    /// 3) Show the generated area
    /// </summary>
    public void InitializeFirstTurn()
    {
        currentPlayer = turnManager.firstPlayer.player;
        currentPlayer.HighlightSetActive(true);
        GenerateMovementArea();
        ShowMovementArea();
    }


    /// <summary>
    /// Add player to combat order
    /// </summary>
    /// <param name="name">Player's name</param>
    /// <param name="materialIndex">What material will be used for token and player's team</param>
    /// <param name="x">X-position</param>
    /// <param name="z">Z-position</param>
    public void AddPlayer(string name, int materialIndex, int x, int z)
    {
        GameObject p = Instantiate(playerToken);
        p.name = name;
        p.GetComponentInChildren<Renderer>().material = teamColors[materialIndex];
        PlacePlayer(p, x, z);

        turnManager.AddPlayerToList(p.GetComponent<PlayerGamePiece>(), materialIndex);
    }


    /// <summary>
    /// Add new player from scriptable object.
    /// </summary>
    /// <param name="hero">Hero scriptable object</param>
    /// <param name="team">Hero's team</param>
    /// <param name="x">X-position</param>
    /// <param name="z">Z-Position</param>
    public void AddPlayer(HeroScriptableObject hero, int team, int x, int z)
    {
        GameObject player = Instantiate(playerToken);
        PlayerGamePiece gamePiece = player.GetComponent<PlayerGamePiece>();

        player.GetComponentInChildren<Renderer>().material = teamColors[team];

        player.name = hero.name;
        gamePiece.sprite.sprite = hero.sprite;

        gamePiece.name = hero.name;
        gamePiece.maxHp = hero.maxHp;
        gamePiece.movementSpeed = hero.movementSpeed;

        PlacePlayer(player, x, z);
        turnManager.AddPlayerToList(player.GetComponent<PlayerGamePiece>(), team);
    }


    /// <summary>
    /// Place player on gameboard
    /// </summary>
    /// <param name="player">Player that will be placed</param>
    /// <param name="x">X-position</param>
    /// <param name="z">Z-position</param>
    public void PlacePlayer(GameObject player, int x, int z)
    {
        Tile wantedTile = gameboard.map[x, z].GetComponent<Tile>();
        wantedTile.currentObject = player;

        player.transform.SetParent(wantedTile.gameObject.transform, false);
    }


    /// <summary>
    /// End player's turn. Reset movement area and set next player's turn. Also play AI's turn if there's one.
    /// </summary>
    public void EndTurn()
    {
        ResetMovementArea(); // Disable previous player's movent area
        currentPlayer.ResetMovement();

        currentPlayer.HighlightSetActive(false); // Highlight current player and dehighlight previous player
        currentPlayer = turnManager.NextTurn();
        

        GenerateMovementArea();

        if (turnManager.currentPlayer.teamNumber == 0)
        {
            currentPlayer.HighlightSetActive(true);
            ShowMovementArea();
        }
        else // It's AI's turn
        {
            ai.PlayTurn(currentPlayer);
            EndTurn();
        }

    }


    /// <summary>
    /// Remove player from combat.
    /// </summary>
    public void RemovePlayer()
    {
        turnManager.RemovePlayer(currentPlayer);
    }


    /// <summary>
    /// Move player to new tile
    /// </summary>
    /// <param name="tile">New tile</param>
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


    /// <summary>
    /// Generate player's movement area.
    /// </summary>
    public void GenerateMovementArea()
    {
        movementArea = new MovementArea().GenerateMovementArea(currentPlayer.GetGameObject().GetComponentInParent<Tile>(), 
                                                               currentPlayer.GetCurrentMovementLeft());
    }


    /// <summary>
    /// Show player's movement area.
    /// </summary>
    public void ShowMovementArea()
    {
        foreach (Tile t in movementArea)
        {
            t.highlight.SetActive(true);
        }
    }


    /// <summary>
    /// Don't show player's movement area
    /// </summary>
    public void ResetMovementArea()
    {
        foreach (Tile t in movementArea)
        {
            t.highlight.SetActive(false);
        }
    }
}

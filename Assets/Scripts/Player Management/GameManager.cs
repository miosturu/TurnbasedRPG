using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for managing the whole game.
/// This takes care of player actions, movement, adding and removing players, telling the UI to update, set players to the gameboard, and end turns.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Actual variables")]
    [SerializeField] private int initiativeDie = 20;
    public GameObject playerToken;
    [SerializeField] private Material[] teamColors;
    private TurnManager turnManager;
    [SerializeField] private Gameboard gameboard;
    public IGamePiece currentPlayer;

    public event EventHandler<OnEndTurnEventArgs> OnEndTurn; // Event for updating UI

    [Header("Player variables")]
    public ActionScriptableObject[] heroActions;
    public ActionScriptableObject selectedAction;
    public List<Tile> movementArea = new List<Tile>(); // Stores all the tiles where current player can go
    public int playerActionsLeftOnTurn;
    [SerializeField] private PartyManager partyManager;

    [Header("AI")]
    [SerializeField] private EnemyAI ai;
    public EnemyPartyScriptableObject enemyParty;


    private void Start()
    {
        turnManager = new TurnManager();
        ai = new EnemyAI(gameboard, this);
        heroActions = new ActionScriptableObject[4];

        SetPlayerTokens();
        SetEnemyTokens();

        InitializeFirstTurn();
    }


    /// <summary>
    /// Set human's tokens on gameboard
    /// </summary>
    public void SetPlayerTokens()
    {
        HeroScriptableObject[,] party = partyManager.GetMarchingOrder();

        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                if (party[x, z] != null)
                    AddPlayer(party[x, z], 0, x, z);
            }
        }
    }


    /// <summary>
    /// Set enemy players on gameboard
    /// </summary>
    public void SetEnemyTokens()
    {
        try
        {
            HeroScriptableObject[,] party = enemyParty.GetMarchingOrder();

            for (int x = 0; x < 3; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (party[x, z] != null)
                        AddPlayer(party[x, z], 1, x + 3, z + 6);
                }
            }
        }
        catch
        {
            Debug.Log("No selected enemy party");
        }
    }


    /// <summary>
    /// Initialize first turn:
    /// 1) Set first player
    /// 2) Get player's actions
    /// 3) Generate player's allowed movement area
    /// 4) Get how many actions player is allowed to take
    /// 5) Show the generated movement area if human
    /// </summary>
    public void InitializeFirstTurn()
    {
        currentPlayer = turnManager.firstPlayer.player;
        heroActions = currentPlayer.GetActions();
        GenerateMovementArea();
        playerActionsLeftOnTurn = currentPlayer.GetMaxActionsPerTurn();
        currentPlayer.HighlightSetActive(true);

        bool isPlayerTurn = false;
        if (turnManager.currentPlayer.teamNumber == 0)
        {
            isPlayerTurn = true;
            ShowMovementArea();
        }
        else
        {
            StartCoroutine(ai.AITurn(currentPlayer));
        }

        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(isPlayerTurn, isPlayerTurn)); // Fire event to update UI
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
        gamePiece.currentHp = hero.maxHp;
        gamePiece.movementSpeed = hero.movementSpeed;
        gamePiece.movementLeft = hero.movementSpeed;

        gamePiece.actions = hero.heroActions;
        gamePiece.maxActionsPerTurn = hero.maxActionsPerTurn;
        gamePiece.actionsLeft = hero.maxActionsPerTurn;

        PlacePlayer(player, x, z);
        turnManager.AddPlayerToList(player.GetComponent<PlayerGamePiece>(), team, new DiceRoller().RollDice(1, initiativeDie) + hero.initiativeBonus);
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
        selectedAction = null;
        ResetMovementArea(); // Disable previous player's movent area
        currentPlayer.ResetMovement();


        currentPlayer.HighlightSetActive(false); // Highlight current player and dehighlight previous player
        currentPlayer = turnManager.NextTurn();
        currentPlayer.HighlightSetActive(true);

        playerActionsLeftOnTurn = currentPlayer.GetMaxActionsPerTurn();

        GenerateMovementArea();

        if (turnManager.currentPlayer.teamNumber == 0) // Set up player's turn
        {
            ShowMovementArea();
            heroActions = currentPlayer.GetActions();
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true));
        }
        else // It's AI's turn
        {
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(false));
            StartCoroutine(ai.AITurn(currentPlayer));
        }
    }


    /// <summary>
    /// Remove current player from combat.
    /// </summary>
    public void RemovePlayer()
    {
        turnManager.RemovePlayer(currentPlayer);
    }


    /// <summary>
    /// Remove one spesific player from combat and set tile's current object to be null.
    /// </summary>
    /// <param name="player">Player that will be deleted</param>
    public void RemovePlayer(IGamePiece player)
    {
        //Get player's position and set CurrentObject = null then remove from the game
        player.GetGameObject().GetComponentInParent<Tile>().currentObject = null;
        turnManager.RemovePlayer(player);
    }


    /// <summary>
    /// Move player to new tile
    /// </summary>
    /// <param name="tile">New tile</param>
    public void MovePlayer(GameObject tile) // TODO: When we have selected action AND valid tile is clikced -> do the action NOT move       DONE
    {                                       // Also, when the action is selected, don't show allowed movement area BUT the allowed targets
                                            // In UI some how highlight the selected action, maybe hight contrass gameobject                DONE

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

        if (turnManager.currentPlayer.teamNumber == 0) // If the player is real player Fire event to update UI
        {
            bool canMakeActions = false;
            if (playerActionsLeftOnTurn > 0)
                canMakeActions = true;

            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, !canMakeActions));
            ShowMovementArea();
        }
    }


    /// <summary>
    /// Do selected action.
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="target">Target tile</param>
    public void DoSelectedAction(Tile origin, Tile target)
    {
        if (playerActionsLeftOnTurn > 0 && selectedAction.Action(origin, target))
        {
            Debug.Log("Action was done");
            playerActionsLeftOnTurn -= selectedAction.actionCost;
            selectedAction = null;

            if (playerActionsLeftOnTurn <= 0)
                OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, true));
        }
        else
        {
            Debug.Log("Action failed");
            if (playerActionsLeftOnTurn <= 0)
                Debug.Log("Not enough actions left");
        }

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
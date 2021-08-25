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
    [Header("Visual")]
    [SerializeField] private Color movementAreaColor;
    [SerializeField] private int movementAreaColorFallOff;

    [SerializeField] private Color validTargetColor;

    [SerializeField] private Color[] teamColors;

    [SerializeField] private float playerVisualMovementDelay = 0.25f;

    [Header("Actual variables")]
    [SerializeField] private int initiativeDie = 20;
    public IGamePiece currentPlayer;
    [SerializeField] private GameObject playerToken; // Player prefab
    private TurnManager turnManager;
    [SerializeField] private Gameboard gameboard;
    public event EventHandler<OnEndTurnEventArgs> OnEndTurn; // Event for updating UI

    [Header("Player variables")]
    public ActionScriptableObject[] heroActions;
    public ActionScriptableObject selectedAction;
    public Dictionary<Tile, int> movementArea = new Dictionary<Tile, int>(); // Stores all the tiles where current player can go
    public List<Tile> validTargets = new List<Tile>(); // Stores all the tiles that player can target with its action
    public int playerActionsLeftOnTurn;
    [SerializeField] private PartyManager partyManager;

    [Header("AI")]
    [SerializeField] private EnemyAI ai;
    public EnemyPartyScriptableObject enemyParty;


    private void Start()
    {
        StartCoroutine(SetUpCombat());
    }


    /// <summary>
    /// Start combat. Create the needed arrays and set the players on gameboard
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetUpCombat()
    {
        turnManager = new TurnManager();
        ai = new EnemyAI(gameboard, this);
        heroActions = new ActionScriptableObject[4];

        SetPlayerTokens();
        SetEnemyTokens();

        yield return new WaitForSeconds(1.5f);
        InitializeFirstTurn();
        // Call function from UIManager to get the actions

        bool isPlayerTurnFirst = false;
        if (turnManager.firstPlayer.teamNumber == 0)
        {
            isPlayerTurnFirst = true;
        }

        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(isPlayerTurnFirst, !isPlayerTurnFirst));
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

        player.GetComponentInChildren<Renderer>().material.color = teamColors[team];

        player.name = hero.name;
        gamePiece.sprite.sprite = hero.sprite;
        gamePiece.team = team;

        gamePiece.name = hero.name;
        gamePiece.maxHp = hero.maxHp;
        gamePiece.currentHp = hero.maxHp;
        gamePiece.movementSpeed = hero.movementSpeed;
        gamePiece.movementLeft = hero.movementSpeed;

        gamePiece.actions = hero.heroActions;
        gamePiece.maxActionsPerTurn = hero.maxActionsPerTurn;
        gamePiece.actionsLeft = hero.maxActionsPerTurn;
        gamePiece.playerType = hero.playerType;

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
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, false));
        }
        else // It's AI's turn
        {
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(false, true));
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
    public void MovePlayer(GameObject tile)
    {
        //Debug.Log(tile.name + " " + movementArea.Contains(tile.GetComponent<Tile>()));

        if (!movementArea.ContainsKey(tile.GetComponent<Tile>()) || tile.GetComponent<Tile>().currentObject != null)  // If wanted tile has no other object on it and is in range
            return;

        ResetMovementArea();

        List<Tile> path = new BreadthFirstSearch().GeneratePath(currentPlayer.GetGameObject().GetComponentInParent<Tile>(), 
                                                                                                 tile.GetComponent<Tile>()); // TODO: actually show the player the path when hovering ovet the tile

        GameObject playerObject = currentPlayer.GetGameObject(); // Player game object
        playerObject.GetComponentInParent<Tile>().currentObject = null; // Remove previous parent
        tile.GetComponent<Tile>().currentObject = playerObject; // Set new tile's current object to be player

        // TODO movement animation goes here. Implemented with coroutine
        StartCoroutine(AnimatePlayerMovement(playerObject, path, tile.GetComponent<Tile>()));

        int distance = -1;
        foreach (Tile t in path)
            distance++;
        currentPlayer.ReduceMovement(distance);

        GenerateMovementArea();

        if (turnManager.currentPlayer.teamNumber == 0) // If the player is real player Fire event to update UI
        {
            bool canMakeActions = false;
            if (playerActionsLeftOnTurn > 0)
                canMakeActions = true;

            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, !canMakeActions));
        }
    }


    /// <summary>
    /// Move player on gameboard one tile at the time.
    /// </summary>
    /// <param name="player">Moved player</param>
    /// <param name="path">Path of the player</param>
    /// <param name="target">Target tile</param>
    /// <returns>Nothing I guess</returns>
    private IEnumerator AnimatePlayerMovement(GameObject player, List<Tile> path, Tile target)
    {
        foreach (Tile t in path)
        {
            player.transform.position = t.transform.position;
            yield return new WaitForSeconds(playerVisualMovementDelay);
        }
        player.transform.SetParent(target.gameObject.transform, true);
        GenerateMovementArea();
        ShowMovementArea();

        yield return null;
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
        }
        else
        {
            Debug.Log("Action failed"); // TODO: Maybe deselect selectedAction and show movementArea?
            if (playerActionsLeftOnTurn <= 0)
                Debug.Log("Not enough actions left");
        }

        ResetValidTargets();
        if (playerActionsLeftOnTurn <= 0)
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, true));

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
        foreach (Tile t in movementArea.Keys)
        {
            t.highlight.GetComponent<MeshRenderer>().material.color = new Color(movementAreaColor.r / movementAreaColorFallOff * movementArea[t],
                                                                                movementAreaColor.g / movementAreaColorFallOff * movementArea[t],
                                                                                movementAreaColor.b / movementAreaColorFallOff * movementArea[t]);
            t.highlight.SetActive(true);
        }
    }


    /// <summary>
    /// Don't show player's movement area
    /// </summary>
    public void ResetMovementArea()
    {
        foreach (Tile t in movementArea.Keys)
        {
            t.highlight.SetActive(false);
        }
    }


    /// <summary>
    /// Check if the players on tiles a and b are on the same team.
    /// </summary>
    /// <param name="a">Tile a</param>
    /// <param name="b">Tile b</param>
    /// <returns>Players on tiles a and b are on the same team</returns>
    public bool PlayersAreOnSameTeam(Tile a, Tile b)
    {
        try
        {
            int aTeam = turnManager.GetPlayerTeamNumber(a.GetComponentInChildren<IGamePiece>());
            int bTeam = turnManager.GetPlayerTeamNumber(b.GetComponentInChildren<IGamePiece>());

            Debug.Log("A's team: " + aTeam + " B's team: " + bTeam);

            return a == b;
        }
        catch
        {
            Debug.Log("Failed to check teams for tiles " + a.name + " and " + b.name);
            return false;
        }
    }


    /// <summary>
    /// Get valid targets for the selected action.
    /// </summary>
    /// <returns>All the valid targets for selected action.</returns>
    public void GetValidTargets()
    {
        validTargets = new List<Tile>();
        Tile playerTile = currentPlayer.GetGameObject().GetComponentInParent<Tile>();

        foreach(GameObject tileObject in gameboard.map)
        {
            Tile tile = tileObject.GetComponent<Tile>();
            if (selectedAction.TargetIsValid(playerTile, tile))
            {
                Debug.Log("Valid tile: " + tile.name);
                tile.highlight.GetComponent<MeshRenderer>().material.color = validTargetColor;
                validTargets.Add(tile);
                tile.highlight.SetActive(true);
            }
        }
    }


    /// <summary>
    /// Show valid targets for selected action.
    /// </summary>
    public void ResetValidTargets()
    {
        foreach(Tile t in validTargets)
        {
            t.highlight.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
            t.highlight.SetActive(false);
        }
    }
}
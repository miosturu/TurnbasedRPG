using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for managing the whole game.
/// This takes care of player actions, movement, adding and removing players, telling the UI to update, set players to the gameboard, and end turns.
/// Human player uses number 0 to indicate it, AI player uses 1.
/// 
/// TODO: Add way to reset the gameboard
/// TODO: Add terminal state detection i.e. the winner detection DONE
/// </summary>
public class GameManager : MonoBehaviour
{
    // Controlls visuals of the game.
    [Header("Visual")]
    [SerializeField] private Color movementAreaColor;
    [SerializeField] private int movementAreaColorFallOff; // Further away the tiles are from the origin, lighter the color is. Used for better visualization.
    [SerializeField] private Color validTargetColor; // Color of the valid target when action is selected.
    [SerializeField] private Color[] teamColors;
    [SerializeField] private float playerVisualMovementDelay = 0.25f;
    [SerializeField] private float combatStartDelay = 1.5f;

    [Header("Actual variables")]
    [SerializeField] private int initiativeDie = 20; // When the combat starts the player roll d20 to see who goes first. Lifted directly from D&D 5E.
    private TurnManager turnManager; // Keeps trak of who is next and who are in the game.

    public IGamePiece currentPlayer;
    [SerializeField] private GameObject playerToken; // Player prefab
    [SerializeField] private Gameboard gameboard;
    public event EventHandler<OnEndTurnEventArgs> OnEndTurn; // Event for updating UI

    [Header("Player variables")]
    public ActionScriptableObject[] heroActions;
    public ActionScriptableObject selectedAction;
    public Dictionary<Tile, int> movementArea = new Dictionary<Tile, int>(); // Stores all the tiles where current player can go
    public List<Tile> validTargets = new List<Tile>(); // Stores all the tiles that player can target with its action
    public int playerActionsLeftOnTurn;
    [SerializeField] private PartyManager partyManager;
    [SerializeField] private int[] numberOfPieces = new int[] { 0, 0 }; // Store how many game pieces each team has. First element is for the human player, second one is for the AI
    public int winnerTeamNumber = -1; // Stores which team is the winner. -1 is no one is, 0 is the player, 1 is the AI.

    private Dictionary<PlayerGamePiece, int[]> playerTokenPositions = new Dictionary<PlayerGamePiece, int[]>();

    [Header("AI")]
    [SerializeField] private EnemyAI ai;
    public EnemyPartyScriptableObject enemyParty;
    [SerializeField] private bool trainingMode = false;
    [SerializeField] private HeroScriptableObject[] tokenPool; // Used for random selection of tokens
    [SerializeField] private float tokenProb; // How likely is it to get token

    private void Start()
    {
        gameboard.GenerateLevel(MapType.Random);
        StartCoroutine(SetUpCombat());
    }

    // TODO: This is for testing the reset function. Should be remove when the testing is done
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }


    /// <summary>
    /// Start combat. Create the needed arrays and set the players on gameboard
    /// </summary>
    /// <returns>Nothing</returns>
    public IEnumerator SetUpCombat()
    {
        ai = new EnemyAI(gameboard, this);
        heroActions = new ActionScriptableObject[4];

        if (!trainingMode)
        {
            turnManager = new TurnManager();
            SetPlayerTokens();
            SetEnemyTokens();
        }
        else
        {
            AddDummyTokens();
            SelectRandomTokens();
        }

        yield return new WaitForSeconds(combatStartDelay); // Wait before the actual combat starts
        InitializeFirstTurn();
        // Call function from UIManager to get the actions

        bool isPlayerTurnFirst = false;
        if (turnManager.firstPlayer.teamNumber == 0)
        {
            isPlayerTurnFirst = true;
        }

        //turnManager.PrintPlayers();
        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(isPlayerTurnFirst, !isPlayerTurnFirst));
    }


    /// <summary>
    /// Modify pre existing token to new token.
    /// </summary>
    /// <param name="player">Pre existing token</param>
    /// <param name="hero">New token information</param>
    private void ModifyToken(PlayerGamePiece player, HeroScriptableObject hero)
    {
        player.GetComponentInChildren<Renderer>().material.color = teamColors[player.GetPlayerTeam()];

        player.GetGameObject().name = hero.name;
        player.sprite.sprite = hero.sprite;

        player.team = player.GetPlayerTeam();

        player.name = hero.name;
        player.maxHp = hero.maxHp;
        player.currentHp = hero.maxHp;
        player.movementSpeed = hero.movementSpeed;
        player.movementLeft = hero.movementSpeed;

        player.actions = hero.heroActions;
        player.maxActionsPerTurn = hero.maxActionsPerTurn;
        player.actionsLeft = hero.maxActionsPerTurn;
        player.playerType = hero.playerType;

        PlacePlayer(player.gameObject, playerTokenPositions[player][0], playerTokenPositions[player][1]);
        turnManager.AddPlayerToList(player.GetComponent<PlayerGamePiece>(), player.GetPlayerTeam(), new DiceRoller().RollDice(1, initiativeDie) + hero.initiativeBonus);
    }


    /// <summary>
    /// Add dummy tokens that can be modified later.
    /// </summary>
    private void AddDummyTokens()
    {
        // Place dummy tokens for player 0
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                AddPlayer(0, x, z); // TODO: Add overload to create dummy tokens
            }
        }

        // Place dummy tokens for player 1
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                AddPlayer(1, x + 3, z + 6); // TODO: Add overload to create dummy tokens
            }
        }
    }


    // TODO: Make sure atleast one token spawns per team
    private void SelectRandomTokens()
    {
        turnManager = new TurnManager();

        bool[] teamsHaveOne = new bool[] { false, false };

        foreach (PlayerGamePiece gp in playerTokenPositions.Keys)
        {
            gp.gameObject.SetActive(false);
            // Remove player token from the tile
            int x = playerTokenPositions[gp][0];
            int z = playerTokenPositions[gp][1];
            gameboard.map[x, z].GetComponent<Tile>().currentObject = null;
        }

        foreach (PlayerGamePiece gp in playerTokenPositions.Keys)
        {
            if (new DiceRoller().RollDice(1, 20) < 15)
            {
                gp.gameObject.SetActive(true);
                ModifyToken(gp, tokenPool[(int)UnityEngine.Random.Range(0, 3)]);
                numberOfPieces[gp.GetPlayerTeam()]++;
                //gp.gameObject.GetComponent<PlayerHpUIManager>().ChangeStatusBarWidth(gp.GetMaxHp()); // TODO: not working as intended
            }
        }
    }


    /// <summary>
    /// Set human's tokens on gameboard from pre defined party.
    /// </summary>
    public void SetPlayerTokens()
    {
        HeroScriptableObject[,] party = partyManager.GetMarchingOrder(); // Both parties have certain order of deployment. We need to get it before we can spawn them.

        // Simple nested loop allows us to place them
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                if (party[x, z] != null)
                {
                    AddPlayer(party[x, z], 0, x, z);
                    numberOfPieces[0]++; // Increase player token count.
                }
            }
        }
    }


    /// <summary>
    /// Set enemy players on gameboard from pre defined party.
    /// </summary>
    public void SetEnemyTokens()
    {
        // If one accidentally forgets to add enemy party to the gameboard, then the game will tell why the enemies aren't spawning.
        try
        {
            HeroScriptableObject[,] party = enemyParty.GetMarchingOrder(); // Both parties have certain order of deployment. We need to get it before we can spawn them.

            // Simple nested loop allows us to place them
            for (int x = 0; x < 3; x++) 
            {
                for (int z = 0; z < 3; z++)
                {
                    if (party[x, z] != null)
                    {
                        AddPlayer(party[x, z], 1, x + 3, z + 6); // The off set is due to the wanted start position of the enemies.
                        numberOfPieces[1]++; // Increase player token count.
                    }
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
        currentPlayer = turnManager.firstPlayer.player; // Get the first player
        heroActions = currentPlayer.GetActions(); // Get the player's actions
        GenerateMovementArea(); // Get the players movement area. Based on movement speed and obsticles.
        playerActionsLeftOnTurn = currentPlayer.GetMaxActionsPerTurn(); // Get the player's actions per turn. It's usually only one, but added option to increase it just in case if one actions isn't enough.
        currentPlayer.HighlightSetActive(true); // Highlight the first player by switching on the highligh gameobject

        bool isPlayerTurn = false;
        if (turnManager.currentPlayer.teamNumber == 0)
        {
            isPlayerTurn = true;
            ShowMovementArea();
        }
        else // If the first player is AI, then start the coroutine to make the action
        {
            StartCoroutine(ai.AITurn(currentPlayer));
        }

        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(isPlayerTurn, isPlayerTurn)); // Fire event to update UI and the the turn
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
    /// Add new player to the game. Crates empty token that can be modified later.
    /// </summary>
    /// <param name="team">Token's team</param>
    /// <param name="x">X position</param>
    /// <param name="z">Z position</param>
    private void AddPlayer(int team, int x, int z)
    {
        // Debug.Log("New player. Team: " + team + ". X: " + x + ". Z: " + z);
        GameObject player = Instantiate(playerToken);
        player.GetComponent<PlayerGamePiece>().SetPlayerTeam(team);
        PlacePlayer(player, x, z);
        playerTokenPositions.Add(player.GetComponent<PlayerGamePiece>(), new int[] { x, z }); // We store the reference and starting position so we can reset the token
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

        if (turnManager.currentPlayer.teamNumber == 0) // Set up player's turn. TODO: Maybe ML agent could play here also
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
    /// Remove current player from rotation.
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
        numberOfPieces[player.GetPlayerTeam()]--; // The pieces was captured, decrease the number of pieces in that team.
        turnManager.RemovePlayer(player);
        CheckWinnerTeam(); // Check if one of the teams won.
    }


    /// <summary>
    /// Move player to new tile
    /// </summary>
    /// <param name="tile">New tile</param>
    public void MovePlayer(GameObject tile)
    {
        //Debug.Log(tile.name + " " + movementArea.Contains(tile.GetComponent<Tile>()));

        // If wanted tile has no other object on it and is in range
        if (!movementArea.ContainsKey(tile.GetComponent<Tile>()) || tile.GetComponent<Tile>().currentObject != null)
            return;

        ResetMovementArea();

        // Generate the path the player will take. TODO: actually show the player the path when hovering ovet the tile
        List<Tile> path = new BreadthFirstSearch().GeneratePath(currentPlayer.GetGameObject().GetComponentInParent<Tile>(), 
                                                                                                 tile.GetComponent<Tile>());

        // Set the tile to have a player
        GameObject playerObject = currentPlayer.GetGameObject(); // Player game object
        playerObject.GetComponentInParent<Tile>().currentObject = null; // Remove previous parent
        tile.GetComponent<Tile>().currentObject = playerObject; // Set new tile's current object to be player

        // Movement animation. Implemented with coroutine
        StartCoroutine(AnimatePlayerMovement(playerObject, path, tile.GetComponent<Tile>()));

        // Move player and recude allowed movement this turn
        int distance = -1;
        foreach (Tile t in path)
            distance++;
        currentPlayer.ReduceMovement(distance);

        GenerateMovementArea(); // Generate new movement area
    }


    /// <summary>
    /// Move player on gameboard one tile at the time.
    /// If this method is not used, then the player will just teleport to the destination.
    /// </summary>
    /// <param name="player">Moved player</param>
    /// <param name="path">Path of the player</param>
    /// <param name="target">Target tile</param>
    /// <returns>Nothing</returns>
    private IEnumerator AnimatePlayerMovement(GameObject player, List<Tile> path, Tile target)
    {
        int playerTeam = player.GetComponent<IGamePiece>().GetPlayerTeam();

        bool isHumanPlayer = false;
        if (playerTeam == 0)
            isHumanPlayer = true;

        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(isHumanPlayer, true));

        foreach (Tile t in path)
        {
            player.transform.position = t.transform.position;
            yield return new WaitForSeconds(playerVisualMovementDelay);
        }

        player.transform.SetParent(target.gameObject.transform, true);
        GenerateMovementArea();
        ShowMovementArea();

        if (playerTeam == 0) // If the player is real then show actions of allowed to take some
        {
            bool canMakeActions = false;
            if (playerActionsLeftOnTurn > 0)
                canMakeActions = true;

            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, !canMakeActions));
        }

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
            // Debug.Log("Action was done");
            playerActionsLeftOnTurn -= selectedAction.actionCost;
            selectedAction = null;
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, false));
        }
        else
        {
            Debug.Log("Action failed");
            if (playerActionsLeftOnTurn <= 0)
                Debug.Log("Not enough actions left");
            selectedAction = null;
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, false));
        }

        ResetValidTargets();
        if (playerActionsLeftOnTurn <= 0)
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, true));

        GenerateMovementArea();
        ShowMovementArea();
    }


    /// <summary>
    /// Generate player's movement area i.e. get the tiles that the player can enter with current movement left.
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
                //Debug.Log("Valid tile: " + tile.name);
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


    /// <summary>
    /// Check the winnig team.
    /// </summary>
    private void CheckWinnerTeam()
    {
        if (numberOfPieces[0] <= 0) // If player loses all the pieces, the AI wins
        {
            winnerTeamNumber = 1;
        }
        else if (numberOfPieces[1] <= 0) // If AI loses all the pieces, the player wins
        {
            winnerTeamNumber = 0;
        }
        Debug.Log("Winning team: " + winnerTeamNumber);
        // If no team has been wiped down, then we don't have to do anything
    }


    /// <summary>
    /// Reset the entire game as if the game was just started.
    /// </summary>
    public void ResetGame()
    {
        EndTurn(); // We make sure that the UI will be reset

        gameboard.ResetGameBoard(); // Reset the game board's tiles

        // After reset we have to regenerate movement area and show it
        ResetMovementArea();

        // Reset the number of tokens per team
        numberOfPieces[0] = 0;
        numberOfPieces[1] = 0;

        // Pick new teams and reset the positions
        ResetPlayerTokenPositions();
        SelectRandomTokens();

        // Initialize new game as if it's first
        InitializeFirstTurn();
    }


    /// <summary>
    /// Reset all 18 tokens to their original positions.
    /// </summary>
    private void ResetPlayerTokenPositions()
    {
        foreach(GameObject tile in gameboard.map)
        {
            tile.GetComponent<Tile>().currentObject = null;
        }

        foreach(PlayerGamePiece gp in playerTokenPositions.Keys)
        {
            int x = playerTokenPositions[gp][0];
            int z = playerTokenPositions[gp][1];

            PlacePlayer(gp.gameObject, x, z);
        }
    }
}
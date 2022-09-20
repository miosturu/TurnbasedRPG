using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for managing the whole game.
/// This takes care of player actions, movement, adding and removing players, telling the UI to update, set players to the gameboard, and end turns.
/// Human player uses number 0 to indicate it, AI player uses 1.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Controlls visuals of the game.
    #region eye candy
    [Header("Visual")]
    [SerializeField] private Color movementAreaColor;

    /// <summary>
    /// Further away the tiles are from the origin, lighter the color is. Used for better visualization.
    /// </summary>  
    [SerializeField] private int movementAreaColorFallOff; 
    [SerializeField] private Color validTargetColor; // Color of the valid target when action is selected.
    [SerializeField] private Color[] teamColors;
    [SerializeField] private float playerVisualMovementDelay = 0.25f;
    [SerializeField] private float combatStartDelay = 1.5f;
    #endregion

    [Header("Gameplay variables")]
    [SerializeField] private int initiativeDie = 20; // When the combat starts the player roll d20 to see who goes first. Lifted directly from D&D 5E.
    private TurnManager turnManager; // Keeps trak of who is next and who are in the game.

    public IGamePiece currentPlayer { get; private set; }
    [SerializeField] private GameObject playerToken; // Player prefab
    [SerializeField] private Gameboard gameboard;
    public event EventHandler<OnEndTurnEventArgs> OnEndTurn; // Event for updating UI

    [Header("Player/token variables")]
    public ActionScriptableObject[] heroActions;
    public ActionScriptableObject selectedAction;
    public Dictionary<Tile, int> movementArea = new Dictionary<Tile, int>(); // Stores all the tiles where current player can go
    public List<Tile> validTargets = new List<Tile>(); // Stores all the tiles that player can target with its action
    public int playerActionsLeftOnTurn;
    [SerializeField] private PartyManager partyManager;
    [SerializeField] private int[] numberOfPieces = new int[] { 0, 0 }; // Store how many game pieces each team has. First element is for the human player, second one is for the AI

    /// <summary>
    /// Stores which team is the winner. -1 is no one is, 0 is the player, 1 is the AI.
    /// </summary>
    public int winnerTeamNumber = -1; // 

    private Dictionary<PlayerGamePiece, int[]> playerTokenPositions = new Dictionary<PlayerGamePiece, int[]>();

    [Header("AI")]
    [SerializeField] private AIManager aIManager;
    [SerializeField] private EnemyAI ai;
    public EnemyPartyScriptableObject enemyParty;
    [SerializeField] private bool trainingMode = false;
    [SerializeField] private HeroScriptableObject[] tokenPool; // Used for random selection of tokens
    [SerializeField] [Range(0, 100)] private int tokenProb; // How likely is it to get token in percent
    [SerializeField] private bool[] aIOnTeamX = new bool[2] { true, true };
    [SerializeField] MapType trainingMapType;

    private void Start()
    {
        gameboard.GenerateLevel(MapType.Blank);
        StartCoroutine(SetUpCombat());
    }

    // TODO: This is for testing the reset function. Should be remove when the testing is done
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            aIManager.MakeAIPlayTurn(currentPlayer.GetPlayerTeam());
        }
    }


    /// <summary>
    /// Start combat. Create the needed arrays and set the players on gameboard
    /// </summary>
    /// <returns>Nothing</returns>
    public IEnumerator SetUpCombat()
    {
        //ai = new EnemyAI(gameboard, this);
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

        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, false));
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
    /// Both teams can have up to 9 tokens.
    /// </summary>
    private void AddDummyTokens()
    {
        // Place dummy tokens for player 0
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                AddPlayer(0, x, z);
            }
        }

        // Place dummy tokens for player 1
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                AddPlayer(1, x + 3, z + 6);
            }
        }
    }


    /// <summary>
    /// Select random tokens for both team.
    /// This method guarantees that both teams get atleast one token if token spawn propability is grater than zero.
    /// Token's spawn propability can be changed in the editor.
    /// </summary>
    private void SelectRandomTokens()
    {
        turnManager = new TurnManager();

        bool[] teamsHaveOne = new bool[] { false, false };

        // Remove all players from tiles so the tiles are made free
        foreach (PlayerGamePiece gp in playerTokenPositions.Keys)
        {
            gp.gameObject.SetActive(false);
            // Remove player token from the tile
            int x = playerTokenPositions[gp][0];
            int z = playerTokenPositions[gp][1];
            gameboard.map[x, z].GetComponent<Tile>().currentObject = null;
        }

        // Go through each token and have a %-chance to add it. Both teams will get atleast one token
        foreach (PlayerGamePiece gp in playerTokenPositions.Keys)
        {
            if (!teamsHaveOne[0] && gp.team != 1) // Team 0 doesn't have a player we add it 
            {
                gp.gameObject.SetActive(true);
                ModifyToken(gp, tokenPool[(int)UnityEngine.Random.Range(0, tokenPool.Length)]);
                numberOfPieces[gp.GetPlayerTeam()]++;
                teamsHaveOne[0] = true;
                // Debug.Log("Added player to team 0");
            }
            else if (!teamsHaveOne[1] && gp.team != 0) // Team 1 doesn't have a player we add it 
            {
                gp.gameObject.SetActive(true);
                ModifyToken(gp, tokenPool[(int)UnityEngine.Random.Range(0, tokenPool.Length)]);
                numberOfPieces[gp.GetPlayerTeam()]++;
                teamsHaveOne[1] = true;
                // Debug.Log("Added player to team 1");
            }
            else if (new DiceRoller().RollDice(1, 100) <= tokenProb) // If atleast both teams have atleast one then we can add a player to both teams
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
    /// This is a seperate function because one could forget to add enemy party at the editor.
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

        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, false));

        // Reset, get and show player's movement
        ResetMovementArea();
        GenerateMovementArea();
        ShowMovementArea();

        currentPlayer.HighlightSetActive(true); // Highlight the first player by switching on the highligh gameobject

        int currentPlayerTeam = currentPlayer.GetPlayerTeam();
        if (aIOnTeamX[currentPlayerTeam])
        {
            aIManager.MakeAIPlayTurn(currentPlayerTeam);
        }
    }


    /// <summary>
    /// End player's turn. Reset movement area and set next player's turn.
    /// </summary>
    public void EndTurn()
    {
        selectedAction = null;
        ResetMovementArea(); // Disable previous player's movent area
        currentPlayer.ResetMovement(); // Add movement for the next turn so the turn engin token can move.

        // Highlight new current player and dehighlight previous player
        currentPlayer.HighlightSetActive(false);
        currentPlayer = turnManager.NextTurn();
        currentPlayer.HighlightSetActive(true);

        playerActionsLeftOnTurn = currentPlayer.GetMaxActionsPerTurn();

        GenerateMovementArea();

        ShowMovementArea();
        heroActions = currentPlayer.GetActions();
        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, !true));

        int currentPlayerTeam = currentPlayer.GetPlayerTeam();
        if (aIOnTeamX[currentPlayerTeam])
        {
            aIManager.MakeAIPlayTurn(currentPlayerTeam);
        }
    }


    /// <summary>
    /// Reset the entire game as if the game was just started.
    /// </summary>
    public void ResetGame()
    {
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
        EndTurn(); // We make sure that the UI will be reset
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
        numberOfPieces[player.GetPlayerTeam()]--; // The pieces was eliminated, decrease the number of pieces in that team.
        turnManager.RemovePlayer(player);
        CheckWinnerTeam(); // Check if one of the teams won.
    }


    /// <summary>
    /// Move player to new tile.
    /// If the tile is not empty, then the token won't move.
    /// After the token is moved, this function checks if there are other tile that can be moved to.
    /// </summary>
    /// <param name="tile">New tile</param>
    /// <return>Returns if the token moved</return>
    public bool MovePlayer(GameObject tile)
    {
        //Debug.Log(tile.name + " " + movementArea.Contains(tile.GetComponent<Tile>()));

        // If wanted tile has no other object on it and is in range
        if (!movementArea.ContainsKey(tile.GetComponent<Tile>()) || tile.GetComponent<Tile>().currentObject != null)
        {
            return false;
        }

        ResetMovementArea();

        // Generate the path the player will take.
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

        return true;
    }


    /// <summary>
    /// Move player in relative manner.
    /// Same functionality as MovePlayer(GameObject), thus it checks if the tile is valid.
    /// The plan is to use this function for AI's decisions.
    /// </summary>
    /// <param name="x">Relative movement on X-axis</param>
    /// <param name="z">Relative movement on Z-axis</param>
    /// <returns>Did the movement work. Used for AI's rewards.</returns>
    public bool MovePlayer(int x, int z)
    {
        GameObject tile = null;
        Tile currentTile = currentPlayer.GetGameObject().GetComponentInParent<Tile>();
        int currentX = currentTile.xCoord;
        int currentZ = currentTile.zCoord;

        try
        {
            tile = gameboard.map[currentX + x, currentZ + z]; // The try-catch is in the case of the AI tries to go to invalid tile
        }
        catch
        {
            return false;
        }        

        return MovePlayer(tile.gameObject);
    }


    /// <summary>
    /// Move player on gameboard one tile at the time.
    /// If this method is not used, then the player will just teleport to the destination.
    /// At the moment so many things are tied to this so making the delay zero just teleports the token.
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

        bool canMakeActions = false;
        if (playerActionsLeftOnTurn > 0)
            canMakeActions = true;

        OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, !canMakeActions));

        yield return null;
    }


    /// <summary>
    /// Do selected action. This method checks if the player can do omore actions and the action target is valid.
    /// For example, attacking friendly tokens is not allowed, thus making this making this method do nothing.
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="target">Target tile</param>
    /// <return>Returns if the action succeeded</return>
    public bool DoSelectedAction(Tile origin, Tile target)
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
            {
                Debug.Log("Not enough actions left");
            }
            if (selectedAction.Action(origin, target))
            {
                String originString = "(" + origin.xCoord + ", " + origin.zCoord + ")";
                String targetString = "(" + target.xCoord + ", " + target.zCoord + ")";

                Debug.Log("Target is invalid relative to current position: " + originString + " vs. " + targetString);
            }

            selectedAction = null;
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, false));
            return false;
        }

        ResetValidTargets();
        if (playerActionsLeftOnTurn <= 0)
            OnEndTurn?.Invoke(this, new OnEndTurnEventArgs(true, true));

        GenerateMovementArea();
        ShowMovementArea();

        return true;
    }


    /// <summary>
    /// Do selected action targeting tile at (x, z).
    /// Same functionality as DoSelectedAction(Tile, Tile), thus it checks if the tile is valid.
    /// The plan is to use this function for AI's decisions.
    /// As of 2022/9/14 all the actions orginate from current player's position thus only target's coordinates are needed to do actions.
    /// </summary>
    /// <param name="x">Target tile relative to X-axis</param>
    /// <param name="z">Target tile relative to Z-axis</param>
    /// <returns>Was the action done</returns>
    public bool DoSelectedAction(int x, int z)
    {
        Tile tile = null;
        Tile currentTile = currentPlayer.GetGameObject().GetComponentInParent<Tile>();

        try
        {
            tile = gameboard.map[x, z].GetComponent<Tile>(); // The try-catch is in the case of the AI tries to go to invalid tile
        }
        catch
        {
            Debug.LogError("Failed to do action with DoSelectedAction(x, z)");
            return false;
        }


        return DoSelectedAction(currentTile, tile);
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
    /// This is done by going through the list of token's movement area and then coloring the tiles' higlights accrodingly.
    /// Also further the tile is, more it costs to move.
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
    /// Don't show player's movement area.
    /// Disables all the highlight tiles just in case.
    /// </summary>
    public void ResetMovementArea()
    {
        foreach (GameObject t in gameboard.map)
        {
            t.GetComponent<Tile>().highlight.SetActive(false);
        }
    }


    /// <summary>
    /// Check if the players on tiles a and b are on the same team.
    /// This is done by getting tiles' tokens and then checking their team numbers.
    /// When this was created it was used to debug the code, but now it's not used for anything.
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
            Debug.LogError("Failed to check teams for tiles " + a.name + " and " + b.name);
            return false;
        }
    }


    /// <summary>
    /// Get valid targets for the selected action.
    /// This goes through every single tile on the map and them checks through action's 'TargetIsValid(origin, target)'-function.
    /// The valid targets are stored in 'validTargets'-variable.
    /// This also shows all the valid targets when the function is used by enabling tiles' highlights.
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
    /// Hide all the targets for the action.
    /// This is done by going through 'validTargets'-list and disableing tiles' highlights.
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
    /// Check the winnig team i.e. which side has eliminated the other side.
    /// This is used to create a terminal state for the AI.
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


    /// <summary>
    /// Return current gameboard.
    /// This method is planned to be used by the AI, as it needs a way to see the gamestate.
    /// </summary>
    /// <returns>Returns current gmaboard</returns>
    public Gameboard GetGameboard()
    {
        return gameboard;
    }


    /// <summary>
    /// Returns reference to turn manager.
    /// This function is planned to be used by AI.
    /// </summary>
    /// <returns>Reference to turn manager.</returns>
    public TurnManager GetTurnManager()
    {
        return turnManager;
    }


    /// <summary>
    /// Return current token's position as Vector2.
    /// Mainly used by the AI.
    /// </summary>
    /// <returns>Current token's position as vector2</returns>
    public Vector2 GetCurrentTokenCoordinates()
    {
        Tile parentTile = currentPlayer.GetGameObject().GetComponentInParent<Tile>();
        return new Vector2(parentTile.xCoord, parentTile.zCoord);
    }


    /// <summary>
    /// 6 rows * 9 columns * 2 coordinates = 108 floats
    /// Fill in the float in order then fill it with -1s.
    /// Remove the tile where the current token is
    /// </summary>
    /// <returns></returns>
    public List<float> GetMovementAreaAsFloats()
    {
        List<float> movementTiles = new List<float>();
        Tile currentTile = currentPlayer.GetGameObject().GetComponentInParent<Tile>();

        foreach(Tile tile in movementArea.Keys)
        {
            movementTiles.Add(tile.xCoord);
            movementTiles.Add(tile.zCoord);
        }

        if (movementTiles.Count < 108)
        {
            for (int i = 0; movementTiles.Count < 108; i++)
            {
                //Debug.Log(i);
                movementTiles.Add(-1f);
            }
        }

        // Debug.Log("Size of the movement area vector: " + movementTiles.Count);

        return movementTiles;
    }


    /// <summary>
    /// Get current token's type as integer.
    /// Used by the AI to make disicions.
    /// </summary>
    /// <returns>Tokens type as int</returns>
    public int GetCurrentTokenType()
    {
        return (int)currentPlayer.GetTokenType();
    }


    /// <summary>
    /// Return every available target for every single action.
    /// This function is desined to be used by ML agent.
    /// The outout is following:
    ///     [action's index][target X cooridnates][-1 * n if there's less valid targets than there're players]
    /// Outout will always be 37 * 4 floats long, because:
    ///     One float represents the action's index.
    ///     18 tokens at max * 2 coordinates/token = 36 floats represent coodinates.
    ///     There are four actions that the token can make.
    /// The coordinates are filled with -1, because then we preserve the length of the input thus possibly making the training faster and more stable.
    /// </summary>
    /// <returns>List of all possible targets for each action</returns>
    public List<float> GetValidTargetForEachAction()
    {
        // Loop over every action and store the in list
        List<float> actionsAndTargets = new List<float>();

        float actionIndex = 0.0f;

        foreach(ActionScriptableObject action in heroActions)
        {
            List<float> oneActionAndTargets = new List<float>();
            oneActionAndTargets.Add(actionIndex);

            validTargets = new List<Tile>();
            Tile playerTile = currentPlayer.GetGameObject().GetComponentInParent<Tile>();

            foreach (GameObject tileObject in gameboard.map)
            {
                Tile tile = tileObject.GetComponent<Tile>();
                if (action.TargetIsValid(playerTile, tile))
                {
                    validTargets.Add(tile);
                }
            }

            foreach (Tile tile in validTargets)
            {
                float x = tile.xCoord;
                float z = tile.zCoord;
                oneActionAndTargets.Add(x);
                oneActionAndTargets.Add(z);
            }

            if (oneActionAndTargets.Count < 37)
            {
                // Fill the list with -1 till it is 37 numbers long
                for (int i = 0; oneActionAndTargets.Count < 37; i++)
                 oneActionAndTargets.Add(-1f);
            }

            actionsAndTargets.AddRange(oneActionAndTargets);
            actionIndex++;
        }

        return actionsAndTargets;
    }


    /// <summary>
    /// Get team's tokens' locations as a list of floats.
    /// This method is planned to be used by ML agent.
    /// The return list is always 18 floats long, because each team can have up to 9 tokens with 2 coordinates each.
    /// </summary>
    /// <param name="teamNumber">Which team is being investigated</param>
    /// <returns>List of coordinates</returns>
    public List<float> GetTokenLocations(int teamNumber)
    {
        List<float> tokenLocations = new List<float>();

        foreach(GameObject tileObject in gameboard.map)
        {
            PlayerGamePiece playerToken = tileObject.GetComponentInChildren<PlayerGamePiece>();
            Tile tile = tileObject.GetComponent<Tile>();

            if (playerToken != null && playerToken.team == teamNumber)
            {
                tokenLocations.Add(tile.xCoord);
                tokenLocations.Add(tile.zCoord);
            }
        }

        // Fill the list to be 2 coords * max 9 tokens = 18 floats long
        if (tokenLocations.Count <= 18)
        {
            for (int i = 0; tokenLocations.Count < 18; i++)
            {
                tokenLocations.Add(-1f);
            }
        }

        return tokenLocations;
    }


    /// <summary>
    /// Get team's total HP as int value.
    /// This goes through all the tokens from 'playerTokenPositions'-dictionary and then checks if the token is in current game and then adds their current hp to total value.
    /// This method is planned for rewarding AI.
    /// </summary>
    /// <param name="teamNumber"></param>
    /// <returns></returns>
    public int GetTeamHP(int teamNumber)
    {
        int totalHP = 0;

        foreach(PlayerGamePiece piece in playerTokenPositions.Keys)
        {
            if (piece.isActiveAndEnabled && piece.GetComponent<PlayerGamePiece>().team == teamNumber)
                totalHP += piece.GetComponent<PlayerGamePiece>().currentHp;
        }

        return totalHP;
    }
}
/// TODO: A way to choose which side(s) have an AI.
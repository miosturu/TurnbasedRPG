using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// AI agent that uses Unity's machine learning library. 
/// Thus we have to define, what the agent can do and how can we encourage it to do what is wanted.
/// One agent controlls on team, thus allowing some level of strategy. 
/// 
/// One challenge in this project is that actions have a possibility to do zero damage, thus varying the reward.
/// 
/// Does the AI need some kind of memory? Most likely not as past shouldn't affect current decisions.
/// 
/// Current plan is the following:
///     > Use RL as eliminates the need of human trainer.
///     > Use PPO as it is supported by the library.
///     > We train the agent against it's self.
///     > No need to nomaliza the observations as the values are small.
///     > As game is turn based, the agent's actions are sequntial.
/// 
/// What the agent needs to make a decision:
///     > Layout of the map.
///         > Data type: 2D array with tiles represented as numbers.
///             > Doesn't work, list of floats used instead
///         > Where the token can be moved to.
///         > Can token use action over certain obsticles.
///     > What is the current token
///         > Determines what the AI can do on turn
///     > How much movement is left
///         > Data type: bool.
///         > If there's nothing to do, then turn should end.
///     > Can the player make actions
///         > Data type: bool.
///         > If there's nothing to do, then turn should end.
///     > Where other tokens are.
///         > Data type: List of floats.
///         > Can potentially create strategy with using all the tokens.
///     > How healthy the tokens are.
///         > Data type: Array of all health values.
///         > Could learn to prioritize hurt tokens on enemy team.
///     > What actions tokes can make.
///         > Data type: int for index.
///         > Can make a decision on what to do.
///         > Human can't see what tokens can do but they know it by playing the game.
///     > Valid targets at current position.
///         > Data type: List of tokens.
///         > Can make a decision on what to do.
/// 
/// Agent could observe the environment by getting the gamebaord from the GameManager.
/// Reward function could be:
///     > (d HP_player1 - d HP_player2) / round.
///         > A problem could emerge if the AI decides that just hurting the token is more important than actually eliminating it.
///     > What is a round.
///         > Round is when both players have had a chance to do something.
///         > OR when all tokens have been used.
///     
/// Agent action space:
///     > Move(Direction).
///         > As tokens could be moved by arbitary amount, the AI should move tokens one square at the time.
///     > DoAction(actionSlot).
///     > EndTurn().
/// </summary>
public class MLAgentShapedActions : Agent
{
    [SerializeField] private GameManager gameManager;
    EnvironmentParameters environmentParameters;
    public AIState currentAIState { get; set; }

    private int invalidMoveCount = 0;
    private int maxInvalidMoves = 5;

    public int ownTeamNumber { get; set; }
    public int enemyTeamNumber { get; set; }

    /// <summary>
    /// This is used to interpret the action for movement. 
    /// Expressed as cardinal directions: 0 is North, 1 is North-East and so on.
    /// </summary>
    private readonly int[,] directions = new int[,] { { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { -1, 1 }};

    private void Start()
    {
        // Get game manager reference so we can use it later
        // gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    /// <summary>
    /// When new episode begings do the following instructions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Debug.Log("Starting new episode");
        gameManager.ResetGame(); // reset whole gameboard again
        currentAIState = AIState.waitingTurn;

        /*teamTotalHP = gameManager.GetTeamHP(ownTeamNumber);
        ownHPPercentage = 1 / teamTotalHP;
        enemyTotalHP = gameManager.GetTeamHP(enemyTeamNumber);
        enemyHPPercentage = 1 / enemyTotalHP;*/

    }


    /// <summary>
    /// Collect needed observations to make a decision.
    /// This method is used as the gamestate is represented abstractically in the code.
    /// We're using a vector of observations which include the following:
    ///     > Layout of the map as list of floats
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Input map layout
        // 54 bools
        foreach(bool b in gameManager.GetGameboard().GetTileWalkabilityMap())
        {
            sensor.AddObservation(b);
        }

        // Current token type
        // 1 int
        sensor.AddObservation(gameManager.GetCurrentTokenType());

        // Current position
        // 54 bools
        foreach (bool b in gameManager.GetCurrentLocationAsBoolList())
        {
            sensor.AddObservation(b);
        }

        // Current movement area
        // 54 bool
        foreach (bool b in gameManager.GetMovementAreaAsBools())
        {
            sensor.AddObservation(b);
        }

        // Locations of own team tokens
        // 54 bools
        foreach (bool b in gameManager.GetTokenLocationsBool(ownTeamNumber))
        {
            sensor.AddObservation(b);
        }

        // Locations of enemy team tokens
        // 54 bools
        foreach (bool b in gameManager.GetTokenLocationsBool(enemyTeamNumber))
        {
            sensor.AddObservation(b);
        }

        // Valid targets
        // 216 bools
        foreach (bool b in gameManager.GetValidTargetForEachActionBool())
        {
            sensor.AddObservation(b);
        }

        // How many actions token can make
        // 1 int
        sensor.AddObservation(gameManager.playerActionsLeftOnTurn);

        // HP on each tile
        // 54 float
        sensor.AddObservation(gameManager.GetHPonTiles());
    }


    /// <summary>
    /// Remade actions to include less useless actions.
    /// This is done by instead of making AI choose the coordinates of the action, we make it choose the valid target from the list.
    /// This way the AI doesn't need to learn about proper targeting, instead it can focus on the strategy aspect.
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // If it's not current AI's turn, then don't do anything
        if (currentAIState != AIState.playingTurn)
            return;

        int actionMoveOrEnd = actions.DiscreteActions[0]; // 3 values
        int actionIdex = actions.DiscreteActions[1]; // 4 values
        int actionTargetIdex = actions.DiscreteActions[2]; // 9 values
        int moveDirection = actions.DiscreteActions[3]; // 8 values

        gameManager.selectedAction = gameManager.heroActions[actionIdex];
        gameManager.GetValidTargets();
        List<Tile> validTargets = gameManager.validTargets;
        if (validTargets.Count < 9)
        {
            while (validTargets.Count < 9)
            {
                validTargets.Add(null);
            }
        }

        // If there's valid targets but the AI doesn't try to do actions
        if (!validTargets.Contains(null) && actionMoveOrEnd != 0)
        {
            AddReward(-0.5f);
        }

        switch (actionMoveOrEnd)
        {
            case (0):
                // Do action
                // Give reward for trying to do actions
                AddReward(0.5f);

                Tile originTile = gameManager.currentPlayer.GetGameObject().GetComponentInParent<Tile>();
                Tile targetTile = gameManager.validTargets[actionTargetIdex];

                if (gameManager.DoSelectedAction(originTile, targetTile))
                {
                    // Give really big reward for doing action correctly
                    AddReward(2.5f);
                }
                else
                {
                    invalidMoveCount++;
                }
                
                break;
            case (1):
                // Move to direction
                int relativeXmove = directions[moveDirection, 0];
                int relativeZmove = directions[moveDirection, 1];

                float previousDistance = gameManager.AverageDistanceToTeam(enemyTeamNumber);

                if (gameManager.MovePlayer(relativeXmove, relativeZmove))
                {
                    // Give reward for moving towards the enemies
                    if (gameManager.AverageDistanceToTeam(enemyTeamNumber) < previousDistance)
                    {
                        AddReward(0.25f);
                    }
                }
                else
                {
                    invalidMoveCount++;
                }
                break;
            case (2):
                // End turn
                gameManager.EndTurn();
                invalidMoveCount = 0;
                currentAIState = AIState.waitingTurn;
                AddReward(0.1f);
                // Give reward for ending turn
                break;
        }

        // If AI does too many invalid moves during the turn
        if (invalidMoveCount >= maxInvalidMoves)
        {
            invalidMoveCount = 0;
            currentAIState = AIState.waitingTurn;
            gameManager.EndTurn();
        }

        // If AI can't do anything else on the turn
        // Seperated so the reward can be different from invalid moves
        if (gameManager.movementArea.Count <= 0 && gameManager.playerActionsLeftOnTurn <= 0)
        {
            invalidMoveCount = 0;
            currentAIState = AIState.waitingTurn;
            gameManager.EndTurn();
        }

        // If one of the teams is eliminated
        if (gameManager.winnerTeamNumber != -1)
        {
            EndEpisode();
        }
    }


    /// <summary>
    /// Used to test heurestics of the agent. Turns user's inputs to agent's inupts.
    /// </summary>
    /// <param name="actionsOut"></param>
    /*public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Writing to action buffers");

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

    }*/


    /// <summary>
    /// Offset all the values in a float list by some amount.
    /// This is used for the AI because it only can get values from zero and up.
    /// </summary>
    /// <param name="originalList">Original list of floats</param>
    /// <param name="offsetAmount">Amount of offset</param>
    /// <returns>New offset list</returns>
    private List<float> Offsetter(List<float> originalList, float offsetAmount)
    {
        List<float> offsettedList = new List<float>();

        foreach(float f in originalList)
        {
            offsettedList.Add(f + offsetAmount);
        }

        return offsettedList;
    }


    /// <summary>
    /// Offset all the values in a Vector2 by some amount.
    /// This is used for the AI because it only can get values from zero and up.
    /// </summary>
    /// <param name="originalVector2">Original Vector2</param>
    /// <param name="offsetAmount">Offset amount</param>
    /// <returns>New offset Vector2</returns>
    private Vector2 Offsetter(Vector2 originalVector2, float offsetAmount)
    {
        return new Vector2(originalVector2.x + offsetAmount, originalVector2.y + offsetAmount);
    }


    /// <summary>
    /// This method exists because I tried to implement list of bools as observations but the AI only supports float lists
    /// </summary>
    /// <param name="boolList"></param>
    /// <returns></returns>
    private List<float> BoolListToFloatList(List<bool> boolList)
    {
        List<float> floatList = new List<float>();

        foreach(bool b in boolList)
        {
            floatList.Add(System.Convert.ToSingle(b));
        }

        return floatList;
    }

}
/// Other notes AI:
///     > As per https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Design-Agents.md#decisions, function Agent.RequestDecision() should be done manually
///     > Turnbased game --> discrete actions https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Design-Agents.md#discrete-actions
///         > Multiple branches for each action
///         > In this case Move, action and end turn
///     > Reward function should be simple
///         > Reward results rather than actions --> Give big reward if enemy token is eliminated
///         > Maybe give negative reward, if dHP is zero
///     > Giving reward
///         > AddReward() or SetReward() 
///             > Range should be [-1,1]
///     > Agent properties https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Design-Agents.md#discrete-actions
///     > Multi-agent scenario https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Design-Agents.md#defining-multi-agent-scenarios
///     > Must create trainer config https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-ML-Agents.md#training-configurations
///         > Symmetric game, this the same policy
/// 
/// Other notes config:
///     > trainer_type=ppo
///     > self-play
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public enum AIState
{
    error = -1,
    waitingTurn = 0,
    playingTurn = 1
}

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
public class MLAgent : Agent
{
    [SerializeField] private GameManager gameManager;
    EnvironmentParameters environmentParameters;
    public AIState aIState { get; private set; }

    /// <summary>
    /// This is used to interpret the action for movement. 
    /// Expressed as cardinal directions: 0 is North, 1 is North-East and so on.
    /// </summary>
    private readonly int[,] directions = new int[,] { { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { -1, 1 }};

    private void Start()
    {
        // Get game manager reference so we can use it later
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    /// <summary>
    /// When new episode begings do the following instructions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        Debug.Log("Starting new episode");
        gameManager.ResetGame(); // reset whole gameboard again
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
        Debug.Log("AI is observing");
        int enemyTeamNumber = -1;
        if (gameManager.currentPlayer.GetPlayerTeam() == 0)
        {
            enemyTeamNumber = 1;
        }
        else
        {
            enemyTeamNumber = 0;
        }

        if (gameManager.currentPlayer.GetPlayerTeam() != enemyTeamNumber)
        {
            // Get the layout of the map as float list. Originally tried to use 2D array of enums but the library requires list of floats in this case
            // For example, 0.0f is walkable, 1.0f is a wall.
            sensor.AddObservation(gameManager.GetGameboard().GetTileTypeMap());
            sensor.AddObservation(gameManager.GetCurrentTokenType());
            sensor.AddObservation(gameManager.GetCurrentTokenCoordinates());
            sensor.AddObservation(gameManager.GetTokenLocations(gameManager.currentPlayer.GetPlayerTeam()));
            sensor.AddObservation(gameManager.GetTokenLocations(enemyTeamNumber));
            sensor.AddObservation(gameManager.GetValidTargetForEachAction());
            sensor.AddObservation(gameManager.GetMovementAreaAsFloats());
            sensor.AddObservation(gameManager.playerActionsLeftOnTurn);

            RequestDecision();
        }
    }


    /// <summary>
    /// Used to test heurestics of the agent. Turns user's inputs to agent's inupts.
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Writing to action buffers");

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

    }


    /// <summary>
    /// Called when the agent has to do an action.
    /// Also include the reward, which is in this case total damage done or negated.
    /// 
    /// The plan is forthe AI to move one tile at the time.
    /// Actions can get values starting from 0, so we need to rethink this a bit
    /// </summary>
    /// <param name="actions">List of inputs that will determine actions</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Debug.Log("AI is trying to do something");

        Debug.Log("Move dir.: "   + actions.DiscreteActions[0]  
                + "\nAction index: " + actions.DiscreteActions[1] 
                + "\nAction x: " + actions.DiscreteActions[2] 
                + "\nAction z: " + actions.DiscreteActions[3] 
                + "\nEnd turn: " + actions.DiscreteActions[4]
            );

        int movementDirection = actions.DiscreteActions[0] - 1; // -1, 0, 1, ..., 7
        int actionNumber = actions.DiscreteActions[1] - 1; // -1, 0, 1, 2, 3
        int actionCoordX = actions.DiscreteActions[2] - 1; // 0...6
        int actionCoordZ = actions.DiscreteActions[3] - 1; // 0...9
        int endTurn = actions.DiscreteActions[4];

        // Move token, if failed, set reward as negative value
        if (movementDirection != -1)
        {
            // Moving player to certain direction
            // Get current game piece's location and move it from that location to new one
            // Seperated this here so the code could be more readable
            int relativeXmove = directions[movementDirection, 0];
            int relativeZmove = directions[movementDirection, 1];

            // Debug.Log("AI relative move: (" + relativeXmove + ", " + relativeZmove + ")");

            // If the move is a success
            if (gameManager.MovePlayer(relativeXmove, relativeZmove))
            {
                AddReward(0.05f);
            }
            else // if fail
            {
                SetReward(-1f);
            }
        }

        // Do one of the token's actions, if failed, set reward as negative
        if (actionNumber != -1)
        {
            // Do selected action
            // Get current game piece's location abd action's target location
            if(gameManager.DoSelectedAction(actionCoordX, actionCoordZ))
            {
                AddReward(0.05f);
            }
            else
            {
                SetReward(-1f);
            }
        }

        if (endTurn == 1)
        {
            gameManager.EndTurn();
        }

        if (gameManager.winnerTeamNumber != -1)
        {
            EndEpisode();
        }

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
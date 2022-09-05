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
///     > As game is turn based, the agent's actions are sequntial
/// 
/// What the agent needs to make a decision:
///     > Layout of the map
///         > Data type: 2D array with tiles represented as numbers
///         > Where the token can be moved to
///         > Can token use action over certain obsticles
///     > Where other tokens are
///         > Data type: Enum
///         > Can potentially create strategy with using all the tokens
///     > How healthy the tokens are
///         > Data type: Array of all health values
///         > Could learn to prioritize hurt tokens on enemy team
///     > What actions tokes can make
///         > Data type: Enum
///         > Can make a decision on what to do
///         > Human can't see what tokens can do but they know it by playing the game
///     > Valid targets at current position
///         > Data type: List of tokens
///         > Can make a decision on what to do
///     > Agent can make actions/move on turn
///         > Data type: bool
///         > If there's nothing to do, then turn should end
/// 
/// Agent could observe the environment by getting the gamebaord from the GameManager.
/// Reward function could be:
///     > (d HP_player1 - d HP_player2) / round
///         > A problem could emerge if the AI decides that just hurting the token is more important than actually eliminating it
///     > What is a round
///         > Round is when both players have had a chance to do something
///         > OR when all tokens have been used
///     
/// Agent action space:
///     > Move(Direction)
///     > DoAction(actionSlot)
///     > EndTurn()
/// </summary>
public class MLAgent : Agent
{
    private GameManager gameManager;
    private PlayerGamePiece playerGamePiece;
    EnvironmentParameters environmentParameters;


    private void Start()
    {
        // Get game manager reference so we can use it later
    }

    /// <summary>
    /// When new episode begings do the following instructions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        // TODO reset whole gameboard again
    }


    /// <summary>
    /// Collect needed observations to make a decision.
    /// This method is used as the gamestate is represented abstractically in the code.
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        // TODO
    }


    /// <summary>
    /// Called when the agent has to do an action.
    /// Also include the reward, which is in this case total damage done or negated.
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        // TODO
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
///     > Multi-agent scenario https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Design-Agents.md#discrete-actions
///     > Must create trainer config https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-ML-Agents.md#training-configurations
///         > Symmetric game, this the same policy
/// 
/// Other notes config:
///     > trainer_type=ppo
///     > self-play

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Purpose of this class is to manage ML agents from other script so the GameManager doesn't bloat.
/// </summary>
public class AIManager : MonoBehaviour
{
    [SerializeField] private MLAgent[] agents;
    [SerializeField] private GameManager gameManager;

    private void Start()
    {
        agents[0].ownTeamNumber = 0;
        agents[1].ownTeamNumber = 1;

        agents[0].enemyTeamNumber = 1;
        agents[1].enemyTeamNumber = 0;
    }

    public void MakeAIPlayTurn(int teamNumber)
    {
        agents[teamNumber].currentAIState = AIState.playingTurn;
    }
}

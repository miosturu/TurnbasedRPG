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

    private int playerTurn;

    private void Start()
    {
        playerTurn = gameManager.currentPlayer.GetPlayerTeam();
    }

    public void MakeAIPlayTurn(int teamNumber)
    {
        // Debug.Log("MakeAIPlayTurn() was called");
        agents[teamNumber].RequestAction();
    }
}

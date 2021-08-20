using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage player turns. This class has reference to first player and knows what player is being played at the moment.
/// One can add new players based on initiative and remove wanted players.
/// </summary>
public class TurnManager
{
    public PlayerTurn firstPlayer;
    public PlayerTurn currentPlayer;


    /// <summary>
    /// Add new player to the initiative.
    /// </summary>
    /// <param name="player">To be added player.</param>
    public void AddPlayerToList(IGamePiece player, int teamNumber)
    {
        string pName = player.GetGameObject().name;
        // Debug.Log("Adding player " + pName);

        PlayerTurn playerTurn = new PlayerTurn(player, teamNumber);

        if (firstPlayer == null)
        {
            // Debug.Log("It looks like that " + pName + " will be first");
            firstPlayer = playerTurn;
            firstPlayer.next = playerTurn;

            currentPlayer = firstPlayer;
        }
        else
        {
            // Debug.Log(pName + " won't be first");
            PlayerTurn indicator = firstPlayer;

            while (indicator.next != firstPlayer)
            {
                indicator = indicator.next;
            }

            indicator.next = playerTurn;
            playerTurn.next = firstPlayer;
        }
    }


    /// <summary>
    /// Add players to list based on the initiative.
    /// </summary>
    /// <param name="player">Added player</param>
    /// <param name="teamNumber">Player's team number</param>
    /// <param name="initiative">Player's initiative</param>
    public void AddPlayerToList(IGamePiece player, int teamNumber, int initiative)
    {
        Debug.Log("Adding new player " + player.GetGameObject().name + " to list. Ini: " + initiative);
        PlayerTurn playerTurn = new PlayerTurn(player, teamNumber, initiative);

        if (firstPlayer == null) // If there's no other players
        {
            firstPlayer = playerTurn;
            firstPlayer.next = playerTurn;
        }
        else // If there're other players
        {
            if (initiative > firstPlayer.initiative)
            {
                // Find last of the list
                PlayerTurn indicator = firstPlayer;
                while(indicator.next != firstPlayer)
                {
                    indicator = indicator.next;
                }

                playerTurn.next = firstPlayer;
                indicator.next = playerTurn;
                firstPlayer = playerTurn;

            }
            else
            {
                PlayerTurn indicator = firstPlayer;
                while(indicator.next.initiative < playerTurn.initiative)
                {
                    indicator = indicator.next;
                }

                playerTurn.next = indicator.next;
                indicator.next = playerTurn;
            }
        }

        currentPlayer = firstPlayer;
    }


    /// <summary>
    /// Remove wanted player from the initiative rotation
    /// </summary>
    /// <param name="player">To be deleted player</param>
    public void RemovePlayer(IGamePiece player)
    {
        // Debug.Log("Removing player " + player.GetGameObject().name);

        PlayerTurn indA = firstPlayer;
        PlayerTurn indB = null;

        while (indA.next.player != player)
        {
            indA = indA.next;
        }

        indB = indA.next;

        indA.next = indB.next;
    }


    /// <summary>
    /// Set current player to be next one.
    /// </summary>
    public IGamePiece NextTurn()
    {
        currentPlayer = currentPlayer.next;
        //Debug.Log("Turn ended, nexplayer: " + currentPlayer.player.GetGameObject().name);
        return currentPlayer.player;
    }


    /// <summary>
    /// Get wanted player's team number
    /// </summary>
    /// <param name="player">Player who's team number is wanted</param>
    /// <returns>Player's team number</returns>
    public int GetPlayerTeamNumber(IGamePiece player)
    {
        PlayerTurn indicator = firstPlayer;

        while(indicator != indicator.player)
        {
            indicator = indicator.next;
        }

        return indicator.teamNumber;
    }
}

/// <summary>
/// Sub class of the TurnManager. Holds needed info of the players, such as player, who will play next, player's initiative and player's team.
/// </summary>
public class PlayerTurn
{
    public IGamePiece player;
    public PlayerTurn next;
    public int initiative;
    public int teamNumber;


    /// <summary>
    /// Create new PlayerTurn object for containing player info.
    /// </summary>
    /// <param name="player">Player</param>
    /// <param name="teamNumber">Player's team number</param>
    public PlayerTurn(IGamePiece player, int teamNumber)
    {
        this.player = player;
        this.teamNumber = teamNumber;
    }


    /// <summary>
    /// Create new PlayerTurn object for containing player info.
    /// </summary>
    /// <param name="player">Player</param>
    /// <param name="teamNumber">Player's team number</param>
    /// <param name="initiative">Player's initiative</param>
    public PlayerTurn(IGamePiece player, int teamNumber, int initiative)
    {
        this.player = player;
        this.teamNumber = teamNumber;
        this.initiative = initiative;
    }
}
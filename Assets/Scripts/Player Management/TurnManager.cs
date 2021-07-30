using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager
{
    public PlayerTurn firstPlayer;
    public PlayerTurn currentPlayer;


    /// <summary>
    /// Add new player to the initiative.
    /// </summary>
    /// <param name="player">To be added player.</param>
    public void AddPlayerToList(IGamePiece player)
    {
        string pName = player.GetGameObject().name;
        // Debug.Log("Adding player " + pName);

        PlayerTurn playerTurn = new PlayerTurn(player);

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
}

public class PlayerTurn
{
    public IGamePiece player;
    public PlayerTurn next;
    // public int initiative;

    public PlayerTurn(IGamePiece player)
    {
        this.player = player;
    }
}
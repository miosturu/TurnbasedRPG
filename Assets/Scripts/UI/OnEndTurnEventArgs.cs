using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When the turn ends, this class is used to communicate to the UI to turn on or off some UI elements.
/// </summary>
public class OnEndTurnEventArgs : EventArgs
{
    private bool isPlayerTurn;
    private bool playerCanMakeActions;


    public OnEndTurnEventArgs(bool isPlayerTurn)
    {
        this.isPlayerTurn = isPlayerTurn;
    }


    public OnEndTurnEventArgs(bool isPlayerTurn, bool playerCanMakeActions)
    {
        this.isPlayerTurn = isPlayerTurn;
        this.playerCanMakeActions = playerCanMakeActions;
    }


    public bool GetIsPlayerTurn()
    {
        return isPlayerTurn;
    }


    public bool GetPlayerCanMakeActions()
    {
        return playerCanMakeActions;
    }
}

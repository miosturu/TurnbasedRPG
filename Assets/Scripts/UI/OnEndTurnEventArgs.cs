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
    private bool canMakeActionsBlock;


    public OnEndTurnEventArgs(bool isPlayerTurn)
    {
        this.isPlayerTurn = isPlayerTurn;
    }


    /// <summary>
    /// OnEndTurnEvent was named when it was only used when ending turn but at the moment it's used to update the UI.
    /// </summary>
    /// <param name="isPlayerTurn"></param>
    /// <param name="canMakeActionsBlock"></param>
    public OnEndTurnEventArgs(bool isPlayerTurn, bool canMakeActionsBlock)
    {
        this.isPlayerTurn = isPlayerTurn;
        this.canMakeActionsBlock = canMakeActionsBlock;
    }


    public bool GetIsPlayerTurn()
    {
        return isPlayerTurn;
    }


    public bool GetcanMakeActionsBlock()
    {
        return canMakeActionsBlock;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

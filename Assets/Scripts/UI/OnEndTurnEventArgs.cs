using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEndTurnEventArgs : EventArgs
{
    private bool isPlayerTurn;

    public OnEndTurnEventArgs(bool isPlayerTurn)
    {
        this.isPlayerTurn = isPlayerTurn;
    }


    public bool GetIsPlayerTurn()
    {
        return isPlayerTurn;
    }
}

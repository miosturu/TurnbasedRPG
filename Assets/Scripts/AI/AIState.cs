using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enumeration of AI's state. This is used to controll AI's actions during gameplay.
/// </summary>
public enum AIState
{
    error = -1, // Should never occur
    waitingTurn = 0,
    playingTurn = 1
}
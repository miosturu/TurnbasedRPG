using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Roll dice.
/// </summary>
public class DiceRoller
{
    public int RollDice(int numberOfDice, int faces)
    {
        int total = 0;

        for (int i = 0; i < numberOfDice; i++)
        {
            total += (int)Random.Range(1, faces - 1);
        }

        //Debug.Log("<color=red>Total: </color>" + total);

        return total;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGamePiece : MonoBehaviour, IGamePiece
{
    [Header("Movement")]
    public int movementSpeed = 2;
    public int movementLeft = 2; 

    [Header("HP")]
    public int currentHp = 10;
    public int maxHp = 10;

    [Header("Armor")]
    public int armorDie = 6;

    [Header("Other")]
    public GameObject highlight;
    public SpriteRenderer sprite;
    public ActionScriptableObject[] actions;


    /// <summary>
    /// Heal player for certain amount. Prevents over healing.
    /// </summary>
    /// <param name="amount">Heal amount</param>
    public void Heal(int amount)
    {
        currentHp += amount;
        if (currentHp > maxHp)
            currentHp = maxHp;
    }


    /// <summary>
    /// Take dAmount of damage
    /// </summary>
    /// <param name="amount">Damage die</param>
    public void TakeDamage(int amount)
    {
        int reduction = (int)Random.Range(1.0f, (float)armorDie);

        if (amount - reduction > 0)
            currentHp -= amount;
        if (currentHp <= 0)
        {
            // Player's character dies
            enabled = false;
        }
    }


    /// <summary>
    /// Get the game object where this component is attached.
    /// </summary>
    /// <returns>Game object of the component</returns>
    public GameObject GetGameObject()
    {
        return this.gameObject;
    }


    /// <summary>
    /// Enable/disable player's highlight.
    /// </summary>
    /// <param name="state">On/off</param>
    public void HighlightSetActive(bool state)
    {
        highlight.SetActive(state);
    }


    /// <summary>
    /// Get how much the player can move yet.
    /// </summary>
    /// <returns>Movement left</returns>
    public int GetCurrentMovementLeft()
    {
        return movementLeft;
    }


    /// <summary>
    /// Reduce how much more player can move.
    /// </summary>
    /// <param name="amount"></param>
    public void ReduceMovement(int amount)
    {
        movementLeft -= amount;
    }


    public void ResetMovement()
    {
        movementLeft = movementSpeed;
    }


    public ActionScriptableObject[] GetActions()
    {
        return actions;
    }

    public int GetCurrentHp()
    {
        return currentHp;
    }

    public int GetMaxHp()
    {
        return maxHp;
    }
}

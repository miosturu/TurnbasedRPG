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

    [Header("Action")]
    public ActionScriptableObject[] actions;
    public int maxActionsPerTurn = 1;
    public int actionsLeft = 1;

    [Header("Other")]
    [SerializeField] private PlayerHpUIManager playerHpUIManager;
    public GameObject highlight;
    public SpriteRenderer sprite;
    public int team;
    public PlayerType playerType;
    public Tokens tokenType;


    /// <summary>
    /// Heal player for certain amount. Prevents over healing.
    /// </summary>
    /// <param name="amount">Heal amount</param>
    public void Heal(int amount)
    {
        currentHp += amount;
        if (currentHp > maxHp)
            currentHp = maxHp;

        playerHpUIManager.ChangeStatusBarWidth(currentHp);

        Debug.Log("Healing for " + amount + " HP. Current HP.:" + currentHp);
    }


    /// <summary>
    /// Take dX of damage. 'D' stands for die/dice. For example if d=6, then roll regular six sided die.
    /// </summary>
    /// <param name="amount">Damage die</param>
    public void TakeDamage(int amount)
    {
        int reduction = new DiceRoller().RollDice(1, armorDie);

        // Debug.Log("Damage roll: " + amount + ". Armor roll: " + reduction);

        if (amount - reduction > 0)
        {
            currentHp -= amount;
            playerHpUIManager.ChangeStatusBarWidth(currentHp);
            Debug.Log(name + " took " + amount + " damage.");
        }
        else
        {
            Debug.Log("Attack didn't go through the armor");
        }
            
        if (currentHp <= 0)
        {
            // Player's character dies
            Debug.Log(name + " took " + amount + " damage and died");
            //GameObject.Find("GameManager").GetComponent<GameManager>().RemovePlayer(this);
            GetComponentInParent<GameManager>().RemovePlayer(this);
            gameObject.SetActive(false);
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


    /// <summary>
    /// Set player's movement to be max movement
    /// </summary>
    public void ResetMovement()
    {
        movementLeft = movementSpeed;
    }


    /// <summary>
    /// Return player's available actions
    /// </summary>
    /// <returns>Available actions</returns>
    public ActionScriptableObject[] GetActions()
    {
        return actions;
    }


    /// <summary>
    /// Return player's current HP.
    /// </summary>
    /// <returns>Current HP.</returns>
    public int GetCurrentHp()
    {
        return currentHp;
    }


    /// <summary>
    /// Return player's max HP.
    /// </summary>
    /// <returns>Player's max HP.</returns>
    public int GetMaxHp()
    {
        return maxHp;
    }


    /// <summary>
    /// Return how many more actions player can take this turn.
    /// </summary>
    /// <returns>How many actions player can take</returns>
    public int GetHowManyActionsLeft()
    {
        return actionsLeft;
    }


    /// <summary>
    /// Decrease how many more actions player can take this turn.
    /// </summary>
    /// <param name="i">How many actions player took.</param>
    public void DecreaseActionsLeft(int i)
    {
        actionsLeft -= i;
    }


    /// <summary>
    /// Get total actions player can take per turn.
    /// </summary>
    /// <returns>Max actions per turn</returns>
    public int GetMaxActionsPerTurn()
    {
        return maxActionsPerTurn;
    }


    /// <summary>
    /// Return player's team
    /// </summary>
    /// <returns></returns>
    public int GetPlayerTeam()
    {
        return team;
    }


    /// <summary>
    /// Return if player is:
    ///     1) Tank
    ///     2) Support
    ///     3) Offence
    /// -type character
    /// </summary>
    /// <returns>Player's class type</returns>
    public PlayerType GetPlayerType()
    {
        return playerType;
    }


    /// <summary>
    /// Set token's team.
    /// </summary>
    /// <param name="teamNumber">New team number. For human it's 0, for AI it's 1.</param>
    public void SetPlayerTeam(int teamNumber)
    {
        team = teamNumber;
    }


    /// <summary>
    /// Get this token's type i.e. 'class'.
    /// </summary>
    /// <returns>Token's type.</returns>
    public Tokens GetTokenType()
    {
        return tokenType;
    }
}

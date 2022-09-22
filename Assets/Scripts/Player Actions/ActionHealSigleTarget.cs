using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Healing action. Create a new scriptable object in the editor and change the settings to your liking.
/// The healer can only heal the tokens on the same team that are in range.
/// What one can choose in a healing action:
///     1) How much the action heals.
///     2) How far one can heal the target.
///     3) The target needs to be seen.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/HealSingleTarget")]
public class ActionHealSigleTarget : ActionScriptableObject
{
    public int healDie;
    public int range;
    public bool needsLineOfSight;

    public override bool Action(Tile origin, Tile target)
    {
        if (TargetIsValid(origin, target))
        {
            target.currentObject.GetComponent<IGamePiece>().Heal(new DiceRoller().RollDice(1, healDie));
            Debug.Log("Tile can be seen and healing was done");
            return true;
        }
        else
        {
            Debug.Log("Healing failed: tile not in line of sight");
            return false;
        }
    }

    public override string GetDescription()
    {
        return string.Format("Heal die: {0} \nRange: {1}", healDie, range);
    }


    /// <summary>
    /// Check if the heal target is valid. Requirements are:
    ///     1) Target tile is not empty
    ///     2) Target's and origin's teams are same
    ///     3) Line of sight is ok
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="target">Target tile</param>
    /// <returns>Heal is valid</returns>
    public override bool TargetIsValid(Tile origin, Tile target)
    {
        try
        {
            if
                (
                    target.currentObject != null && // Target tile is not empty
                    (origin.GetComponentInChildren<IGamePiece>().GetPlayerTeam() == target.GetComponentInChildren<IGamePiece>().GetPlayerTeam()) && // Target's and origin's teams are same
                    (!needsLineOfSight || new LineOfSight().TileCanBeSeenAndIsInDistance(origin.GetGameboardOfTile(), origin, target, range)) // Line of sight is ok
                    || target == origin // Heal is being cast to self
                )
                return true;

            return false;
        }
        catch
        {
            //Debug.LogError("Tried to target invalid tile: " + origin.name + " vs. " + target.name);
            return false;
        }
    }
}

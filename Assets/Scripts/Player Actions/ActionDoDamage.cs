using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/DoDamage")]
public class ActionDoDamage : ActionScriptableObject
{
    public int damageDie;
    public int range;
    public bool needsLineOfSight;

    public override bool Action(Tile origin, Tile target)
    {
        if (TargetIsValid(origin, target))
        {
            target.currentObject.GetComponent<IGamePiece>().TakeDamage(new DiceRoller().RollDice(1, damageDie));
            return true;
        }
        else
        {
            Debug.Log("Attack failed");
            return false;
        }
    }


    /// <summary>
    /// Check if the target is valid for attacking. Requirements are:
    ///     1) Target tile is empty
    ///     2) Target's and origin's teams are different
    ///     3) Not targeting self
    ///     4) Target is within line of sight
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="target">Target tile</param>
    /// <returns>Attack is valid</returns>
    public bool TargetIsValid(Tile origin, Tile target)
    {
        if 
        (
            target.currentObject != null && // Target tile is empty
            (origin.GetComponentInChildren<IGamePiece>().GetPlayerTeam() != target.GetComponentInChildren<IGamePiece>().GetPlayerTeam()) && // Target's and origin's teams are different
            origin != target && // Not targeting self
            (!needsLineOfSight || new LineOfSight().TileCanBeSeenAndIsInDistance(origin.GetGameboardOfTile(), origin, target, range)) // Target is within line of sight
        ) return true;

        return false;
    }
}

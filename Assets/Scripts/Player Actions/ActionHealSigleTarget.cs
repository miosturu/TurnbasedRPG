using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    /// <summary>
    /// Check if the heal target is valid. Requirements are:
    ///     1) Target tile is not empty
    ///     2) Target's and origin's teams are same
    ///     3) Line of sight is ok
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="target">Target tile</param>
    /// <returns>Heal is valid</returns>
    public bool TargetIsValid(Tile origin, Tile target)
    {
        if
        (
            target.currentObject != null && // Target tile is not empty
            (origin.GetComponentInChildren<IGamePiece>().GetPlayerTeam() == target.GetComponentInChildren<IGamePiece>().GetPlayerTeam()) && // Target's and origin's teams are same
            (!needsLineOfSight || new LineOfSight().TileCanBeSeenAndIsInDistance(origin.GetGameboardOfTile(), origin, target, range)) // Line of sight is ok
        )
            return true;

        return false;
    }
}

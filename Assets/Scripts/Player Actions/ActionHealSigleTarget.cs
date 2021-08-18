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
        if (target.currentObject != null && (!needsLineOfSight || new LineOfSight().TileCanBeSeenAndIsInDistance(origin.GetGameboardOfTile(), origin, target, range)))
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
}

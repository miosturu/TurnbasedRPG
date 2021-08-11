using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/HealSingleTarget")]
public class ActionHealSigleTarget : ActionScriptableObject
{
    public int healDie;
    public int range;
    public bool needsLineOfSight;

    public override void Action(Tile origin, Tile target)
    {
        if (!needsLineOfSight || new LineOfSight().TileCanBeSeen(origin.GetGameboardOfTile(), origin, target))
        {
            target.currentObject.GetComponent<IGamePiece>().Heal(new DiceRoller().RollDice(1, healDie));
            Debug.Log("Tile can be seen and healing was done");
        }
        else
        {
            Debug.Log("Healing failed: tile not in line of sight");
        }
    }
}

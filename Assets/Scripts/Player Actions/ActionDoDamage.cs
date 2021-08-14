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
        if (target.currentObject != null && origin != target && (!needsLineOfSight || new LineOfSight().TileCanBeSeen(origin.GetGameboardOfTile(), origin, target)))
        {
            target.currentObject.GetComponent<IGamePiece>().Heal(new DiceRoller().RollDice(1, damageDie));
            Debug.Log("Tile can be seen and damage was done");
            return true;
        }
        else
        {
            Debug.Log("Attack failed");
            return false;
        }
    }
}

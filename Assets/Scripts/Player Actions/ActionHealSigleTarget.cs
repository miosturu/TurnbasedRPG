using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/HealSingleTarget")]
public class ActionHealSigleTarget : ActionScriptableObject
{
    public int healDie;
    public int range;
    public bool needsLineOfSight;

    public override void Action(Tile target)
    {
        target.currentObject.GetComponent<IGamePiece>().Heal(healDie); // TODO randomise?
    }
}

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
        int dam = (int)Random.Range(1.0f, (float)damageDie);
        target.currentObject.GetComponent<IGamePiece>().TakeDamage(dam);
        return true;
    }
}

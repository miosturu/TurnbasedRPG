using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/DoDamage")]
public class ActionDoDamage : ActionScriptableObject
{
    public int damageDie;
    public int range;

    public bool needsLineOfSight;

    public override void Action(Tile target)
    {
        target.currentObject.GetComponent<IGamePiece>().TakeDamage((int)Random.Range(1.0f, (float)damageDie));
        Debug.Log("Doing damage");
    }
}

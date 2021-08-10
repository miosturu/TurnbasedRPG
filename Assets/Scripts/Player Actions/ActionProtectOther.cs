using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ActionProtectOther")]
public class ActionProtectOther : ActionScriptableObject
{
    int numberOfturns = 2;

    public override void Action(Tile origin, Tile target)
    {
        Debug.Log("Called 'ActionProtectOther()'-action for " + numberOfturns + " turns");
        throw new System.NotImplementedException();
    }
}

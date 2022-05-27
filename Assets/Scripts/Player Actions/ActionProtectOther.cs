using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Not implemented. Idea was to make this action function like the protect action from 'Darkest Dungeon'.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/ActionProtectOther")]
public class ActionProtectOther : ActionScriptableObject
{
    int numberOfturns = 2;

    public override bool Action(Tile origin, Tile target)
    {
        Debug.Log("Called 'ActionProtectOther()'-action for " + numberOfturns + " turns");
        return true;
    }

    public override string GetDescription()
    {
        return actionDescription;
    }

    public override bool TargetIsValid(Tile origin, Tile target)
    {
        throw new System.NotImplementedException();
    }
}

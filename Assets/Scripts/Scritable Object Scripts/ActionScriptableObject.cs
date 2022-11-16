using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every token can make actions on their turn. Every single action is derived from this class.
/// One could for example use this class to create damaging action or some other arbitary action.
/// </summary>
public abstract class ActionScriptableObject : ScriptableObject
{
    public string actionName;
    public Sprite actionIcon;
    public readonly int actionCost = 1;
    public string actionDescription;

    public abstract bool Action(Tile origin, Tile target);
    public abstract bool TargetIsValid(Tile origin, Tile target);
    public abstract string GetDescription();
    public abstract float GetExpectedValue();
}

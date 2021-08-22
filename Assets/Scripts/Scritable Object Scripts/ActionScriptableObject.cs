using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionScriptableObject : ScriptableObject
{
    public string actionName;
    public Sprite actionIcon;
    public readonly int actionCost = 1;
    public string actionDescription;

    public abstract bool Action(Tile origin, Tile target);
    public abstract bool TargetIsValid(Tile origin, Tile target);
    public abstract string GetDescription();
}

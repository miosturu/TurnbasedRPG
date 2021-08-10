using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionScriptableObject : ScriptableObject
{
    public string actionName;
    public Sprite actionIcon;

    public abstract void Action(Tile origin, Tile target);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile", menuName = "Scriptable Objects/TileScriptableObject", order = 3)]
public class TileScriptableObject : ScriptableObject
{
    public bool isWalkable;
    public bool canBeAttackedOver;
    public Material tileMaterial;
}

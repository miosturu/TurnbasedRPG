using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TileScriptableObject stores information about the tile:
///     1) Is the tile walkable i.e. player can place token on it.
///     2) Can the tile be attacked over i.e. player can see through the tile. Mostly affects ranged attacks.
///     3) What material the tile uses.
/// Third is purely visual, others affect the gameplay to some degree.
/// </summary>
[CreateAssetMenu(fileName = "Tile", menuName = "Scriptable Objects/TileScriptableObject", order = 4)]
public class TileScriptableObject : ScriptableObject
{
    public bool isWalkable;
    public bool canBeAttackedOver;
    public Material tileMaterial;
}

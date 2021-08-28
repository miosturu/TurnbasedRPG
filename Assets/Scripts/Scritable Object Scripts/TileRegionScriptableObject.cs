using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tile region SO. Indexes goes from button left to top right.
/// </summary>
[CreateAssetMenu(fileName = "TileRegion", menuName = "Scriptable Objects/TileRegionScriptableObject", order = 5)]
public class TileRegionScriptableObject : ScriptableObject
{
    public TileRegion region;

    public int regionH = 3;
    public int regionW = 3;

    public TileType[] tiles = new TileType[9];
}

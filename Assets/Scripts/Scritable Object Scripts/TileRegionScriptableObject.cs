using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileRegion", menuName = "Scriptable Objects/TileRegionScriptableObject", order = 5)]
public class TileRegionScriptableObject : ScriptableObject
{
    public TileRegion region;
    public string x0y0, x1y0, x2y0;
    public string x0y1, x1y1, x2y1;
    public string x0y2, x1y2, x2y2;
}

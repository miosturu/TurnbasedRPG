using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum for tile regions. Used in map generation.
/// Each region is 3x3 tiles and are in following order:
/// +---+---+
/// | e | f |
/// | c | d |
/// | a | b |
/// +---+---+
/// </summary>
public enum TileRegion
{
    A,
    B,
    C,
    D,
    E,
    F,
    Universal
}

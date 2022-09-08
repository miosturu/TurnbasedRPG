using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enumeration of all the playable tokens in the game. 
/// Plan is for the AI use this to evaluate situation.
/// Differs from player type bacause type is generic but this is more spesific.
/// Maybe this could be done dynamically but at this time this is good enough.
/// </summary>
public enum Tokens
{
    Jeff,
    Knight,
    Rogue,
    Sage
}

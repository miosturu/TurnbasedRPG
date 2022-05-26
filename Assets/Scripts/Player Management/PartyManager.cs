using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Stores the player party composition i.e. which game piece goes where at the start of the game.
/// </summary>
public class PartyManager : MonoBehaviour
{
    public HeroScriptableObject x0z0, x1z0, x2z0;
    public HeroScriptableObject x0z1, x1z1, x2z1;
    public HeroScriptableObject x0z2, x1z2, x2z2;

    /// <summary>
    /// Get the player's marching order
    /// </summary>
    /// <returns>The party's marhing order</returns>
    public HeroScriptableObject[,] GetMarchingOrder()
    {
        HeroScriptableObject[,] party = new HeroScriptableObject[,] { { x0z0, x1z0, x2z0 }, 
                                                                      { x0z1, x1z1, x2z1 }, 
                                                                      { x0z2, x1z2, x2z2 } };

        return party;
    }
}

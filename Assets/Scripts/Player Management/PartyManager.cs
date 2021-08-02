using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PartyManager : MonoBehaviour
{
    public HeroScriptableObject x0z0, x1z0, x2z0;
    public HeroScriptableObject x0z1, x1z1, x2z1;
    public HeroScriptableObject x0z2, x1z2, x2z2;


    public HeroScriptableObject[,] GetMarchingOrder()
    {
        HeroScriptableObject[,] party = new HeroScriptableObject[,] { { x0z0, x1z0, x2z0 }, 
                                                                      { x0z1, x1z1, x2z1 }, 
                                                                      { x0z2, x1z2, x2z2 } };

        return party;
    }
}

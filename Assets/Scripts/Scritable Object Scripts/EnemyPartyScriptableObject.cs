using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyParty", menuName = "Scriptable Objects/EnemyPartyScriptableObject", order = 3)]
public class EnemyPartyScriptableObject : ScriptableObject
{
    public EnemyScriptableObject x0z0, x1z0, x2z0;
    public EnemyScriptableObject x0z1, x1z1, x2z1;
    public EnemyScriptableObject x0z2, x1z2, x2z2;

    public EnemyScriptableObject[,] GetMarchingOrder()
    {
        EnemyScriptableObject[,] party = new EnemyScriptableObject[,] { { x0z0, x1z0, x2z0 },
                                                                        { x0z1, x1z1, x2z1 },
                                                                        { x0z2, x1z2, x2z2 } };

        return party;
    }
}

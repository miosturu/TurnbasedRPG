using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/EnemyScriptableObject", order = 2)]
public class EnemyScriptableObject : ScriptableObject
{
    public Sprite sprite;

    public string className;

    public int maxHp;
    public int armorDie;
    public int initiativeBonus;
    public int movementSpeed;

    public ActionScriptableObject[] enemyActions;
}

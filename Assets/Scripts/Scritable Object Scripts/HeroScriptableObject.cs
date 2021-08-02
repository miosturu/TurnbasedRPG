using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Hero", menuName = "Scriptable Objects/HeroScriptableObject", order = 1)]
public class HeroScriptableObject : ScriptableObject
{
    public Sprite sprite;

    public string className;

    public int maxHp;
    public int armorDie;
    public int initiativeBonus;
    public int movementSpeed;

    public ActionScriptableObject[] heroActions;
}

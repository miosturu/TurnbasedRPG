using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data of the player token:
///     1) Visuals for the token.
///     2) What the token is called.
///     3) Max HP.
///     4) Armor die i.e. the damage reduction when defending.
///     5) Initiative bonus i.e. some int that will be added to iniative roll.
///     6) Movement max number of tiles player can take per turn.
///     7) What actions are available to the token.
///     8) How many actions the token can take per turn. It's usually one, but if one wants to make more actions the it's possible.
///     9) Player type. It only affects chase AI, no real gameplay effects.
/// </summary>
[CreateAssetMenu(fileName = "Player", menuName = "Scriptable Objects/HeroScriptableObject", order = 1)]
public class HeroScriptableObject : ScriptableObject
{
    public Sprite sprite;

    public string className;

    public int maxHp;
    public int armorDie;
    public int initiativeBonus;
    public int movementSpeed;

    public ActionScriptableObject[] heroActions;
    public int maxActionsPerTurn = 1;
    public PlayerType playerType;
}

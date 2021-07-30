using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGamePiece : MonoBehaviour, IGamePiece
{
    [Header("Movement")]
    public int movementSpeed = 2;
    public int movementLeft = 2; 

    [Header("HP")]
    public int currentHp = 10;
    public int maxHp = 10;

    [Header("Armor")]
    public int armorDie = 6;

    [Header("Other")]

    [SerializeField]
    private GameObject highlight;
    [SerializeField]
    private SpriteRenderer sprite;


    public void Heal(int amount)
    {
        currentHp += amount;
        if (currentHp > maxHp)
            currentHp = maxHp;
    }


    public void TakeDamage(int amount)
    {
        int reduction = (int)Random.Range(1.0f, (float)armorDie);

        if (amount - reduction > 0)
            currentHp -= amount;
        if (currentHp <= 0)
        {
            // Player's character dies
            enabled = false;
        }
    }


    public GameObject GetGameObject()
    {
        return this.gameObject;
    }


    public void HighlightSetActive(bool state)
    {
        highlight.SetActive(state);
    }
}

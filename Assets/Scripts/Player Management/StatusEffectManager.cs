using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    [SerializeField] private int statusEffects = 0b0000;


    private void Start()
    {
        Debug.Log(statusEffects);
        AddStatusEffect(StatusEffect.Protected);
        AddStatusEffect(StatusEffect.Protected);
    }

    public void AddStatusEffect(StatusEffect statusEffect)
    {
        statusEffects += (int)statusEffect;
        Debug.Log(statusEffects.ToString());
    }


    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        statusEffects -= (int)statusEffect;
        Debug.Log(statusEffects.ToString());
    }


    public int GetStatusEffects()
    {
        return statusEffects;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamePiece
{
    void TakeDamage(int amount);
    void Heal(int amount);
    GameObject GetGameObject();
    void HighlightSetActive(bool state);
    int GetCurrentMovementLeft();
    void ReduceMovement(int amount);
    void ResetMovement();
    ActionScriptableObject[] GetActions();
}

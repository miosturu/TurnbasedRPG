using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamePiece
{
    // HP related functions
    void TakeDamage(int amount);
    void Heal(int amount);
    int GetCurrentHp();
    int GetMaxHp();

    // Game object related functions
    GameObject GetGameObject();
    void HighlightSetActive(bool state);

    // Movement related functions
    int GetCurrentMovementLeft();
    void ReduceMovement(int amount);
    void ResetMovement();

    // Player management related functions
    int GetPlayerTeam();
    PlayerType GetPlayerType();

    // Action functions
    ActionScriptableObject[] GetActions();
    int GetHowManyActionsLeft();
    int GetMaxActionsPerTurn();
    void DecreaseActionsLeft(int i);
}

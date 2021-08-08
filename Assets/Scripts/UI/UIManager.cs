using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References to other objects")]
    [SerializeField] private GameManager gameManager;

    [Header("Text elements")]
    [SerializeField] private Text heroNameText;
    [SerializeField] private Text heroHpText;
    [SerializeField] private Text heroMovementText;

    [Header("Actions")]
    [SerializeField] private Button[] abilityButtons;
    [SerializeField] private Text[] abilityNamesTexts;
    [SerializeField] private ActionScriptableObject[] actions;


    // Start is called before the first frame update
    void Start()
    {
        actions = new ActionScriptableObject[4];
        actions = gameManager.currentPlayer.GetActions();
        gameManager.OnEndTurn += UpdateUI;
        UpdateUI();
    } 


    /// <summary>
    /// Update UI subscriber. This method is launched when GameManager sends delegate.
    /// </summary>
    /// <param name="sender">Sender of the message</param>
    /// <param name="e">Arguments. Not needed as of 2021/08/08</param>
    private void UpdateUI(object sender, EventArgs e)
    {
        UpdateUI();
    }


    /// <summary>
    /// Update UI.
    /// </summary>
    private void UpdateUI()
    {
        UpdateHeroInfo();
        UpdateActions();
    }


    /// <summary>
    /// Update buttons that handle actions visuals. Also get the available actions.
    /// </summary>
    public void UpdateActions()
    {
        actions = gameManager.currentPlayer.GetActions();

        for (int i = 0; i < abilityNamesTexts.Length; i++)
        {
            abilityNamesTexts[i].text = actions[i].actionName;
            abilityButtons[i].image.sprite = actions[i].actionIcon;
        }
    }


    /// <summary>
    /// Update UI's information about the hero.
    /// </summary>
    public void UpdateHeroInfo()
    {
        heroNameText.text = "Name: " + gameManager.currentPlayer.ToString();
        heroHpText.text = "HP: " + gameManager.currentPlayer.GetCurrentHp().ToString() + "/" + gameManager.currentPlayer.GetMaxHp().ToString();
        heroMovementText.text = "Mov.: " + gameManager.currentPlayer.GetCurrentMovementLeft().ToString();
    }


    /// <summary>
    /// Function for selecting wanted action from button. If the same action is selected, then deselect it. Otherwise select new action.
    /// For example if button's variable i = 0, then get actions[0] from array.
    /// </summary>
    /// <param name="i">Action's index</param>
    public void GetClickedAction(int i)
    {

        if (gameManager.selectedAction == actions[i]) // If we're clicking the same action
        {
            gameManager.selectedAction = null;
        }
        else // We're clikcing new action
        {
            gameManager.selectedAction = actions[i]; 
        }

    }
}

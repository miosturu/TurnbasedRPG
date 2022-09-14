using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Strings")]
    [SerializeField] private string playerNameString = "Name: ";
    [SerializeField] private string playerHpString = "HP: ";
    [SerializeField] private string playerMovementString = "Mov.: ";

    [Header("References to other objects")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject selectionIndicator;

    [Header("Text elements")]
    [SerializeField] private Text heroNameText;
    [SerializeField] private Text heroHpText;
    [SerializeField] private Text heroMovementText;

    [Header("Actions")]
    [SerializeField] private Text actionInformationText;
    [SerializeField] private Button[] abilityButtons;
    [SerializeField] private Text[] abilityNamesTexts;
    [SerializeField] private ActionScriptableObject[] actions;
    [SerializeField] private RawImage[] selectionHighlights;

    [Header("Agent Testing")]
    [SerializeField] private InputField textInput;
    [SerializeField] private Button submitAgentAction;

    // Start is called before the first frame update
    void Start()
    {
        actions = new ActionScriptableObject[4];
        gameManager.OnEndTurn += UpdateUI;

        heroNameText.text = playerNameString;
        heroHpText.text = playerHpString;
        heroMovementText.text = playerMovementString;

        actionInformationText.text = "";
        foreach(Text text in abilityNamesTexts)
        {
            text.text = "";
        }
    } 


    /// <summary>
    /// Update UI subscriber. This method is launched when GameManager sends delegate.
    /// </summary>
    /// <param name="sender">Sender of the message</param>
    /// <param name="e">Arguments. Not needed as of 2021/08/08</param>
    private void UpdateUI(object sender, OnEndTurnEventArgs e)
    {
        UpdateUI(e.GetIsPlayerTurn(), e.GetcanMakeActionsBlock());
    }


    /// <summary>
    /// Update UI. Is player turn enables or disables buttons, canMakeActionsBlock enables or disables black rectagnle that indicates if actions can be made.
    /// </summary>
    private void UpdateUI(bool isPlayerTurn, bool canMakeActionsBlock)
    {
        UpdateHeroInfo();
        UpdateActions();
        ResetSelectionHighlight();
        EnableOrDisableButtons(isPlayerTurn);
        EnableOrDisableSelectionIndicator(canMakeActionsBlock);
        actionInformationText.text = "";
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
        heroNameText.text = playerNameString + gameManager.currentPlayer.ToString();
        heroHpText.text = playerHpString + gameManager.currentPlayer.GetCurrentHp().ToString() + "/" + gameManager.currentPlayer.GetMaxHp().ToString();
        heroMovementText.text = playerMovementString + gameManager.currentPlayer.GetCurrentMovementLeft().ToString();
    }


    /// <summary>
    /// Function for selecting wanted action from button. If the same action is selected, then deselect it. Otherwise select new action.
    /// For example if button's variable i = 0, then get actions[0] from array.
    /// Reset of the tile must be first, other wise there's problems with it.
    /// </summary>
    /// <param name="i">Action's index</param>
    public void GetClickedAction(int i)
    {
        if (gameManager.selectedAction == actions[i]) // If we're clicking the same action
        {
            gameManager.selectedAction = null;
            selectionHighlights[i].enabled = false;
            actionInformationText.text = "";
            gameManager.ResetValidTargets();
            gameManager.ShowMovementArea();
        }
        else // We're clikcing new action
        {
            gameManager.selectedAction = actions[i];
            foreach(RawImage image in selectionHighlights)
                image.enabled = false;

            selectionHighlights[i].enabled = true;
            actionInformationText.text = gameManager.selectedAction.GetDescription();
            gameManager.ResetMovementArea();
            gameManager.GetValidTargets();
        }

        if (gameManager.selectedAction == null) // Just make sure that there's no highlight if no action is selected. This is here because as of 2021/08/09, one action can be on many button, which can lead to weird behavior
        {
            ResetSelectionHighlight();
            actionInformationText.text = "";
            gameManager.ResetValidTargets();
            gameManager.ShowMovementArea();
        }
    }


    /// <summary>
    /// Reset all action selection highlights
    /// </summary>
    public void ResetSelectionHighlight()
    {
        foreach (RawImage image in selectionHighlights)
            image.enabled = false;
    }


    /// <summary>
    /// Set if the buttons can be clicked.
    /// </summary>
    /// <param name="state">State of buttons. True = can be clicked, False = can't be clicked</param>
    public void EnableOrDisableButtons(bool state)
    {
        endTurnButton.enabled = state;
        foreach(Button button in abilityButtons)
        {
            button.enabled = state;
        }
    }


    /// <summary>
    /// Enable or disable selectionIndicator
    /// </summary>
    /// <param name="state">State of highlight. True = active, False = inactive</param>
    public void EnableOrDisableSelectionIndicator(bool state)
    {
        selectionIndicator.SetActive(state);
    }


    /// <summary>
    /// Make "End Turn"-button clickable or not.
    /// </summary>
    /// <param name="state">Set active or not</param>
    public void EnableOrDisableEndTurnButton(bool state)
    {
        endTurnButton.enabled = state;
    }


    /// <summary>
    /// Used to test targetting
    /// </summary>
    public void SubmitAgentActions()
    {
        try
        {
            String input = textInput.text;
            String[] splitInput = input.Split(',');
            int[] actionAndCoords = new int[3];

            actionAndCoords[0] = Int32.Parse(splitInput[0]);
            actionAndCoords[1] = Int32.Parse(splitInput[1]);
            actionAndCoords[2] = Int32.Parse(splitInput[2]);

            GetClickedAction(actionAndCoords[0]);
            gameManager.DoSelectedAction(actionAndCoords[1], actionAndCoords[2]);
            Debug.Log("Submitting actions");
        }
        catch
        {
            Debug.Log("Error: Either coordinates were not seperated by ',', they're out of range or there's in recognised characters in the input.");
        }

    }
}

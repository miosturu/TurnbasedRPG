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


    // Start is called before the first frame update
    void Start()
    {
        gameManager.OnEndTurn += UpdateUI;
        UpdateUI();
    } 


    private void UpdateUI(object sender, EventArgs e)
    {
        UpdateActions();
        UpdateHeroInfo();
    }

    private void UpdateUI()
    {
        UpdateActions();
        UpdateHeroInfo();
    }


    public void UpdateActions()
    {
        ActionScriptableObject[] actions = gameManager.heroActions;

        for (int i = 0; i < abilityNamesTexts.Length; i++)
        {
            abilityNamesTexts[i].text = actions[i].actionName;
            abilityButtons[i].image.sprite = actions[i].actionIcon;
        }
    }


    public void UpdateHeroInfo()
    {
        heroNameText.text = "Name: " + gameManager.currentPlayer.ToString();
        heroHpText.text = "HP: " + gameManager.currentPlayer.GetCurrentHp().ToString() + "/" + gameManager.currentPlayer.GetMaxHp().ToString();
        heroMovementText.text = "Mov.: " + gameManager.currentPlayer.GetCurrentMovementLeft().ToString();
    }


    public void GetClickedAction(Button button)
    {
        Debug.Log("Clicked action button: " + button.name);
    }
}

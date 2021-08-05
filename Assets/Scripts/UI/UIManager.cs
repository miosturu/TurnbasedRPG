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
        
    }


    private void Update() // TODO use delegates
    {
        UpdateActions();
        UpdateHeroInfo();
    }

    public void UpdateUI()
    {

    }


    public void UpdateActions()
    {
        ActionScriptableObject[] actions = gameManager.heroActions;

        for (int i = 0; i < abilityNamesTexts.Length; i++)
        {
            abilityNamesTexts[i].text = actions[i].name;
            abilityButtons[i].image.sprite = actions[i].actionIcon;
        }
    }


    public void UpdateHeroInfo()
    {
        heroNameText.text = gameManager.currentPlayer.ToString();
    }
}

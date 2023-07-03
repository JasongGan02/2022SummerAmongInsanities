using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : Menu
{
    [Header("Manu Navigation")]
    [SerializeField] private SaveSlotsMenu saveSlotsMenu;

    [Header("Manu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;

    private void Start()
    {
        if(!DataPersistenceManager.instance.HasGameData())
        {
            Debug.Log(DataPersistenceManager.instance.HasGameData());
            continueGameButton.interactable = false;
        }
    }

    public void OnNewGameClicked()
    {
        saveSlotsMenu.ActivateMenu();
        this.DeactivateMenu(); 
    }

    public void OnContinueGameClicked()
    {
        DisableMenuButtons();
        SceneManager.LoadSceneAsync("MainScene");
    }

    private void DisableMenuButtons()
    {
        this.newGameButton.interactable = false;
        this.continueGameButton.interactable = false;
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}

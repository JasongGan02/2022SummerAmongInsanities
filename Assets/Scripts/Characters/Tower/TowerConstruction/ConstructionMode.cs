using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Press KeyCode.C to enter construction mode. Players can place different towers in this mode
public class ConstructionMode : MonoBehaviour
{
    private bool isInConstructionMode = false;

    private CoreArchitecture coreArchitecture;
    private UIViewStateManager uiViewStateManager;
    private PlayerInteraction playerInteraction;
    private TowerContainer towerContainer;

    
    [SerializeField] GameObject ConstructionUI;
    [SerializeField] GameObject EnergyText;
    
    int MaxEnergy;
    int CurrentEnergy;

    // Start is called before the first frame update
    void Start()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        uiViewStateManager = FindObjectOfType<UIViewStateManager>();
        towerContainer = FindObjectOfType<TowerContainer>();
        MaxEnergy = 100;
        CurrentEnergy = 0;
        SetEnergyText();

        uiViewStateManager.UpdateUiBeingViewedEvent += UpdateConstructionUI;
    }

    private void OnDestroy()
    {
        uiViewStateManager.UpdateUiBeingViewedEvent -= UpdateConstructionUI;
    }

    // Update is called once per frame
    void Update()
    {
        if(isInConstructionMode)
        {
            EnterConstruction();
        }else
        {
            ExitConstruction();
        }
    }



    private void UpdateConstructionUI(object sender, UIBeingViewed ui)
    {
        isInConstructionMode = ui == UIBeingViewed.Construction;
    }

    void EnterConstruction()
    {
        ConstructionUI.SetActive(true);             // display the construction UI
        coreArchitecture.OpenConstructionMode();
        

    }

    void ExitConstruction()
    {
        
        ConstructionUI.SetActive(false);                // hide the construction UI
      // hide the image in under the cursor
        coreArchitecture.CloseConstructionMode();
        
    }
    // Update UI energy text
    void SetEnergyText()
    {
        TextMeshProUGUI energyText_text = EnergyText.GetComponent<TextMeshProUGUI>();
        energyText_text.SetText("Energy: {0}/{1}",CurrentEnergy, MaxEnergy);
    }

    public bool CheckEnergyAvailableForConstruction(int cost)
    {
        if((CurrentEnergy+cost) > MaxEnergy)
            return false;
        return true;
    }

    public void EnergyConsumption(int cost)
    {
        CurrentEnergy += cost;
        SetEnergyText();
    }


    public void SetConstructionMode(bool status)
    {
        isInConstructionMode = status;
    }
}

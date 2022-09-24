using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Press KeyCode.C to enter construction mode. Players can place different towers in this mode
public class ConstructionMode : MonoBehaviour
{
    private bool isInConstructionMode = false;
    private Constants.TowerType towerType = Constants.TowerType.noShadow;
    private GameObject ShadowObj;
    private CoreArchitecture coreArchitecture;
    
    [SerializeField] List<GameObject> Towers;
    [SerializeField] GameObject ConstructionUI;
    [SerializeField] GameObject EnergyText;
    
    int MaxEnergy;
    int CurrentEnergy;

    // Start is called before the first frame update
    void Start()
    {
        coreArchitecture = FindObjectOfType<CoreArchitecture>();
        MaxEnergy = 100;
        CurrentEnergy = 0;
        SetEnergyText();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.C))
        {
            isInConstructionMode = !isInConstructionMode;
        }

        if(isInConstructionMode)
        {
            EnterConstruction();
        }else
        {
            ExitConstruction();
        }
    }

    void EnterConstruction()
    {
        ConstructionUI.SetActive(true);             // display the construction UI
        coreArchitecture.OpenConstructionMode();
        UpdateTowerType();

        if(!ShadowObj)
        {
            ShadowObj = new GameObject();
        }
        
        GeneratingConstructionShadow();

    }

    // Generating current tower shadow under player's mouse position
    void GeneratingConstructionShadow()
    {
        if(towerType != Constants.TowerType.noShadow){
            Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);        // get mouse position
            RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector2.down, 100.0f, 1 << Constants.Layer.GROUND);     // eject a downside ray
            bool regenerate = false;    // mark if the shadow object need to be regenerated
            if(ShadowObj){
                if(ShadowObj.name != Towers[(int)towerType].name + "(Clone)")
                {
                    regenerate = true;
                }
            }
            if(!ShadowObj || regenerate)
            {
                if(ShadowObj){
                    Destroy(ShadowObj);
                }
                ShadowObj = Instantiate(Towers[(int)towerType]);
                ShadowObj.GetComponent<ConstructionShadows>().StartUp(5.0f);
            }

            if(ShadowObj && downRay){
                // set shadow object's position
                ShadowObj.transform.position = downRay.point;
                ShadowObj.transform.position += new Vector3(0, ShadowObj.GetComponent<BoxCollider2D>().bounds.size.y/2 + 0.03f, downRay.transform.position.z);
                // align object's X position
                ShadowObj.transform.position = new Vector3(downRay.transform.position.x, ShadowObj.transform.position.y, ShadowObj.transform.position.z);
                //ShadowObj.transform.position = new Vector3(0,0,0);
                ConstructionShadows shadowScript = ShadowObj.GetComponent<ConstructionShadows>();
                if(Input.GetMouseButtonDown(0) && shadowScript.GetPlaceStatus() && coreArchitecture.IsPlayerInControlRange()){
                    if(CheckEnergyAvailableForConstruction())
                    {
                        shadowScript.PlaceTower();
                        EnergyConsumption();
                    }
                    else{
                        print("You are run out of power");
                    }
                }
            }
        }
    }



    void ExitConstruction()
    {
        
        ConstructionUI.SetActive(false);                // hide the construction UI
        towerType = Constants.TowerType.noShadow;       // hide the image in under the cursor
        coreArchitecture.CloseConstructionMode();

        Destroy(ShadowObj);
        
    }
    // Update UI energy text
    void SetEnergyText()
    {
        TextMeshProUGUI energyText_text = EnergyText.GetComponent<TextMeshProUGUI>();
        energyText_text.SetText("Energy: {0}/{1}",CurrentEnergy, MaxEnergy);
    }

    bool CheckEnergyAvailableForConstruction()
    {
        int Energy_To_Be_Cost = 0;
        switch(ShadowObj.name)
        {
            case "CatapultShadow(Clone)":
            Energy_To_Be_Cost = 20;
            break;
            case "ArcherTowerShadow(Clone)":
            Energy_To_Be_Cost = 10;
            break;
            case "TrapTowerShadow(Clone)":
            Energy_To_Be_Cost = 5;
            break;
            case "StoneWallShadow(Clone)":
            Energy_To_Be_Cost = 0;
            break;
            case "WoodenWallShadow(Clone)":
            Energy_To_Be_Cost = 0;
            break;
            default:
            Debug.LogError("No such type of construction");
            break;
        }
        if((CurrentEnergy+Energy_To_Be_Cost) > MaxEnergy)
            return false;
        return true;
    }

    void EnergyConsumption()
    {
        int cost_energy = 0;
        switch(ShadowObj.name)
        {
            case "CatapultShadow(Clone)":
            cost_energy = 20;
            break;
            case "ArcherTowerShadow(Clone)":
            cost_energy = 10;
            break;
            case "TrapTowerShadow(Clone)":
            cost_energy = 5;
            break;
            case "StoneWallShadow(Clone)":
            cost_energy = 0;
            break;
            case "WoodenWallShadow(Clone)":
            cost_energy = 0;
            break;
            default:
            Debug.LogError("No such type of construction");
            break;
        }
        CurrentEnergy += cost_energy;
        SetEnergyText();
    }

    void UpdateTowerType()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            towerType = Constants.TowerType.TowerCatapult;
        }
        if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            towerType = Constants.TowerType.TowerArcher;
        }
        if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            towerType = Constants.TowerType.TowerTrap;
        }
        if(Input.GetKeyUp(KeyCode.Alpha4))
        {
            towerType = Constants.TowerType.WoodenWall;
        }
        if(Input.GetKeyUp(KeyCode.Alpha5))
        {
            towerType = Constants.TowerType.StoneWall;
        }
    }

    public void SetConstructionMode(bool status)
    {
        isInConstructionMode = status;
    }
}

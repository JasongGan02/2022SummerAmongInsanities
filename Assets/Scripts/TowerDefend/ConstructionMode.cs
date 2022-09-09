using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 玩家按C键进入建筑模式，这个方法负责建筑模式的交互
public class ConstructionMode : MonoBehaviour
{
    private bool isInConstructionMode = false;
    private Constants.TowerType towerType = Constants.TowerType.noShadow;
    private GameObject shadowObj;
    
    [SerializeField] List<GameObject> Towers;
    [SerializeField] GameObject ConstructionUI;
    [SerializeField] GameObject EnergyText;
    
    int max_energy;
    int current_energy;

    // Start is called before the first frame update
    void Start()
    {
        max_energy = 100;
        current_energy = 0;
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
        // display the construction UI
        ConstructionUI.SetActive(true);
        // show current tower image at cursor position
        ShowConstructionUnderCursor();

        if(!shadowObj)
        {
            shadowObj = new GameObject();
        }
        
        
        if(towerType != Constants.TowerType.noShadow){
            Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);        // get mouse position
            RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector2.down, 100.0f, 1 << Constants.Layer.GROUND);     // eject a downside ray
            bool regenerate = false;    // mark if the shadow object need to be regenerated
            if(shadowObj){
                if(shadowObj.name != Towers[(int)towerType].name + "(Clone)")
                {
                    regenerate = true;
                }
            }
            if(!shadowObj || regenerate)
            {
                if(shadowObj){
                    Destroy(shadowObj);
                }
                shadowObj = Instantiate(Towers[(int)towerType]);
                shadowObj.GetComponent<ConstructionShadows>().StartUp(5.0f);
            }

            if(shadowObj && downRay){
                shadowObj.transform.position = downRay.point;
                shadowObj.transform.position += new Vector3(0, shadowObj.GetComponent<BoxCollider2D>().bounds.size.y/2 + 0.03f, downRay.transform.position.z);
                ConstructionShadows shadowScript = shadowObj.GetComponent<ConstructionShadows>();
                if(Input.GetMouseButtonDown(0) && shadowScript.GetPlaceStatus()){
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
        // hide the construction UI
        ConstructionUI.SetActive(false);
        // hide the image in under the cursor
        towerType = Constants.TowerType.noShadow;

        Destroy(shadowObj);
        

    }
    // Update UI energy text
    void SetEnergyText()
    {
        TextMeshProUGUI energyText_text = EnergyText.GetComponent<TextMeshProUGUI>();
        energyText_text.SetText("Energy: {0}/{1}",current_energy, max_energy);
    }

    bool CheckEnergyAvailableForConstruction()
    {
        int Energy_To_Be_Cost = 0;
        switch(shadowObj.name)
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
            default:
            Debug.LogError("No such type of construction");
            break;
        }
        if((current_energy+Energy_To_Be_Cost) > max_energy)
            return false;
        return true;
    }

    void EnergyConsumption()
    {
        int cost_energy = 0;
        switch(shadowObj.name)
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
            default:
            Debug.LogError("No such type of construction");
            break;
        }
        current_energy += cost_energy;
        SetEnergyText();
    }

    void ShowConstructionUnderCursor()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            towerType = Constants.TowerType.typeOne;
        }
        if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            towerType = Constants.TowerType.typeTwo;
        }
        if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            towerType = Constants.TowerType.typeThree;
        }
        if(Input.GetKeyUp(KeyCode.Alpha4))
        {
            towerType = Constants.TowerType.typeFour;
        }
    }
}

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
        
        /*
        if(!ShadowObj)
        {
            ShadowObj = new GameObject();
        }
        */
        //GeneratingConstructionShadow();

    }

    void ExitConstruction()
    {
        
        ConstructionUI.SetActive(false);                // hide the construction UI
        //_curTower = null;       // hide the image in under the cursor
        coreArchitecture.CloseConstructionMode();

        //Destroy(ShadowObj);
        
    }
    /*
    // Generating current tower shadow under player's mouse position
    void GeneratingConstructionShadow()
    {
        //Debug.Log(_curTower);
        if (_curTower != null)
        {
            
            Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector3.down, 100.0f, 1 << Constants.Layer.GROUND);
            bool regenerate = false;
            if (ShadowObj)
            {
                if (ShadowObj.name != _curTower.itemName)
                {
                    
                    regenerate = true;
                }
            }

            if (!ShadowObj || regenerate)
            {
                if (ShadowObj)
                {
                    Destroy(ShadowObj);
                }
                Debug.Log("213");
                ShadowObj = _curTower.GetShadowGameObject();
                ConstructionShadows shadowScript = ShadowObj.GetComponent<ConstructionShadows>();
                shadowScript.StartUp(5.0f);
                shadowScript.Start();
            }
            
            if (ShadowObj && downRay)
            {
                ShadowObj.transform.position = downRay.point;
                ShadowObj.transform.position += new Vector3(0, ShadowObj.GetComponent<BoxCollider2D>().bounds.size.y/2 + 0.03f, downRay.transform.position.z);
                // align object's X position
                ShadowObj.transform.position = new Vector3(downRay.transform.position.x, ShadowObj.transform.position.y, ShadowObj.transform.position.z);
                ConstructionShadows shadowScript = ShadowObj.GetComponent<ConstructionShadows>();
                //Debug.Log("1"+shadowScript.GetPlaceStatus());
                //Debug.Log("2"+coreArchitecture.IsPlayerInControlRange());
                //Debug.Log("3"+Input.GetMouseButtonDown(0));
                if (Input.GetMouseButtonDown(0) && shadowScript.GetPlaceStatus() && coreArchitecture.IsPlayerInControlRange())
                {
                    //Debug.Log("1");
                    if (CheckEnergyAvailableForConstruction())
                    {
                        //Debug.Log("2");
                        SpawnTower(ShadowObj.transform.position);
                        EnergyConsumption();
                    }
                    else
                    {
                        print("You are run out of power");
                    }
                }
            }
        }
    }
    
    
    void SpawnTower(Vector3 placePosition)
    {
        var curTowerObject = _curTower.GetSpawnedGameObject();
        curTowerObject.transform.parent = towerContainer.gameObject.transform;
        curTowerObject.transform.position = placePosition;
    }
    */
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

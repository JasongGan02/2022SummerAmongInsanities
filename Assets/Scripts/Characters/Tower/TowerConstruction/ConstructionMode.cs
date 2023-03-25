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
    private TowerObject _curTower;
    private GameObject ShadowObj;
    private CoreArchitecture coreArchitecture;
    private UIViewStateManager uiViewStateManager;
    private PlayerInteraction playerInteraction;
    private TowerContainer towerContainer;
    
    
    [SerializeField] List<GameObject> Towers;
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

    public TowerObject CurTower
    {
        get => _curTower;
        set => _curTower = value;
    }


    private void UpdateConstructionUI(object sender, UIBeingViewed ui)
    {
        isInConstructionMode = ui == UIBeingViewed.Construction;
    }

    void EnterConstruction()
    {
        ConstructionUI.SetActive(true);             // display the construction UI
        coreArchitecture.OpenConstructionMode();

        if(!ShadowObj)
        {
            ShadowObj = new GameObject();
        }
        
        GeneratingConstructionShadow();

    }

    void ExitConstruction()
    {
        
        ConstructionUI.SetActive(false);                // hide the construction UI
        towerType = Constants.TowerType.noShadow;       // hide the image in under the cursor
        coreArchitecture.CloseConstructionMode();

        Destroy(ShadowObj);
        
    }

    // Generating current tower shadow under player's mouse position
    void GeneratingConstructionShadow()
    {
        if(towerType != Constants.TowerType.noShadow || _curTower != null){
            Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);        // get mouse position
            RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector2.down, 100.0f, 1 << Constants.Layer.GROUND);     // eject a downside ray
            bool regenerate = false;    // mark if the shadow object need to be regenerated
            if(ShadowObj){
                if(ShadowObj.name != _curTower.itemName)
                {
                    regenerate = true;
                }
            }

            if(!ShadowObj || regenerate)
            {
                if(ShadowObj){
                    Destroy(ShadowObj);
                }
                //ShadowObj = Instantiate(Towers[(int)towerType]);
                ShadowObj = _curTower.GetShadowObject();
                ShadowObj.GetComponent<ConstructionShadows>().StartUp(5.0f);
            }

            if(ShadowObj && downRay){
                // set shadow object's position
                ShadowObj.transform.position = downRay.point;
                ShadowObj.transform.position += new Vector3(0, ShadowObj.GetComponent<BoxCollider2D>().bounds.size.y/2 + 0.03f, downRay.transform.position.z);
                // align object's X position
                ShadowObj.transform.position = new Vector3(downRay.transform.position.x, ShadowObj.transform.position.y, ShadowObj.transform.position.z);
                ConstructionShadows shadowScript = ShadowObj.GetComponent<ConstructionShadows>();
                if(Input.GetMouseButtonDown(0) && shadowScript.GetPlaceStatus() && coreArchitecture.IsPlayerInControlRange()){
                    if(CheckEnergyAvailableForConstruction())
                    {
                        SpawnTower(shadowScript.transform.position);
                        EnergyConsumption();
                    }
                    else{
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

    // Update UI energy text
    void SetEnergyText()
    {
        TextMeshProUGUI energyText_text = EnergyText.GetComponent<TextMeshProUGUI>();
        energyText_text.SetText("Energy: {0}/{1}",CurrentEnergy, MaxEnergy);
    }

    bool CheckEnergyAvailableForConstruction()
    {
        if((CurrentEnergy+_curTower.energyCost) > MaxEnergy)
            return false;
        return true;
    }

    void EnergyConsumption()
    {
        CurrentEnergy += _curTower.energyCost;
        SetEnergyText();
    }


    public void SetConstructionMode(bool status)
    {
        isInConstructionMode = status;
    }
}

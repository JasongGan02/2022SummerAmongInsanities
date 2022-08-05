using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionMode : MonoBehaviour
{
    private bool isInConstructionMode = false;
    private Constants.TowerType towerType = Constants.TowerType.noShadow;
    private GameObject shadowObj;
    
    [SerializeField] List<GameObject> Towers;
    [SerializeField] GameObject ConstructionUI;
    [SerializeField] Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        
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
        
        if(towerType != Constants.TowerType.noShadow){
            Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector2.down, 100.0f, 1 << Constants.Layer.GROUND);
            bool regenerate = false;
            if(shadowObj){
                char objIdx = shadowObj.name[shadowObj.name.Length-8];
                regenerate = (int)towerType != ((int)char.GetNumericValue(objIdx)-1);
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
                shadowObj.transform.position = downRay.transform.position;
                shadowObj.transform.position += new Vector3(0, shadowObj.GetComponent<BoxCollider2D>().bounds.size.y/2+ downRay.collider.bounds.size.y/2 + 0.03f, 0);
                ConstructionShadows shadowScript = shadowObj.GetComponent<ConstructionShadows>();
                if(Input.GetMouseButtonDown(0) && shadowScript.GetPlaceStatus()){
                    shadowScript.PlaceTower();
                }
            }
        }
    }

    void EnterConstruction()
    {
        // display the construction UI
        ConstructionUI.SetActive(true);
        // show current tower image at cursor position
        ShowConstructionUnderCursor();
        // when player hit left mouse button, check and place tower

    }

    void ExitConstruction()
    {
        // hide the construction UI
        ConstructionUI.SetActive(false);
        // hide the image in under the cursor
        towerType = Constants.TowerType.noShadow;

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

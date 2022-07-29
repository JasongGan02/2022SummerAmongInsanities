using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionMode : MonoBehaviour
{
    private bool isInConstructionMode = false;

    
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

    }

    void ShowConstructionUnderCursor()
    {
        Vector3 mousePositionInMinusTen = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePosition = new Vector3(mousePositionInMinusTen.x, mousePositionInMinusTen.y, 0);
        GeneratingTower(mousePosition);
        

    }
    
    void GeneratingTower(Vector3 generatingPosition)
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            Instantiate(Towers[0],generatingPosition,Quaternion.identity);
        }
        if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            Instantiate(Towers[1],generatingPosition,Quaternion.identity);
        }
        if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            Instantiate(Towers[2],generatingPosition,Quaternion.identity);
        }
        if(Input.GetKeyUp(KeyCode.Alpha4))
        {
            Instantiate(Towers[3],generatingPosition,Quaternion.identity);
        }
    }
}

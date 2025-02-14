using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Every tower will have a tower shadow, which will be displayed on Canvas UI
public class ConstructionShadows : MonoBehaviour
{
    private uint CollisionCount = 0;
    private float PlaceDistance = 0;
    private SpriteRenderer ShadowSpriteRenderer;
    
    private CoreArchitectureController coreArchitecture;
    private Color OriginalColor;
    private bool isPlaceAble = false;

    public void Start()
    {
        ShadowSpriteRenderer = GetComponent<SpriteRenderer>();
        OriginalColor = ShadowSpriteRenderer.color;
        coreArchitecture = FindObjectOfType<CoreArchitectureController>();

    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlaceStatus();
    }

    public void StartUp(float placeDistance){
        PlaceDistance = placeDistance;
    }
    
    public bool GetPlaceStatus(){
        return isPlaceAble;
    }

    // Check whether construction shadow is placeable
    private void UpdatePlaceStatus(){

        if(GameObject.FindWithTag("Player") == null || FindObjectOfType<CoreArchitectureController>() == null)
        {
            isPlaceAble = false;
        }
        else
        {
            float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);
            if(distance <= PlaceDistance && CollisionCount == 0 && IsConstructionShadowInRange()){
                ShadowSpriteRenderer.color = new Color(OriginalColor.r/2, OriginalColor.g, OriginalColor.b/2, OriginalColor.a);
                isPlaceAble = true;
            }else{
                ShadowSpriteRenderer.color = new Color(OriginalColor.r, OriginalColor.g/2, OriginalColor.b/2, OriginalColor.a);
                isPlaceAble = false;
            
            }
        }
        
    }

    // Check whether player mouse is in constructable range
    bool IsConstructionShadowInRange()
    {
        float Constructable_Distance = coreArchitecture.GetConstructableDistance();
        float Mouse_Distance = CalculateDistanceBetweenCoreAndMouse();
        if(Constructable_Distance>Mouse_Distance)
        {
            return true;
        }
        return false;
    }

    float CalculateDistanceBetweenCoreAndMouse()
    {
        Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);    // get mouse world poistion (x,y)
        Vector3 corePosition = coreArchitecture.GetComponent<Transform>().position;
        float distance = Mathf.Sqrt(Mathf.Pow((rayOrigin.x-corePosition.x) ,2) + Mathf.Pow((rayOrigin.y-corePosition.y) ,2));

        return distance;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject);
        ++CollisionCount;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        --CollisionCount;
    }
}

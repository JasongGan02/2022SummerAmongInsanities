using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Every tower will have a tower shadow, which will be displayed on Canvas UI
public class ConstructionShadows : MonoBehaviour
{
    [SerializeField]
    private GameObject TargetTower;      
    private uint CollisionCount = 0;
    private float PlaceDistance = 0;
    private SpriteRenderer ShadowSpriteRenderer;
    private TowerContainer towerContainer;
    private Color OriginalColor;
    private bool isPlaceAble = false;
    void Start()
    {
        ShadowSpriteRenderer = GetComponent<SpriteRenderer>();
        OriginalColor = ShadowSpriteRenderer.color;
        towerContainer = FindObjectOfType<TowerContainer>();
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

    public void PlaceTower(){
        var instance = Instantiate(TargetTower, transform.position, transform.rotation);
        instance.transform.parent = towerContainer.gameObject.transform;
    }

    private void UpdatePlaceStatus(){
        float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);
        if(distance <= PlaceDistance && CollisionCount == 0){
            ShadowSpriteRenderer.color = new Color(OriginalColor.r/2, OriginalColor.g, OriginalColor.b/2, OriginalColor.a);
            isPlaceAble = true;
        }else{
            ShadowSpriteRenderer.color = new Color(OriginalColor.r, OriginalColor.g/2, OriginalColor.b/2, OriginalColor.a);
            isPlaceAble = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ++CollisionCount;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        --CollisionCount;
    }
}

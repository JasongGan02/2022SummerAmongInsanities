using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Every tower will have a tower shadow, which will be displayed on Canvas UI
public class ConstructionShadows : MonoBehaviour
{
    private uint collisionCount = 0;
    private float placeDistance = 0;
    private SpriteRenderer shadowSpriteRenderer;

    private CoreArchitectureController coreArchitecture;
    private Color originalColor;
    private bool isPlaceAble = false;

    public void Start()
    {
        shadowSpriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = shadowSpriteRenderer.color;
        coreArchitecture = FindFirstObjectByType<CoreArchitectureController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlaceStatus();
    }

    public void StartUp(float placeDistance)
    {
        this.placeDistance = placeDistance;
    }

    public bool GetPlaceStatus()
    {
        return isPlaceAble;
    }

    // Check whether construction shadow is placeable
    private void UpdatePlaceStatus()
    {
        if (GameObject.FindWithTag("Player") == null || FindFirstObjectByType<CoreArchitectureController>() == null)
        {
            isPlaceAble = false;
        }
        else
        {
            float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);
            if (distance <= placeDistance && collisionCount == 0 && IsConstructionShadowInRange())
            {
                shadowSpriteRenderer.color = new Color(originalColor.r / 2, originalColor.g, originalColor.b / 2, originalColor.a);
                isPlaceAble = true;
            }
            else
            {
                shadowSpriteRenderer.color = new Color(originalColor.r, originalColor.g / 2, originalColor.b / 2, originalColor.a);
                isPlaceAble = false;
            }
        }
    }

    // Check whether player mouse is in constructable range
    bool IsConstructionShadowInRange()
    {
        float Constructable_Distance = coreArchitecture.GetConstructableDistance();
        float Mouse_Distance = CalculateDistanceBetweenCoreAndMouse();
        if (Constructable_Distance > Mouse_Distance)
        {
            return true;
        }

        return false;
    }

    float CalculateDistanceBetweenCoreAndMouse()
    {
        Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition); // get mouse world poistion (x,y)
        Vector3 corePosition = coreArchitecture.GetComponent<Transform>().position;
        float distance = Mathf.Sqrt(Mathf.Pow((rayOrigin.x - corePosition.x), 2) + Mathf.Pow((rayOrigin.y - corePosition.y), 2));

        return distance;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject);
        ++collisionCount;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        --collisionCount;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShadowObjectController : MonoBehaviour
{
    IShadowObject curShadow; 
    private uint CollisionCount = 0;
    private Dictionary<Vector2Int, GameObject> _worldTilesDictionary = null;
    private Dictionary<Vector2Int, GameObject> worldTilesDictionary
    {
        get
        {
            if (_worldTilesDictionary == null)
            {
                _worldTilesDictionary = TerrainGeneration.worldTilesDictionary;
            }
            return _worldTilesDictionary;
        }
    }
   
    
    /***
    Place Tile Implementations

    ***/


    private bool CheckAdjcentPos(Vector2Int tilePos)
    {
        // Check for adjacent tiles
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // skip the current tile position
                if (Math.Abs(i)==Math.Abs(j)) continue; 
                Vector2Int adjacentPos = tilePos + new Vector2Int(i, j);
                if (worldTilesDictionary.ContainsKey(adjacentPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    
    public TileGhostPlacementResult GetTileGhostPlacementResult(BaseObject objectType)
    {
        Vector2 mousePosition = GetMousePosition2D();
        float x = GetSnappedCoordinate(mousePosition.x);
        float y = GetSnappedCoordinate(mousePosition.y);
        if (objectType is TileObject) 
        {    
            return new TileGhostPlacementResult(new Vector2(x, y), TileObjectCheck(x, y));
        } 
        else if(objectType is TowerObject)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Rotate the object
                Debug.Log("1");
                //transform.rotation = Quaternion.Euler(0, transform.rotation.y-(objectType as TowerObject).SetDirection(),0);
                transform.rotation *= (objectType as TowerObject).SetDirection();
            }
            return TowerObjectCheck(objectType);
        }

        return null;
    }

    private bool TileObjectCheck(float x, float y)
    {
        return worldTilesDictionary.ContainsKey(new Vector2Int((int)(x * 4), (int)(y * 4))) == false && CheckAdjcentPos(new Vector2Int((int)(x * 4), (int)(y * 4)))  && CollisionCount==0;
    }

    private TileGhostPlacementResult TowerObjectCheck(BaseObject objectType)
    {
        Vector2 mousePosition = GetMousePosition2D();
        CoreArchitecture coreArchitecture = FindObjectOfType<CoreArchitecture>();
        ConstructionMode constructionMode = FindObjectOfType<ConstructionMode>();
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector3.down, 100.0f, 1 << Constants.Layer.GROUND);
        float x = GetSnappedCoordinate(mousePosition.x);
        float y = GetComponent<BoxCollider2D>().bounds.size.y/0.9f/2 + 0.03f + downRay.point.y;
        return new TileGhostPlacementResult(new Vector2(x, y), CollisionCount==0 && IsConstructionShadowInRange(coreArchitecture) && downRay && constructionMode.CheckEnergyAvailableForConstruction((objectType as TowerObject).energyCost));
    }

    bool IsConstructionShadowInRange(CoreArchitecture coreArchitecture)
    {
        float Constructable_Distance = coreArchitecture.GetConstructableDistance();
        float Mouse_Distance = CalculateDistanceBetweenCoreAndMouse(coreArchitecture);
        if(Constructable_Distance>Mouse_Distance)
        {
            return true;
        }
        return false;
    }

    float CalculateDistanceBetweenCoreAndMouse(CoreArchitecture coreArchitecture)
    {
        Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);    // get mouse world poistion (x,y)
        Vector3 corePosition = coreArchitecture.GetComponent<Transform>().position;
        float distance = Mathf.Sqrt(Mathf.Pow((rayOrigin.x-corePosition.x) ,2) + Mathf.Pow((rayOrigin.y-corePosition.y) ,2));

        return distance;
    }

    private Vector2 GetMousePosition2D()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePos.x, mousePos.y);
    }

    private float GetSnappedCoordinate(float number)
    {
        switch (number % 1)
        {
            case float res when (res >= 0.25 && res < 0.5):
                return (int)number + 0.375f;
                
            case float res when (res >= 0.5 && res < 0.75):
                return (int)number + 0.625f;
                
            case float res when (res >= 0.75 && res < 1.0):
                return (int)number + 0.875f;
                
            case float res when (res >= 0.0 && res < 0.25):
                return (int)number + 0.125f;
            default:
                return -1; // impossible to get here
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CollisionCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //if(curShadow is not TileObject)
        CollisionCount--;

    }
}

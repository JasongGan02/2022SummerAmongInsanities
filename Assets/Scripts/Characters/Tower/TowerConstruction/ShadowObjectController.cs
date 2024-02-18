using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class ShadowObjectController : MonoBehaviour
{
    CoreArchitectureController coreArchitecture;
    IShadowObject curShadow; 
    private uint CollisionCount = 0;
    
   
    private void Awake()
    {
        coreArchitecture = CoreArchitectureController.Instance;
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
                Vector2Int chunkCoord = new Vector2Int(WorldGenerator.GetChunkCoordsFromPosition(adjacentPos), 0);
                if (WorldGenerator.WorldData.ContainsKey(chunkCoord))
                {
                    if (WorldGenerator.GetDataFromWorldPos(adjacentPos) > 0)
                    {
                        return true;
                    }
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
        transform.position = new Vector2(x, y);
        if (objectType is TileObject) 
        {    
            return new TileGhostPlacementResult(transform, TileObjectCheck(x, y));
        } 
        else if(objectType is TowerObject)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Rotate the object
                
                transform.rotation *= (objectType as TowerObject).rotateAngle;
            }
            return TowerObjectCheck(objectType);
        }
        else if(objectType is ChestObject) 
        {
            return ChestObjectCheck(objectType);
        }
        

        return null;
    }

    private bool TileObjectCheck(float x, float y)
    {
        Vector2Int worldPosition = new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
        return WorldGenerator.GetDataFromWorldPos(worldPosition) <=0 && CheckAdjcentPos(worldPosition)  && CollisionCount==0 && !IsConstructionShadowInRange(coreArchitecture);
    }

    private TileGhostPlacementResult TowerObjectCheck(BaseObject objectType)
    {
        Vector2 mousePosition = GetMousePosition2D();
        ConstructionMode constructionMode = FindObjectOfType<ConstructionMode>();
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector3.down, 100.0f, 1 << Constants.Layer.GROUND);
        float x = GetSnappedCoordinate(mousePosition.x);
        float y = GetComponent<BoxCollider2D>().bounds.size.y/0.9f/2 + 0.03f + downRay.point.y;
        transform.position = new Vector2(x, y);
        return new TileGhostPlacementResult(transform, CollisionCount==0 && IsConstructionShadowInRange(coreArchitecture) && downRay && constructionMode.CheckEnergyAvailableForConstruction((objectType as TowerObject).energyCost));
    }

    private TileGhostPlacementResult ChestObjectCheck(BaseObject objectType)
    {
        Vector2 mousePosition = GetMousePosition2D();
        ConstructionMode constructionMode = FindObjectOfType<ConstructionMode>();
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector3.down, 100.0f, 1 << Constants.Layer.GROUND);
        float x = GetSnappedCoordinate(mousePosition.x);
        float y = GetComponent<BoxCollider2D>().bounds.size.y / 0.9f / 2 + 0.03f + downRay.point.y;
        transform.position = new Vector2(x, y);
        return new TileGhostPlacementResult(transform, CollisionCount == 0 && IsConstructionShadowInRange(coreArchitecture) && downRay);
    }



    bool IsConstructionShadowInRange(CoreArchitectureController coreArchitecture)
    {
        float Constructable_Distance = coreArchitecture.GetConstructableDistance();
        float Mouse_Distance = CalculateDistanceBetweenCoreAndMouse(coreArchitecture);
        if(Constructable_Distance>Mouse_Distance)
        {
            return true;
        }
        return false;
    }

    float CalculateDistanceBetweenCoreAndMouse(CoreArchitectureController coreArchitecture)
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
        return Mathf.Floor(number) + 0.5f;
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

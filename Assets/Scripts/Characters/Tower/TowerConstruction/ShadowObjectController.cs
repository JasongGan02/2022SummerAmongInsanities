using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class ShadowObjectController : MonoBehaviour
{
    private CoreArchitectureController coreArchitecture;
    private uint collisionCount = 0;
    
   
    private void Awake()
    {
        coreArchitecture = CoreArchitectureController.Instance;
    }
    
    public TileGhostPlacementResult GetTileGhostPlacementResult(BaseObject objectType)
    {
        Vector2 mousePosition = GetMousePosition2D();
        float x = GetSnappedCoordinate(mousePosition.x);
        float y = GetSnappedCoordinate(mousePosition.y);
        transform.position = new Vector2(x, y);

        if (objectType is TileObject tileObject)
        {
            return HandleTileObjectPlacement(tileObject);
        }
        else if (objectType is TowerObject towerObject)
        {
            return HandleTowerObjectPlacement(towerObject);
        }
        else if (objectType is ChestObject chestObject)
        {
            return HandleChestObjectPlacement(chestObject);
        }

        return null;
    }
    
    private TileGhostPlacementResult HandleTileObjectPlacement(TileObject tileObject)
    {
        bool isPlacementValid = IsBasePlacementValid() && IsTilePlacementValid(tileObject);
        return new TileGhostPlacementResult(transform, isPlacementValid);
    }

    private TileGhostPlacementResult HandleTowerObjectPlacement(TowerObject towerObject)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateObject(((TowerStats)towerObject.baseStats).rotateAngle);
        }
        
        bool isPlacementValid = IsBasePlacementValid() && IsTowerPlacementValid(towerObject);
        return new TileGhostPlacementResult(transform, isPlacementValid);
    }

    private TileGhostPlacementResult HandleChestObjectPlacement(ChestObject chestObject)
    {
        bool isPlacementValid = IsBasePlacementValid() && IsChestPlacementValid(chestObject);
        return new TileGhostPlacementResult(transform, isPlacementValid);
    }

    private bool IsBasePlacementValid()
    {
        Vector2Int worldPosition = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D downRay = Physics2D.Raycast(rayOrigin, Vector3.down, 100.0f, 1 << Constants.Layer.GROUND);
        return WorldGenerator.IsCurTileEmpty(worldPosition) && collisionCount == 0 && downRay;
    }

    private bool IsTilePlacementValid(TileObject tileObject)
    {
        Vector2Int worldPosition = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        return CheckAdjacentPos(worldPosition) && !IsConstructionShadowInRange(coreArchitecture);
    }

    private bool IsTowerPlacementValid(TowerObject towerObject)
    {
        return IsConstructionShadowInRange(coreArchitecture) && CheckEnergyAvailable(((TowerStats)towerObject.baseStats).energyCost);
    }

    private bool IsChestPlacementValid(ChestObject chestObject)
    {
        return IsConstructionShadowInRange(coreArchitecture);
    }

    private void RotateObject(Quaternion rotation)
    {
        transform.rotation *= rotation;
    }

    private bool CheckEnergyAvailable(int energyCost)
    {
        ConstructionModeManager constructionModeManager = FindObjectOfType<ConstructionModeManager>();
        return constructionModeManager.CheckEnergyAvailableForConstruction(energyCost);
    }
    
    private bool CheckAdjacentPos(Vector2Int tilePos)
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
                    if (!WorldGenerator.IsCurTileEmpty(adjacentPos))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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
        collisionCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //if(curShadow is not TileObject)
        collisionCount--;

    }
}

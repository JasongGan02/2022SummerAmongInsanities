using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public class BreakableObjectController : MonoBehaviour
{
    public static bool IsGameRunning = true;
    private IBreakableObject tile;
    private float healthPoint;
    private TerrainGeneration terrainGeneration;
    [HideInInspector] public bool isPlacedByPlayer;
    private void Start()
    {
        IsGameRunning = true;
    }

    private void OnApplicationQuit()
    {
        IsGameRunning = false;
    }

    private void OnDestroy()
    {
        if (IsGameRunning)
        {
            Debug.Log("Object is being destroyed. Call Stack: " + System.Environment.StackTrace);
        }
    }
    public void Initialize(TileObject tile, int hp, bool isPlacedByPlayer)
    {
        healthPoint = hp;
        this.tile = tile;
        this.isPlacedByPlayer = isPlacedByPlayer;
    }

    public void OnClicked(float damage)
    {
        healthPoint -= damage;
        if (healthPoint <= 0)
        {
            Debug.Log("Destroy by Clicking");
            Destroy(gameObject);
            OnObjectDestroyed();
        }
    }
  
    private void OnObjectDestroyed()
    {
        var drops = tile.GetDroppedGameObjects(isPlacedByPlayer);
        
        Vector2Int coord = new( (int) (transform.localPosition.x), (int) (transform.localPosition.y));
        if (TerrainGeneration.worldTilesDictionary.ContainsKey(coord))
        {
            // Remove the tile entry from the dictionary
            Debug.Log(TerrainGeneration.worldTilesDictionary.Remove(coord));
        }
        if (((IGenerationObject)tile).NeedsBackground)
        {
            terrainGeneration = FindObjectOfType<TerrainGeneration>();
            terrainGeneration.PlaceWallTile(((IGenerationObject)tile), coord.x, coord.y);
        }
        foreach (GameObject droppedItem in drops)
        {
            droppedItem.transform.parent = gameObject.transform.parent;
            droppedItem.transform.position = gameObject.transform.position;
            droppedItem.GetComponent<Rigidbody2D>().AddTorque(10f);
        }
    }
}

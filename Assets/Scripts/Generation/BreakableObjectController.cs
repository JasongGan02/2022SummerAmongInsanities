using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public class BreakableObjectController : MonoBehaviour
{
    private IBreakableObject tile;
    private float healthPoint;
    private WorldGenerator terrainGeneration;

    private audioManager am;

    
    private void Awake()
    {
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
    }

    [HideInInspector] public bool isPlacedByPlayer;
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
        am.playWeaponAudio(am.tile_endbreak);
        var drops = tile.GetDroppedGameObjects(isPlacedByPlayer);
        Vector2Int worldPostion = new Vector2Int((int) transform.position.x, (int)transform.position.y);
        Vector2Int chunkCoord = new Vector2Int(WorldGenerator.GetChunkCoordsFromPosition(worldPostion), 0);
        if (WorldGenerator.WorldData.ContainsKey(chunkCoord))
        {

            // Remove the tile entry from the dictionary
            WorldGenerator.WorldData[chunkCoord][(int)transform.localPosition.x, (int)transform.localPosition.y] = 0;
        }
        if (((IGenerationObject)tile).NeedsBackground && !isPlacedByPlayer)
        {
            WorldGenerator.WorldData[chunkCoord][(int)transform.localPosition.x, (int)transform.localPosition.y] = -((TileObject)tile).TileID;
            WorldGenerator.PlaceTile(((TileObject)tile), worldPostion.x, worldPostion.y, chunkCoord, true, false);
        }
        foreach (GameObject droppedItem in drops)
        {
            droppedItem.transform.parent = gameObject.transform.parent;
            droppedItem.transform.position = gameObject.transform.position;

            // Apply random torque
            float randomTorque = Random.Range(-20f, 20f); // Adjust the range as needed
            droppedItem.GetComponent<Rigidbody2D>().AddTorque(randomTorque);
        }
    }
}

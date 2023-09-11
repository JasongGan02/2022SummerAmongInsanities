using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public class BreakableObjectController : MonoBehaviour
{
    private IBreakableObject tile;
    private float healthPoint;
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

        foreach (GameObject droppedItem in drops)
        {
            droppedItem.transform.parent = gameObject.transform.parent;
            droppedItem.transform.position = gameObject.transform.position;
            droppedItem.GetComponent<Rigidbody2D>().AddTorque(10f);
        }
    }
}

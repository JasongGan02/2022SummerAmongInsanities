using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BreakableObjectController : MonoBehaviour
{
    private TileObject tile;
    private int healthPoint;
    [HideInInspector] public bool isPlacedByPlayer;

    public void Initialize(TileObject tile, int hp, bool isPlacedByPlayer)
    {
        healthPoint = hp;
        this.tile = tile;
        this.isPlacedByPlayer = isPlacedByPlayer;
    }

    public void OnClicked()
    {
        healthPoint -= 1;
        if (healthPoint <= 0)
        {
            Destroy(gameObject);
            if (!isPlacedByPlayer)
            {
                OnObjectDestroyed();
            }   
        }
    }

    private void OnObjectDestroyed()
    {
        var drops = tile.GetDroppedGameObjectsWhenBroken();
        foreach (GameObject droppedItem in drops)
        {
            droppedItem.transform.parent = gameObject.transform.parent;
            droppedItem.transform.position = gameObject.transform.position;
            droppedItem.GetComponent<Rigidbody2D>().AddTorque(10f);
        }
    }
}

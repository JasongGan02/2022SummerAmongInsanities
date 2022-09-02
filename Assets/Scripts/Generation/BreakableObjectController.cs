using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BreakableObjectController : MonoBehaviour
{
    public TileClass tile;
    private int healthPoint;

    private void Awake()
    {
        healthPoint = tile.healthPoint;
    }

    public void OnClicked()
    {
        healthPoint -= 1;
        if (healthPoint <= 0)
        {
            OnObjectDestroyed();
            Destroy(this.gameObject);
        }
    }

    private void OnObjectDestroyed()
    {
        if (tile.drops.Length > 0)
        {
            foreach (TileClass.Drop droppedItem in tile.drops)
            {
                int count = droppedItem.GetDroppedItemCount();
                for (int i = 0; i < count; i++)
                {
                    GameObject droppedGameObject = Instantiate(droppedItem.droppedItem.droppedItem);
                    droppedGameObject.transform.parent = this.gameObject.transform.parent;
                    droppedGameObject.transform.position = this.gameObject.transform.position;
                    float sizeRatio = droppedItem.droppedItem.sizeRatio;
                    droppedGameObject.transform.localScale = new Vector2(sizeRatio, sizeRatio);
                    droppedGameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 50);
                }
            }
            
        }
    }
}

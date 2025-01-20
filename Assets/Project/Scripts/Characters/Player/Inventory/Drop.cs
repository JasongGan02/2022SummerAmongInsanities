using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Drop
{
    public BaseObject droppedItem;
    public float chance; // 3.4 means you will certianly get 3 items and 40% chance to get 1 more.

    private int GetDroppedItemCount()
    {
        int count = (int)chance;
        if (!Mathf.Approximately(chance - count, 0) && Random.value > chance - count)
        {
            count += 1;
        }
        return count;
    }

    public GameObject GetDroppedItem(Vector3 dropPosition)
    {
        if (droppedItem is IInventoryObject inventoryObject)
        {
            return inventoryObject.GetDroppedGameObject(GetDroppedItemCount(), dropPosition);
        }
        
        if (droppedItem is AshObject ashObject)
        {
            return ashObject.GetDroppedGameObject(GetDroppedItemCount(), dropPosition);
        }

        return null;
    }
}


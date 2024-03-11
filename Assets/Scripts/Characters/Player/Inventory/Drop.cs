using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Drop
{
    public ScriptableObject droppedItem;
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
        return (droppedItem as IInventoryObject).GetDroppedGameObject(GetDroppedItemCount(), dropPosition);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryObject
{
    int MaxStack { get; set; }

    public string GetItemName()
    {
        var baseObject = this as BaseObject;
        return baseObject == null ? null : baseObject.itemName;
    }

    public Sprite GetSpriteForInventory();

    public GameObject GetDroppedGameObject(int amount);
}

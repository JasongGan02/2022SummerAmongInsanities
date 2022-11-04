using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentObject : BaseObject, IInventoryObject
{
    public int level;
    public Color quality;
    public int durability;

    private int _maxStack;
    public int MaxStack { 
        get => _maxStack; 
        set => _maxStack = value; 
    }

    public GameObject GetDroppedGameObject(int amount)
    {
        GameObject drop = Instantiate(prefab);
        drop.layer = Constants.Layer.RESOURCE;
        drop.AddComponent<Rigidbody2D>();
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);

        return drop;
    }

    public Sprite GetSpriteForInventory()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentObject : BaseObject, IInventoryObject
{
    public int level;
    public Color quality;
    public int durability;

    [SerializeField]
    private int _maxStack;
    public int MaxStack { 
        get => _maxStack; 
        set => _maxStack = value; 
    }

    public GameObject GetDroppedGameObject(int amount, Vector3 dropPosition)
    {
        GameObject drop = Instantiate(prefab);
        drop.layer = Constants.Layer.RESOURCE;
        if (drop.GetComponent<Rigidbody2D>() == null)
        {
            drop.AddComponent<Rigidbody2D>();
        }
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
        drop.transform.position = dropPosition;
        return drop;
    }

    public Sprite GetSpriteForInventory()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }
}

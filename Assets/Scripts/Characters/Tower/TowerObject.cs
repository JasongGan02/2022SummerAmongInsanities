using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object")]
public class TowerObject : CharacterObject, IInventoryObject
{
    public int energyCost;

    [Header("Bullet Specification")]
    public float bullet_speed;
    public GameObject bullet;

    [SerializeField]
    private int _maxStack;

    /**
     * implementation of IInventoryObject
     */
    #region
    public int MaxStack
    {
        get => _maxStack;
        set => _maxStack = value;
    }

    public Sprite GetSpriteForInventory()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }

    public GameObject GetDroppedGameObject(int amount)
    {
        GameObject drop = Instantiate(prefab);
        drop.layer = Constants.Layer.RESOURCE;
        if (drop.GetComponent<Rigidbody2D>() == null)
        {
            drop.AddComponent<Rigidbody2D>();
        }
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);

        return drop;
    }
    #endregion

    public override List<GameObject> GetDroppedGameObjects(bool isUserPlaced) //need to consider the case if it is placed by User.
    {
        List<GameObject> droppedItems = new();
        if (isUserPlaced)
        {
            droppedItems.Add(GetDroppedGameObject(1)); //drop itself
        }
        else
        {
            foreach (Drop drop in Drops)
            {
                GameObject droppedGameObject = drop.GetDroppedItem(); //drop some of original materials
                droppedItems.Add(droppedGameObject);
            }   
        }
        return droppedItems;

    }
    /**
     * implementation of ConstructionMode
     */
    public virtual GameObject GetShadowObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        SpriteRenderer spriteRenderer = worldGameObject.GetComponent<SpriteRenderer>(); // Get the sprite renderer component
        Color spriteColor = spriteRenderer.color; // Get the current color of the sprite
        spriteColor.a = 100 / 255f; // Set the alpha value to 100 (out of 255)
        spriteRenderer.color = spriteColor; // Assign the new color back to the sprite renderer
        var controller = worldGameObject.AddComponent<ConstructionShadows>();
        return worldGameObject;
    }
    
    public override GameObject GetSpawnedGameObject() //Use this when you are unsure about what type of controller will be using.
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        controllerName = itemName+"Controller";
        Type type = Type.GetType(controllerName);
        var controller = worldGameObject.AddComponent(type);
        (controller as CharacterController).Initialize(this);
        controller.gameObject.transform.parent = GameObject.Find("TowerContainer").transform;
        return worldGameObject;
    }

  
    
}

using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Chest", menuName = "Objects/Chest Object")]
public class ChestObject : BaseObject, IInventoryObject, ICraftableObject, IShadowObject, IPoolableObject
{

    [SerializeField]
    private int _maxStack;

    [Header("Craft")]
    [SerializeField]
    private CraftRecipe[] _recipe;
    [SerializeField]
    private bool _isCraftable;
    [SerializeField]
    private bool _isCoreNeeded;
    [SerializeField]
    private bool _isLocked;
    [SerializeField]
    private int _craftTime;



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

    public GameObject GetDroppedGameObject(int amount, Vector3 dropPosition)
    {
        GameObject drop = Instantiate(prefab);
        drop.layer = Constants.Layer.RESOURCE;
        if (drop.GetComponent<Rigidbody2D>() == null)
        {
            drop.AddComponent<Rigidbody2D>();
        }
        else
        {
            Rigidbody2D rigidbody = drop.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            drop.GetComponent<Collider2D>().isTrigger = false;

        }
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        drop.transform.position = dropPosition;

        return drop;
    }
    #endregion


    /**
    * implementation of ICraftableObject
*/
    #region
    public CraftRecipe[] Recipe
    {
        get => _recipe;
        set => _recipe = value;
    }

    public void Craft(Inventory inventory)
    {
        inventory.CraftItems(Recipe, this);
    }

    public CraftRecipe[] GetRecipe()
    {
        return Recipe;
    }
    #endregion

    #region
    public bool IsCraftable
    {
        get => _isCraftable;
        set => _isCraftable = value;

    }

    public bool GetIsCraftable()
    {
        return IsCraftable;
    }

    public bool IsCoreNeeded
    {
        get => _isCoreNeeded;
        set => _isCoreNeeded = value;
    }

    public bool GetIsCoreNeeded()
    {
        return _isCoreNeeded;
    }
    public bool IsLocked
    {
        get => _isLocked;
        set => _isLocked = value;
    }
    public bool GetIsLocked()
    {
        return _isLocked;
    }
    public int CraftTime
    {
        get => _craftTime;
        set => _craftTime = value;
    }

    public int GetCraftTime()
    {
        return _craftTime;
    }
    #endregion


    public GameObject GetPoolGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        Type type = Type.GetType("ChestController");
        var controller = worldGameObject.AddComponent(type);
        worldGameObject.tag = "Chest";
        return worldGameObject;
    }
    public GameObject GetShadowGameObject()
    {
        var ghost = Instantiate(prefab);
        ghost.name = itemName;
        ghost.layer = Constants.Layer.DEFAULT;
        SpriteRenderer spriteRenderer = ghost.GetComponent<SpriteRenderer>(); // Get the sprite renderer component
        Color spriteColor = spriteRenderer.color; // Get the current color of the sprite
        spriteColor.a = 100 / 255f; // Set the alpha value to 100 (out of 255)
        spriteRenderer.color = spriteColor; // Assign the new color back to the sprite renderer
        var collider = ghost.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
        collider.size = new Vector2(collider.size.x*0.9f, collider.size.y*0.9f);
        ghost.AddComponent<ShadowObjectController>();
        var controller = ghost.GetComponent<Rigidbody2D>();
        controller.simulated = true;
        controller.isKinematic = true; //so that it does not fall and collide with everything

        return ghost;
    }



}

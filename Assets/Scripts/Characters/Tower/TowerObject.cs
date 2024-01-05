using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Tower Object")]
public class TowerObject : CharacterObject, IInventoryObject, IShadowObject, ICraftableObject, IPoolableObject
{   
    [Header("Construction Parameter")]
    public int energyCost;
    public Quaternion rotateAngle;//a fixed amount that determines the rotation type of a tower
    public Quaternion curAngle =  Quaternion.Euler(0, 0, 0);

    [Header("Bullet Specification")]
    [SerializeField] public ProjectileObject projectileObject;

    
    [Header("Iventory Parameters")]
    [SerializeField]
    private int _maxStack;

    [Header("Craft")]
    [SerializeField]
    private BaseObject[] _recipe;
    [SerializeField]
    private int[] _quantity;
    [SerializeField]
    private bool _isCraftable;
    [SerializeField]
    private bool _isCoreNeeded;
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

    public GameObject GetDroppedGameObject(int amount)
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
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);

        return drop;
    }
    #endregion

    public override GameObject GetPoolGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        if (itemName.Contains("Wall"))
        {
            controllerName = "TowerController";
        }
        else
        {
            controllerName = itemName + "Controller";
        }
        Type type = Type.GetType(controllerName);
        var controller = worldGameObject.AddComponent(type);
        (controller as CharacterController).Initialize(this);
        controller.gameObject.transform.parent = GameObject.Find("TowerContainer").transform;
        worldGameObject.transform.rotation = curAngle;
        curAngle = Quaternion.Euler(0, 0, 0);
        return worldGameObject;
    }

    public override List<GameObject> GetDroppedGameObjects(bool isDestroyedByPlayer) //need to consider the case if it is placed by User.
    {
        List<GameObject> droppedItems = new();
        if (isDestroyedByPlayer)
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
    public virtual GameObject GetShadowGameObject()
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
    
    public override GameObject GetSpawnedGameObject() //Use this when you are unsure about what type of controller will be using.
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        if(itemName.Contains("Wall"))
        {
            controllerName = "TowerController";
        }
        else
        {
            controllerName = itemName+"Controller";
        }
        Type type = Type.GetType(controllerName);
        var controller = worldGameObject.AddComponent(type);
        (controller as CharacterController).Initialize(this);
        controller.gameObject.transform.parent = GameObject.Find("TowerContainer").transform;
        worldGameObject.transform.rotation = curAngle;
        curAngle =  Quaternion.Euler(0,0,0);
        return worldGameObject;
    }

    public Quaternion SetDirection()
    {
        curAngle*=rotateAngle;

        return rotateAngle;
    }


    /**
 * implementation of ICraftableObject
 */
    #region
    public BaseObject[] Recipe
    {
        get => _recipe;
        set => _recipe = value;
    }

    public void Craft(Inventory inventory)
    {
        inventory.CraftItems(this.Recipe, this.Quantity, this);
    }

    public BaseObject[] getRecipe()
    {
        return Recipe;
    }
    #endregion

    #region
    public int[] Quantity
    {
        get => _quantity;
        set => _quantity = value;
    }

    public int[] getQuantity()
    {
        return Quantity;
    }

    public bool IsCraftable
    {
        get => _isCraftable;
        set => _isCraftable = value;

    }

    public bool getIsCraftable()
    {
        return IsCraftable;
    }

    public bool IsCoreNeeded
    { 
        get => _isCoreNeeded;
        set => _isCoreNeeded = value;
    }

    public bool getIsCoreNeeded()
    {
        return _isCoreNeeded;
    }
    public int CraftTime
    {
        get => _craftTime;
        set => _craftTime = value;
    }

    public int getCraftTime()
    {
        return _craftTime;
    }
    #endregion


}

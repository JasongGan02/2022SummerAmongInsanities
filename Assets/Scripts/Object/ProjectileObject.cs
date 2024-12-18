using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "projectile", menuName = "Objects/Projectile Object")]
public class ProjectileObject : BaseObject, IInventoryObject, ICraftableObject, IPoolableObject
{

    [SerializeField]
    private float damageCoef;

    public float DamageCoef
    {
        get => damageCoef;
    }
    //IventoryObject Implementation
    #region
    [SerializeField]
    private int _maxStack;
    public int MaxStack
    {
        get => _maxStack;
        set => _maxStack = value;
    }

    public GameObject GetDroppedGameObject(int amount, Vector3 dropPosition)
    {
        GameObject drop = PoolManager.Instance.Get(this);
        drop.layer = Constants.Layer.RESOURCE;

        drop.GetComponent<Collider2D>().isTrigger = false;
        // Disable the Projectile component if it exists
        Projectile projectileComponent = drop.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.enabled = false;
        }
        var controller = drop.GetComponent<DroppedObjectController>();
        if (controller == null)
        {
            controller = drop.AddComponent<DroppedObjectController>();

        }
        else
        {
            controller.enabled = true;
        }
        controller.Initialize(this, amount);
        drop.transform.position = dropPosition;

        return drop;
    }

    public Sprite GetSpriteForInventory()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }
    #endregion

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
     * implementation of ICraftableObject
     */
    #region
    public BaseObject[] Recipe
    {
        get => _recipe;
        set => _recipe = value;
    }

    public BaseObject[] getRecipe()
    {
        return Recipe;
    }
    public void Craft(Inventory inventory)
    {
        inventory.CraftItems(this.Recipe, this.Quantity, this);
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
    public GameObject GetPoolGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        if (worldGameObject.GetComponent<Projectile>() == null)
        {

            Type type = Type.GetType(itemName);
            worldGameObject.AddComponent(type);
        }
        var controller = worldGameObject.GetComponent<DroppedObjectController>();
        if (controller != null)
        {
            controller.enabled = false;

        }
        return worldGameObject;
    }
}

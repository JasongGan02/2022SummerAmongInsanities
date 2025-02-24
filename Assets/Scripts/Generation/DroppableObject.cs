using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "droppableobject", menuName = "Objects/Droppable Object")]
public class DroppableObject : BaseObject, IInventoryObject, ICraftableObject
{
    [SerializeField] private int maxStack;
    [SerializeField] private CraftRecipe[] recipe;
    [SerializeField] private bool isCraftable;
    [SerializeField] private bool isCoreNeeded;
    [SerializeField] private bool _isLocked;
    [SerializeField] private int craftTime;


    public bool IsCraftable
    {
        get => isCraftable;
        set => isCraftable = value;
    }

    public bool IsCoreNeeded
    {
        get => isCoreNeeded;
        set => isCoreNeeded = value;
    }
    public bool IsLocked
    {
        get => _isLocked;
        set => _isLocked = value;
    }
    public bool getIsLocked()
    {
        return _isLocked;
    }
    public int CraftTime
    {
        get => craftTime;
        set => craftTime = value;
    }
    

    public int getCraftTime()
    {
        return CraftTime;
    }


    public bool getIsCraftable()
    {
        return IsCraftable;
    }
    
    public bool getIsCoreNeeded()
    {
        return IsCoreNeeded;
    }
    /**
     * implementation of IInventoryObject
     */
    #region
    public int MaxStack
    {
        get => maxStack;
        set => maxStack = value;
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
        drop.transform.position = dropPosition;
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        return drop;
    }
    #endregion

    /**
  * implementation of ICraftableObject
  */
    #region
    public CraftRecipe[] Recipe
    {
        get => recipe;
        set => recipe = value;
    }

    public void Craft(Inventory inventory)
    {
        inventory.CraftItems(this.Recipe, this);
    }
 
    public CraftRecipe[] getRecipe()
    {
        return Recipe;
    }


    #endregion

}

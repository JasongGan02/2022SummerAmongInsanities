using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "droppableobject", menuName = "Objects/Droppable Object")]
public class DroppableObject : BaseObject, IInventoryObject, ICraftableObject
{
    [SerializeField] private int maxStack;
    [SerializeField] private BaseObject[] recipe;
    [SerializeField] private int[] quantity;
    [SerializeField] private bool isCraftable;
    [SerializeField] private bool isCoreNeeded;
    [SerializeField] private int craftTime;

    public int[] Quantity
    {
        get => quantity;
        set => quantity = value;
    }

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

    public int[] getQuantity()
    {
        return Quantity;
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

    /**
  * implementation of ICraftableObject
  */
    #region
    public BaseObject[] Recipe
    {
        get => recipe;
        set => recipe = value;
    }

    public void Craft(Inventory inventory)
    {
        inventory.CraftItems(this.Recipe, this.Quantity, this);
    }
    public void CoreCraft(Inventory inventory)
    {
        inventory.CraftItemsCore(this.Recipe, this.Quantity, this);
    }
    public BaseObject[] getRecipe()
    {
        return Recipe;
    }


    #endregion

}

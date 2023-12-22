
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Constants;

[CreateAssetMenu(fileName = "weapon", menuName = "Objects/Weapon Object")]
public class WeaponObject : EquipmentObject , ICraftableObject
{
    [Header("Weapon Stats")]
    [SerializeField]
    private float damageCoef;
    [SerializeField]
    private float farm;
    [SerializeField]
    private float frequency;


    [Header("Projectile Properties")] 
    public ProjectileObject projectileObject;

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

    public float getAttack()
    {
        return damageCoef;
    }


    public float getfarm()
    {
        return farm;
    }


    public float getfrequency()
    {
        return frequency;
    }


    public virtual GameObject GetSpawnedGameObject<T>() where T : MonoBehaviour //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.layer = LayerMask.NameToLayer("weapon");
        worldGameObject.tag = "weapon";
        worldGameObject.name = itemName;
        worldGameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var controller = worldGameObject.AddComponent<T>();
        return worldGameObject;
    }

    public virtual GameObject GetSpawnedGameObject(CharacterController character) //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.layer = LayerMask.NameToLayer("weapon");
        worldGameObject.tag = "weapon";
        worldGameObject.name = itemName;
        worldGameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        //worldGameObject.GetComponent<Collider2D>().isTrigger = true;
        worldGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        Type type = Type.GetType(itemName+"Controller");
        var controller = worldGameObject.AddComponent(type);
        (controller as Weapon).Initialize(this, character);
        return worldGameObject;
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


}

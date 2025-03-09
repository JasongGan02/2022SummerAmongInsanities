using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Constants;

[CreateAssetMenu(fileName = "weapon", menuName = "Objects/Weapon Object")]
public class WeaponObject : EquipmentObject, ICraftableObject
{
    [Header("Weapon Stats")]
    [SerializeField]
    private float damageCoef;

    [SerializeField]
    private float rangeCoef;

    [SerializeField]
    private float attackIntervalCoef;

    [SerializeField]
    private float baseDamage;

    [SerializeField]
    private float baseRange;

    [SerializeField]
    private float baseAttackInterval;

    [SerializeField]
    private float knockBack;


    public float DamageCoef
    {
        get => damageCoef;
    }

    public float RangeCoef
    {
        get => rangeCoef;
    }

    public float BaseDamage
    {
        get => baseDamage;
    }

    public float BaseRange
    {
        get => baseRange;
    }

    public float KnockBack
    {
        get => knockBack;
    }

    public float BaseAttackSpeed
    {
        get => baseAttackInterval;
    }

    public float AttackSpeedCoef
    {
        get => attackIntervalCoef;
    }


    [SerializeField]
    private float farm;
    [SerializeField]
    private float frequency;


    public List<EffectObject> onInitializeEffects = new List<EffectObject>();
    public List<EffectObject> onHitEffects = new List<EffectObject>();

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
        if (itemName != "Hand")
        {
            worldGameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
            worldGameObject.GetComponent<Collider2D>().isTrigger = true;
            worldGameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        }

        Type type = Type.GetType(itemName + "Controller");
        var controller = worldGameObject.AddComponent(type);
        //controller.transform.SetParent(character.transform, false);
        (controller as Weapon).Initialize(this, character);
        return worldGameObject;
    }

    /**
     * implementation of ICraftableObject
     */

    #region

    public CraftRecipe[] Recipe
    {
        get => _recipe;
        set => _recipe = value;
    }

    public CraftRecipe[] getRecipe()
    {
        return Recipe;
    }

    public void Craft(Inventory inventory)
    {
        inventory.CraftItems(Recipe, this);
    }

    #endregion

    #region

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
        get => _craftTime;
        set => _craftTime = value;
    }

    public int getCraftTime()
    {
        return _craftTime;
    }

    #endregion
}
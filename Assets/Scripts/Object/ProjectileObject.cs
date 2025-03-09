using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "projectile", menuName = "Objects/Projectile Object")]
public class ProjectileObject : BaseObject, IInventoryObject, ICraftableObject, IPoolableObject
{
    [SerializeField]
    private float damageCoef;
    public float DamageCoef => damageCoef;

    public List<EffectObject> onHitEffects = new List<EffectObject>();
    
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
        ProjectileController projectileControllerComponent = drop.GetComponent<ProjectileController>();
        if (projectileControllerComponent != null)
        {
            projectileControllerComponent.enabled = false;
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
    private CraftRecipe[] _recipe;
    [SerializeField]
    private bool _isCraftable;
    [SerializeField]
    private bool _isCoreNeeded;
    [SerializeField]
    private int _craftTime;
    [SerializeField]
    private bool _isLocked;

    // Dictionary to store effect-specific data (e.g., stacks per hit)
    public Dictionary<EffectObject, int> effectStacksPerHit = new Dictionary<EffectObject, int>();

    public void SetEffectStacks(EffectObject effect, int stacksPerHit)
    {
        if (!effectStacksPerHit.ContainsKey(effect))
        {
            effectStacksPerHit.Add(effect, stacksPerHit);
        }
        else
        {
            effectStacksPerHit[effect] = stacksPerHit;
        }
        Debug.Log("is effect exists " + effectStacksPerHit.ContainsKey(effect));
        Debug.Log("how many stacks " + effectStacksPerHit[effect]);
    }

    public int GetEffectStacks(EffectObject effect)
    {
        return effectStacksPerHit.GetValueOrDefault(effect, 0);
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
        inventory.CraftItems(this.Recipe, this);
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
    public GameObject GetPoolGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        if (worldGameObject.GetComponent<ProjectileController>() == null)
        {

           Debug.LogError("ProjectileController not attached at " + worldGameObject);
        }
        var controller = worldGameObject.GetComponent<DroppedObjectController>();
        if (controller != null)
        {
            controller.enabled = false;

        }
        return worldGameObject;
    }
}

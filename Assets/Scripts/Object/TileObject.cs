using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "tile", menuName = "Objects/Tile Object")]
public class TileObject : BaseObject, IInventoryObject, IBreakableObject, IGenerationObject, IShadowObject, ICraftableObject
{
    [Tooltip("0 = walls, 1 = entity blocks like tiles, 2 = accessories, 3 = accessories topmost (3 is ignored calculating light)")]
    [SerializeField]
    private int tileLayer = 1; // tile layer it is supposed to be at, for all tile objects the default should be 1 suggesting a entity object. 

    public int TileLayer
    {
        get => tileLayer;
        set => tileLayer = value;
    }

    [SerializeField]
    private int _maxStack;


    [SerializeField]
    private int _healthPoint;

    public int getHP()
    {
        return _healthPoint;
    }

    [SerializeField]
    private bool isBreakable = true;

    [SerializeField]
    private Drop[] _drops;


    [SerializeField]
    private GameObject[] _prefabs;

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

    [SerializeField]
    private bool _needsBackground; //when break the front tile, check this bool to see if a background wall is needed and sent to Terrain Generation to generate a background

    [SerializeField]
    private bool _isLit; // determine if this tile is a source of light

    [SerializeField]
    private bool _isDynamic; // determine if this tile is a source of dynamic light

    [Range(0f, 15f)] [SerializeField]
    private float _lightIntensity = 0;


    public GameObject GetPlacedGameObject(int spriteNumber, Quaternion rotation, bool flipX)
    {
        GameObject worldGameObject = GetGeneratedGameObjects(spriteNumber, rotation, flipX);
        if (isBreakable)
            worldGameObject.GetComponent<BreakableObjectController>().Initialize(this, HealthPoint, true);

        return worldGameObject;
    }

    public GameObject GetShadowGameObject()
    {
        var ghost = Instantiate(prefab);
        ghost.layer = Constants.Layer.DEFAULT;
        SpriteRenderer spriteRenderer = ghost.GetComponent<SpriteRenderer>(); // Get the sprite renderer component
        Color spriteColor = spriteRenderer.color; // Get the current color of the sprite
        spriteColor.a = 100 / 255f; // Set the alpha value to 100 (out of 255)
        spriteRenderer.color = spriteColor; // Assign the new color back to the sprite renderer
        spriteRenderer.sortingOrder = 10;
        var collider = ghost.GetComponent<BoxCollider2D>(); //TODO: not all tile is with box 2d or change all tile collider to box 2d
        if (collider != null)
        {
            collider.isTrigger = true;

            collider.size = new Vector2(collider.size.x * 0.5f, collider.size.y * 0.5f);
        }

        ghost.AddComponent<ShadowObjectController>();
        var controller = ghost.AddComponent<Rigidbody2D>();
        controller.simulated = true;
        controller.isKinematic = true; //so that it does not fall and collide with everything
        return ghost;
    }

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

        drop.transform.position = dropPosition;
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);

        return drop;
    }

    #endregion

    /**
     * implementation of IBreakableObject
     */

    #region

    public int HealthPoint
    {
        get => _healthPoint;
        set => _healthPoint = value;
    }

    public Drop[] Drops
    {
        get => _drops;
        set => _drops = value;
    }

    public List<GameObject> GetDroppedGameObjects(bool isUserPlaced, Vector3 dropPosition)
    {
        List<GameObject> droppedItems = new();
        if (isUserPlaced)
        {
            droppedItems.Add(GetDroppedGameObject(1, dropPosition));
        }
        else
        {
            foreach (Drop drop in Drops)
            {
                GameObject droppedGameObject = drop.GetDroppedItem(dropPosition);
                droppedItems.Add(droppedGameObject);
            }
        }

        return droppedItems;
    }

    #endregion

    /**
     * implementation of IGenerationObject
     */

    #region

    public GameObject[] Prefabs
    {
        get => _prefabs;
    }
    public bool NeedsBackground
    {
        get => _needsBackground;
    }

    public bool IsLit
    {
        get => _isLit;
    }

    public float LightIntensity
    {
        get => _lightIntensity;
    }


    public GameObject GetGeneratedGameObjects(int spriteNumber, Quaternion rotation, bool flipX)
    {
        GameObject worldGameObject;
        if (Prefabs.Length > 1)
        {
            worldGameObject = Instantiate(Prefabs[Random.Range(0, Prefabs.Length)]);
        }
        else
        {
            worldGameObject = Instantiate(prefab);
        }

        worldGameObject.name = itemName;

        if (isBreakable)
        {
            var controller = worldGameObject.AddComponent<BreakableObjectController>();
            controller.Initialize(this, HealthPoint, false);
        }

        if (_isDynamic)
        {
            var controller = worldGameObject.AddComponent<DynamicLightController>();
        }


        SpriteRenderer spriteRenderer = worldGameObject.GetComponent<SpriteRenderer>();
        if (HasSpecialFunctionality)
        {
            spriteRenderer.sprite = TargetSprite[spriteNumber];
            worldGameObject.transform.rotation = rotation;
            spriteRenderer.flipX = flipX;
        }

        spriteRenderer.sortingOrder = TileLayer;
        return worldGameObject;
    }

    public GameObject GetGeneratedWallGameObjects(int spriteNumber, Quaternion rotation, bool flipX)
    {
        GameObject worldGameObject;
        if (Prefabs.Length > 1)
        {
            worldGameObject = Instantiate(Prefabs[Random.Range(0, Prefabs.Length)]);
        }
        else
        {
            worldGameObject = Instantiate(prefab);
        }

        worldGameObject.name = itemName;
        worldGameObject.name += "_Wall";
        SpriteRenderer spriteRenderer = worldGameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        spriteRenderer.sortingOrder = 0;
        worldGameObject.layer = 2; //ignore raycast in general
        if (worldGameObject.GetComponent<Collider2D>() != null)
        {
            Destroy(worldGameObject.GetComponent<Collider2D>());
        }

        if (HasSpecialFunctionality && spriteNumber < 6)
        {
            spriteRenderer.sprite = TargetSprite[spriteNumber];
            worldGameObject.transform.rotation = rotation;
            spriteRenderer.flipX = flipX;
        }

        return worldGameObject;
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
        inventory.CraftItems(this.Recipe, this);
    }

    public CraftRecipe[] getRecipe()
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


    [SerializeField]
    public TileObject[] SpecifiedTiles;

    [SerializeField]
    public TileObject[] AnotherSpecifiedTiles;

    [SerializeField]
    public Sprite[] TargetSprite;

    [SerializeField]
    private bool hasSpecialFunctionality;

    public bool HasSpecialFunctionality
    {
        get => hasSpecialFunctionality;
        set => hasSpecialFunctionality = value;
    }

    [SerializeField]
    private bool isGrassTile;
    public bool IsGrassTile
    {
        get => isGrassTile;
        set => isGrassTile = value;
    }
}
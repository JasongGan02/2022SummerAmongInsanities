using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName ="tile", menuName = "Objects/Tile Object")]
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


    [SerializeField]
    private Drop[] _drops;


    [SerializeField]
    private GameObject[] _prefabs;

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
    
    [SerializeField]
    private bool _needsBackground; //when break the front tile, check this bool to see if a background wall is needed and sent to Terrain Generation to generate a background

    [SerializeField]
    private bool _isLit; // determine if this tile is a source of light

    [Range(0f, 15f)] [SerializeField] private float _lightIntensity = 0;

    public GameObject GetPlacedGameObject()
    {
        GameObject worldGameObject = GetGeneratedGameObjects();
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
        var collider = ghost.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
        collider.size = new Vector2(collider.size.x*0.5f, collider.size.y*0.5f);
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
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
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
    

    public GameObject GetGeneratedGameObjects()
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
        var controller = worldGameObject.AddComponent<BreakableObjectController>();
        controller.Initialize(this, HealthPoint, false);
        SpriteRenderer spriteRenderer = worldGameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = TileLayer;
        return worldGameObject;
    }
    public GameObject GetGeneratedWallGameObjects()
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
        return worldGameObject;
    }
    #endregion



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
        inventory.CraftItems(this.Recipe,this.Quantity,this);
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
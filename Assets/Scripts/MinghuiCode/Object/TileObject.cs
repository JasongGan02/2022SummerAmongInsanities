using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="tile", menuName = "Objects/Tile Object")]
public class TileObject : BaseObject, IInventoryObject, IBreakableObject, IGenerationObject
{
    [SerializeField]
    private int _maxStack;


    [SerializeField]
    private int _healthPoint;


    [SerializeField]
    private Drop[] _drops;


    [SerializeField]
    private GameObject[] _prefabs;

    public GameObject GetPlacedGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<BreakableObjectController>();
        controller.Initialize(this, HealthPoint, true);
        return worldGameObject;
    }

    public GameObject GetTileGhostBeforePlacement()
    {
        var ghost = Instantiate(prefab);
        ghost.layer = Constants.Layer.DEFAULT;
        ghost.transform.localScale = new Vector2(0.25f, 0.25f);
        SpriteRenderer spriteRenderer = ghost.GetComponent<SpriteRenderer>(); // Get the sprite renderer component
        Color spriteColor = spriteRenderer.color; // Get the current color of the sprite
        spriteColor.a = 100 / 255f; // Set the alpha value to 100 (out of 255)
        spriteRenderer.color = spriteColor; // Assign the new color back to the sprite renderer
        ghost.GetComponent<Collider2D>().enabled = false;
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

    public List<GameObject> GetDroppedGameObjects(bool isUserPlaced)
    {
        List<GameObject> droppedItems = new();
        if (isUserPlaced)
        {
            droppedItems.Add(GetDroppedGameObject(1));
        }
        else
        {
            foreach (Drop drop in Drops)
            {
                GameObject droppedGameObject = drop.GetDroppedItem();
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
        set => _prefabs = value;
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
        return worldGameObject;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="tile", menuName = "Objects/Tile Object")]
public class TileObject : BaseObject
{
    /** 
     * how many time you have to click onto the tile to break it
     */
    public int healthPoint;

    /**
     * It's an array because one object can have different designs when being generated, like trees
     */
    [SerializeField]
    protected GameObject[] prefabs;

    /*
     * An array of other objects that can be dropped by breaking this object
     */
    [SerializeField]
    protected Drop[] drops;

    /**
     * return a GameObject that can be placed into the world
     */
    private GameObject GetWorldGameObject(bool isPlacedByPlayer)
    {
        GameObject worldGameObject;
        if (prefabs.Length > 1)
        {
            worldGameObject = Instantiate(prefabs[Random.Range(0, prefabs.Length)]);
        }
        else
        {
            worldGameObject = Instantiate(prefab);
        }
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<BreakableObjectController>();
        controller.Initialize(this, healthPoint, isPlacedByPlayer);
        return worldGameObject;
    }

    public GameObject GetGeneratedWorldGameObject()
    {
        return GetWorldGameObject(false);
    }

    public GameObject GetPlayerPlacedWorldGameObject()
    {
        return GetWorldGameObject(true);
    }

    /**
     * return a list of objects that will be instantiated when this object is broken
     */
    public List<GameObject> GetDroppedGameObjectsWhenBroken()
    {
        List<GameObject> droppedItems = new();

        foreach (Drop drop in drops)
        {
            int count = drop.GetDroppedItemCount();
            GameObject droppedGameObject = drop.droppedItem.GetDroppedGameObject(count);
            droppedItems.Add(droppedGameObject);
        }

        return droppedItems;
    }

    public GameObject GetTileGhostBeforePlacement()
    {
        var ghost = Instantiate(prefab);
        ghost.layer = Constants.Layer.DEFAULT;
        ghost.transform.localScale = new Vector2(0.25f, 0.25f);
        ghost.GetComponent<Collider2D>().enabled = false;
        return ghost;
    }
}

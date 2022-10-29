using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

public class BaseObject : ScriptableObject
{
    public string itemName;
    public int maxStack = 99;
    public float sizeRatio = 1;

    /*
     * It has the default sprite and collider
     * It does not have any script
     */
    [SerializeField]
    protected GameObject prefab;

    /**
     * return the default prefab that will be instantiated when this object is dropped from inventory
     */
    public GameObject GetDroppedGameObject(int amount)
    {
        GameObject drop = Instantiate(prefab);
        drop.layer = Constants.Layer.RESOURCE;
        // TODO setup initial value for the component
        drop.AddComponent<Rigidbody2D>();
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
        var controller = drop.AddComponent<DroppedObjectController>();
        controller.Initialize(this, amount);
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);

        return drop;
    }

    /**
     * return the default sprite
     * The inventory will build a gameObject using this sprite
     */
    public Sprite GetDefaultSprite()
    {
        return prefab.GetComponent<SpriteRenderer>().sprite;
    }

    /**
     * set itemName to fileName
     */
    private void OnValidate()
    {
        itemName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
}


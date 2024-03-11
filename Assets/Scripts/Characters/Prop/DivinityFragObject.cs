using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DivinityFrag", menuName = "Objects/Divinity Frag Object")]
public class DivinityFragObject : BaseObject, IInventoryObject 
{
   [SerializeField]
    private int _maxStack;

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
        var controller = drop.AddComponent<DroppedObjectController>();
        drop.transform.position = dropPosition;
        drop.transform.localScale = new Vector2(sizeRatio, sizeRatio);
        controller.Initialize(this, amount);

        return drop;
    }
    #endregion

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Torch", menuName = "Objects/Torch Object")]
public class TorchObject : BaseObject, IInventoryObject
{
    [Header("Torch Stats")]

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


    public virtual GameObject GetSpawnedGameObject<T>() where T : MonoBehaviour //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.layer = LayerMask.NameToLayer("weapon"); // weapon layer is good for props or other hand-use items
        worldGameObject.tag = "prop";
        worldGameObject.name = itemName;
        worldGameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var controller = worldGameObject.AddComponent<T>();
        return worldGameObject;
    }

    public virtual GameObject GetSpawnedGameObject() //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.layer = LayerMask.NameToLayer("weapon");
        worldGameObject.tag = "prop";
        worldGameObject.name = itemName;
        Debug.Log("name: " + name);
        worldGameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        //worldGameObject.GetComponent<Collider2D>().isTrigger = true;
        worldGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        Type type = Type.GetType(itemName + "Controller");
        Debug.Log(type);
        var controller = worldGameObject.AddComponent(type);
        (controller as TorchController).Initialize(this);

        //var controllerType = typeof(MedicineController); // Assuming MedicineController is a MonoBehaviour script
        //var controller = worldGameObject.AddComponent(controllerType) as MedicineController;
        //controller.Initialize(this);
        return worldGameObject;
    }
}

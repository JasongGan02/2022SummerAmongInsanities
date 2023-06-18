
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Constants;

[CreateAssetMenu(fileName = "weapon", menuName = "Objects/Weapon Object")]
public class WeaponObject : EquipmentObject , ICraftableObject
{ 
    private float attack;
    private float farm;
    private float frequency;

    [SerializeField]
    private BaseObject[] _recipe;


    public float getAttack()
    {
        return attack;
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
        worldGameObject.name = itemName; 
        var controller = worldGameObject.AddComponent<T>();
        return worldGameObject;
    }

    public virtual GameObject GetSpawnedGameObject() //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.layer = LayerMask.NameToLayer("weapon");
        worldGameObject.tag = "weapon";
        worldGameObject.name = itemName;
        //worldGameObject.GetComponent<Collider2D>().isTrigger = true;
        worldGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        Type type = Type.GetType(itemName+"Controller");
        var controller = worldGameObject.AddComponent(type);
        (controller as Weapon).Initialize(this);
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

    public void Craft(Inventory inventory)
    {
        inventory.CraftItems(this.Recipe, this);
    }

    #endregion


}

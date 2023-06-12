
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Constants;

[CreateAssetMenu(fileName = "weapon", menuName = "Objects/Weapon Object")]
public class WeaponObject : EquipmentObject
{ 
    public float attack;
    public float farm;
    public float frequency;

    

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
}

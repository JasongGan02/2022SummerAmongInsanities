using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;


public class CharacterObject : BaseObject, IPoolableObject
{
    [Header("Character Stats")]
    public float _HP;
    public float _armor;
    public float _atkDamage;
    public float _atkSpeed;
    public float _atkRange;
    public float _criticalMultiplier;
    public float _criticalChance;
    public float _movingSpeed;
    public float _jumpForce;
    public int _totalJumps;

    public Drop[] Drops;
    public List<TextAsset> Hatred;
    protected string controllerName;
    
    public virtual GameObject GetSpawnedGameObject<T>()  where T : CharacterController //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<T>();
        controller.Initialize(this);
        return worldGameObject;
    }

    public virtual GameObject GetSpawnedGameObject() //Use this when you are unsure about what type of controller will be using.
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        controllerName = itemName+"Controller";
        Type type = Type.GetType(controllerName);
        var controller = worldGameObject.AddComponent(type);
        (controller as CharacterController).Initialize(this);
        return worldGameObject;
    }

    public virtual List<GameObject> GetDroppedGameObjects(bool isDestroyedByPlayer, Vector3 dropPosition)
    {
        List<GameObject> droppedItems = new();
        foreach (Drop drop in Drops)
        {
            GameObject droppedGameObject = drop.GetDroppedItem(dropPosition);
            droppedItems.Add(droppedGameObject);
        }   
        return droppedItems;

    }

    public virtual GameObject GetPoolGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        controllerName = itemName + "Controller";
        Type type = Type.GetType(controllerName);
        var controller = worldGameObject.AddComponent(type);
        //Debug.Log(controller.ToString());
        (controller as CharacterController).Initialize(this);
        return worldGameObject;
    }
}

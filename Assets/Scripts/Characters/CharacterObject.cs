using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;


public class CharacterObject : BaseObject
{
    [Header("Character Stats")]
    public float HP;
    public float AtkDamage;
    public float AtkInterval;
    public float AtkRange;
    public float MovingSpeed;
    public Drop[] Drops;

    private string controllerName;
    
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

    public virtual List<GameObject> GetDroppedGameObjects(bool isUserPlaced)
    {
        List<GameObject> droppedItems = new();
        foreach (Drop drop in Drops)
        {
            GameObject droppedGameObject = drop.GetDroppedItem();
            droppedItems.Add(droppedGameObject);
        }   
        
        return droppedItems;

    }

   
}

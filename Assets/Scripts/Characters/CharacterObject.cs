using UnityEngine;
using System;
using System.Collections.Generic;

public class CharacterObject : BaseObject, IPoolableObject
{
    [Header("Character Stats")]
    public CharacterStats baseStats;  // Initial template stats
    [HideInInspector] public CharacterStats maxStats;  // Runtime template stats
    public Drop[] drops;
    public List<TextAsset> hatred;
    
    protected string controllerName;
    
    protected virtual void OnEnable()
    {
        // Ensure runtimeTemplateStats is a copy of initialTemplateStats at the start
        if (baseStats == null) return;
        if (maxStats == null || maxStats.GetType() != baseStats.GetType())
        {
            maxStats = (CharacterStats)Activator.CreateInstance(baseStats.GetType());
        }
        maxStats.CopyFrom(baseStats);
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
        foreach (Drop drop in drops)
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
        var controller = worldGameObject.AddComponent(type) as CharacterController;
        controller?.Initialize(this);
        return worldGameObject;
    }
}

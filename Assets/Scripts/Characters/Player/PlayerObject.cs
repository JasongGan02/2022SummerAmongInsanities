using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Player Object")]

public class PlayerObject : CharacterObject
{
    public float RespwanTimeInterval;

    public override List<GameObject> GetDroppedGameObjects(bool playerDropItemsOnDeath)
    {
        List<GameObject> droppedItems = new();
        return droppedItems;
    }
    /*
    public override GameObject GetSpawnedGameObject() //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<PlayerController>();
        controller.Initialize(this);
        return worldGameObject;
    }
    */
}

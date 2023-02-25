using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;


public class CharacterObject : BaseObject
{
    public float HP;
    public float AtkDamage;
    public float AtkInterval;
    public float AtkRange;
    public float MovingSpeed;
    public Drop[] drops;

    public virtual GameObject GetSpawnedGameObject<T>()  where T : CharacterController //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<T>();
        controller.Initialize(this);
        return worldGameObject;
    }
}

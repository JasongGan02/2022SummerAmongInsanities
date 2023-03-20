using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "weapon", menuName = "Objects/Weapon Object")]
public class WeaponObject : EquipmentObject
{ 
    public float attack;
    public float farm;
    public float frequency;

    public virtual GameObject GetSpawnedGameObject<T>() where T : Weapon //Spawn the actual game object through calling this function. 
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<T>();
        return worldGameObject;
    }
}

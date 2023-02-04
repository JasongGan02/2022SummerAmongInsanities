using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Villager Object")]
public class VillagerObject : EnemyObject
{
    public override GameObject GetSpawnedGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<VillagerController>();
        controller.Initialize(this, HP, AtkDamage, AtkInterval, MovingSpeed, SensingRange);
        return worldGameObject;
    }
}

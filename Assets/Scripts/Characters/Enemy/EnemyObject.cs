using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyObject : CharacterObject
{
    public float SensingRange;

    public override GameObject GetSpawnedGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<EnemyController>();
        controller.Initialize(this, HP, AtkDamage, AtkInterval, MovingSpeed, SensingRange);
        return worldGameObject;
    }
}

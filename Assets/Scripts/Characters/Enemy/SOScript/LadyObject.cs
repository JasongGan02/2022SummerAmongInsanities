using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Lady Object")]
public class LadyObject : EnemyObject
{

    public override GameObject GetSpawnedGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<LadyController>();
        controller.Initialize(this, HP, AtkDamage, AtkInterval, MovingSpeed, AtkRange, SensingRange);
        return worldGameObject;
    }
}

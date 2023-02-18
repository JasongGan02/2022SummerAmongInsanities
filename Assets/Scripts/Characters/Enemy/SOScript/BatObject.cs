using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Character Objects/Bat Object")]
public class BatObject : EnemyObject
{

    public override GameObject GetSpawnedGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<BatController>();
        controller.Initialize(this, HP, AtkDamage, AtkInterval, MovingSpeed, AtkRange, SensingRange);
        return worldGameObject;
    }
}

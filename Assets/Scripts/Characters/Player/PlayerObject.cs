using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Player", menuName = "Objects/Player Object")]
public class PlayerObject : CharacterObject
{
    [SerializeField] 
    private float RespwanTimeInterval;

    public override GameObject GetSpawnedCharacter()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<PlayerObjectController>();
        controller.Initialize(this, HealthPoint, movementSpeed, atkDamage, armor, magicResist, atkSpeed);
        return worldGameObject;
    }

    public void LevelUp()
    {
        atkDamage+=10;
    }
}

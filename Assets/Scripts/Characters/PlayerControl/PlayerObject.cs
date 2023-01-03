using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Player", menuName = "Objects/Player Object")]
public class PlayerObject : CharacterObject
{
    [SerializeField] 
    private float RespwanTimeInterval;


    public PlayerObject(float hp)
    {
        this.curHealth = hp;
        Debug.Log(curHealth);
    }
    public GameObject GetPlacedGameObject()
    {
        GameObject worldGameObject = Instantiate(prefab);
        worldGameObject.name = itemName;
        var controller = worldGameObject.AddComponent<PlayerObjectController>();
        controller.Initialize(this, HealthPoint, movementSpeed, atkDamage, armor, magicResist, atkSpeed);
        return worldGameObject;
    }
}

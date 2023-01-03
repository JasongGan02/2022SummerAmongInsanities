using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectController : MonoBehaviour
{
    private float healthPoint;
    private IBreakableObject player;
    private float movementSpeed;
    private float atkDamage;
    private float armor;
    private float magicResist;
    private float atkSpeed; //每秒攻击几次

    public void Initialize(PlayerObject player, float hp, float mS, float ad, float ar, float mr, float aS)
    {
        healthPoint = hp;
        this.player = player;
        this.movementSpeed = mS;
        this.atkDamage = ad;
        this.armor = ar;
        this.magicResist = mr;
        this.atkSpeed = aS;
    }

    public void takenDamage(float damage)
    {
        healthPoint -= damage;
        if (healthPoint <= 0)
        {
            
            OnObjectDestroyed();
        }
    }

    private void OnObjectDestroyed()
    {
        var drops = player.GetDroppedGameObjects(false);
        foreach (GameObject droppedItem in drops)
        {
            droppedItem.transform.parent = gameObject.transform.parent;
            droppedItem.transform.position = gameObject.transform.position;
            droppedItem.GetComponent<Rigidbody2D>().AddTorque(10f);
        }
    }

    private void Death()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BowController : Weapon
{
    private GameObject spawnArrow;
    public float attackInterval = 0.5f; // the minimum time between attacks
    private float timeSinceLastAttack = 0f; // the time since the last attack
    

    

    public override void Update()
    {
        
        Flip();
        Patrol();

        if (Input.GetMouseButton(0))
        {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= attackInterval)
            {
                attack();
                timeSinceLastAttack = 0f;
            }
        }
       



    }

    public override void attack()
    {
        InventorySlot arrowSlot = inventory.findSlot("Arrow");
        WeaponObject arrow = inventory.findItem("Arrow") as WeaponObject;

        if (arrowSlot.count > 0)
        {

            if (playermovement.facingRight)
            {

                spawnArrow = arrow.GetSpawnedGameObject<Projectile>();
                spawnArrow.transform.position = transform.position + Vector3.right * 0.5f;
                arrowSlot.count--;
            }
            else
            {


                spawnArrow = arrow.GetSpawnedGameObject<Projectile>();
                spawnArrow.transform.position = transform.position - Vector3.right * 0.5f;
                arrowSlot.count--;
            }
        }
    }
    public override void Patrol()
    {
        if (playermovement.facingRight)
        {
            transform.position = player.transform.position + new Vector3(0.8f, 0, 0);
        }
        else
        {
            transform.position = player.transform.position - new Vector3(0.8f, 0, 0);
        }
    }

    public override void Flip()
    {
        if (playermovement.facingRight)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0,0,315));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0,135));
        }

    }

}

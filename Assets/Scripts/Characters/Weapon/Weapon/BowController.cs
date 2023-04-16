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

        int arrowCount = inventory.findItemCount("Arrow");
        WeaponObject arrow = inventory.findItem("Arrow") as WeaponObject;

        if (arrowCount > 0)
        {

            if (playermovement.facingRight)
            {

                spawnArrow = arrow.GetSpawnedGameObject<Projectile>();
                spawnArrow.transform.position = transform.position + Vector3.right * 0.5f;
                
            }
            else
            {


                spawnArrow = arrow.GetSpawnedGameObject<Projectile>();
                spawnArrow.transform.position = transform.position - Vector3.right * 0.5f;
                
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

}

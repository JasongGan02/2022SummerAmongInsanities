using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BowController : Weapon
{
    private WeaponObject arrow;

    public override void attack()
    {
        if (playermovement.facingRight)
        {
            
            transform.position = player.transform.position + Vector3.right * 0.8f;
            arrow.GetSpawnedGameObject<Projectile>();

        }   
        else
        {
            transform.position = player.transform.position - Vector3.right * 0.8f;
            arrow.GetSpawnedGameObject<Projectile>();

        }
    }


   


}

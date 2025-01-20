using System.Collections;
using UnityEngine;

// This class represents an archer tower
public class ArcherTowerController : RangedTowerController
{

    // Shoot a bullet at the nearest enemy
    protected override void Attack()
    {
        target = WhatToAttack();
        if (target != null)
        {
            audioEmitter.PlayClipFromCategory("ShootArrow");
            //Vector2 direction = (target.transform.position - transform.position).normalized;
            FireProjectiles(target);
            
        }
    }


}
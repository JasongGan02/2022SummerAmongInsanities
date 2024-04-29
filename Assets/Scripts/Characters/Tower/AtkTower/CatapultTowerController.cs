using System;
using TMPro;
using UnityEngine;

public class CatapultTowerController : RangedTowerController
{
    private Animator animator;
    

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start(); // Call the base start to initialize common features
        animator = GetComponent<Animator>();
        startPosition = transform.GetChild(0);
       
    }

    protected override void Attack()
    {
        
        target = WhatToAttack(); // Method from the base class to determine what to attack
        if (target != null && Math.Abs(target.transform.position.x - startPosition.position.x) > NON_DETECTABLE_RANGE)
        {
            _audioEmitter.PlayClipFromCategory("CatapultShoot");
            animator.Play("Catapult_Attack", -1, 0f); // Play the attack animation
            FireProjectile(target); // Fire a projectile at the target
        }
    }

    // This method flips the catapult sprite depending on the enemy's position
    private void FlipTowardsTarget()
    {
        if (target != null)
        {
            bool shouldFaceLeft = target.transform.position.x < transform.position.x;
            transform.rotation = shouldFaceLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        }
    }

    public override void FireProjectile(GameObject target)
    {
        FlipTowardsTarget(); // Flip the tower to face the target before firing
        base.FireProjectile(target); // Call the base method to handle projectile firing
    }

    const float NON_DETECTABLE_RANGE = 3f;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBowProjectile : Projectile
{
    public void Initialize(CharacterController firingCharacter, ProjectileObject projectileObject, float force, float damage)
    {
        base.Initialize(firingCharacter, projectileObject);
        this.speed = force;
        this.finalDamage = damage * (projectileObject?.DamageCoef ?? 1);
    }
    public void Launch(Vector2 startPosition)
    {
        // Set the projectile's velocity
        Vector2 launchDirection = ((Vector3)startPosition - transform.position ).normalized;
        rb.velocity = launchDirection * speed;
        
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsInHatredList(collider))
        {
            CharacterController character = collider.GetComponent<CharacterController>();
            if (character != null)
            {
                character.takenDamage(finalDamage);
            }

            // Return the projectile to the pool
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject, projectileObject.getPrefab());
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject, projectileObject.getPrefab());
        }
    }
}
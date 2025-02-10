using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBowProjectileController : ProjectileController
{
    float knockbackForce;
    public void Initialize(CharacterController firingCharacter, ProjectileObject projectileObject, float force, float damage,float knockbackForce)
    {
        base.Initialize(firingCharacter, projectileObject);
        this.speed = force;
        this.finalDamage = damage * (projectileObject?.DamageCoef ?? 1);
        this.knockbackForce = knockbackForce;
    }
  
    

    protected override void OnTriggerEnter2D(Collider2D collider)
    {

        if (IsInHatredList(collider))
        {
            CharacterController character = collider.GetComponent<CharacterController>();
            if (character != null)
            {
                ApplyDamage(character);
                KnockbackEnemy(collider);
            }

            // Return the projectile to the pool
            PoolManager.Instance.Return(gameObject, projectileObject);
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            PoolManager.Instance.Return(gameObject, projectileObject);
        }
    }

    protected void KnockbackEnemy(Collider2D enemy)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 knockbackDirection = (enemy.transform.position - firingCharacter.transform.position).normalized;
            enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
}

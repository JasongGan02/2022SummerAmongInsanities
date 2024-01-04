using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultProjectile : Projectile
{
    [SerializeField] private LayerMask damageableLayer; // Set this in the inspector
    [SerializeField] private float explosionRadius = 5.0f;


    public override void Launch(GameObject target, Transform startPosition)
    {
        if (target == null)
        {
            Debug.LogError("Launch target is null.");
            return;
        }

        // Calculate the distance to the target
        Vector2 targetPosition = target.transform.position;
        Vector2 firePosition = startPosition.position;

        float distanceToTarget = Vector2.Distance(targetPosition, firePosition);
        float direction = targetPosition.x > firePosition.x ? 1f : -1f;

        // Calculate the initial velocity required to reach the target in a parabolic path
        float velocityMagnitude = Mathf.Sqrt(distanceToTarget * Physics2D.gravity.magnitude * 3.5f / Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad));
        Vector2 velocity = new Vector2(velocityMagnitude * Mathf.Cos(firingAngle * Mathf.Deg2Rad) * direction, velocityMagnitude * Mathf.Sin(firingAngle * Mathf.Deg2Rad));

        // Apply the calculated velocity to the Rigidbody2D
        rb.velocity = velocity;
        rb.AddTorque(-5);
        rb.gravityScale = 3.5f;
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        // If the projectile hits the ground or a target, trigger explosion
        if (collider.gameObject.layer == LayerMask.NameToLayer("ground") || IsInHatredList(collider))
        {
            // Perform AoE damage
            Explode();

            // Return the projectile to the pool
            PoolManager.Instance.Return(gameObject, projectileObject);
        }
    }


    private void Explode()
    {
        // Find all colliders within the explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayer);
        foreach (var hitCollider in hitColliders)
        {
            // Check if the object is a character and apply damage
            CharacterController character = hitCollider.GetComponent<CharacterController>();
            if (character != null)
            {
                character.takenDamage(finalDamage);
            }
        }
    }


}

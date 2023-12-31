using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapProjectile : Projectile
{
    private Vector2 initialSpot;
    public override void Launch(GameObject target, Transform startPosition)
    {
        if (target == null)
        {
            Debug.LogError("Launch target is null.");
            return;
        }

        // Calculate the direction to the target
        Vector2 directionToTarget = (target.transform.position - startPosition.position);

        // Determine if the movement should be horizontal or vertical
        // This could be based on the tower's orientation or other logic
        Vector2 launchDirection;
        if (Mathf.Abs(directionToTarget.x) > Mathf.Abs(directionToTarget.y))
        {
            // Horizontal movement
            launchDirection = new Vector2(Mathf.Sign(directionToTarget.x), 0);
        }
        else
        {
            // Vertical movement
            launchDirection = new Vector2(0, Mathf.Sign(directionToTarget.y));
        }

        // Set the projectile's velocity in the determined direction
        rb.velocity = launchDirection * speed;
        rb.gravityScale = 0; // Ensure no gravity effect
        initialSpot = startPosition.position;
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        // If the projectile hits the ground or a target, trigger explosion
        if (IsInHatredList(collider))
        {
            CharacterController character = collider.GetComponent<CharacterController>();
            if (character != null)
            {
                character.takenDamage(finalDamage);
            }
        }
    }

    protected override void HasReachedLifespan()
    {
        if (Vector2.Distance(initialSpot, transform.position) >= firingCharacter.AtkRange)
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject, projectileObject.getPrefab());
    }

}

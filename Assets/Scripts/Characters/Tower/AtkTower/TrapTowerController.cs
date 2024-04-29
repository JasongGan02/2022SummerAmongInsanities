using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Trap tower can shoot horizontally and vertically
// The bullet of trap tower have no gravity

public class TrapTowerController : RangedTowerController
{
    private LayerMask enemyLayer;

    protected override void Start()
    {
        base.Start(); // Calls the Start method of RangedTowerController
        enemyLayer = LayerMask.GetMask("enemy");
    }

    protected override void Attack()
    {
        Vector2 facingDirection = GetFacingDirection();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, AttackRange, enemyLayer);
        if (hit)
        {
            // If enemy is spotted, fire a projectile
            FireProjectile(hit.collider.gameObject);
            _audioEmitter.PlayClipFromCategory("ShootArrow");
        }
    }

    Vector2 GetFacingDirection()
    {
        // Convert the transform's rotation to a quaternion
        Quaternion rotation = transform.rotation;

        // Create a vector representing the direction to face (in this case, to the left as the prefab is facing left)
        Vector3 directionToFace = Vector3.left;

        // Transform the direction to face using the quaternion representing the rotation
        Vector3 facingDirection = rotation * directionToFace;

        // Create a new Vector2 from the Vector3, discarding the z component
        return new Vector2(facingDirection.x, facingDirection.y);
    }
}

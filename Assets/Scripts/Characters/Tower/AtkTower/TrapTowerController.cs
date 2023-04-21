using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Trap tower can shoot horizontally and vertically
// The bullet of trap tower have no gravity

public class TrapTowerController : TowerController
{
    LayerMask enemyLayer;
    // Start is called before the first frame update
    void Start()
    {
        enemyLayer = LayerMask.GetMask("enemy");
        InvokeRepeating("SenseEnemyAndShoot", 0.5f, AtkInterval);
    }

    // Sense enemy and shoot if found
    protected void SenseEnemyAndShoot()
    {
        Vector2 facingDirection = GetFacingDirection();
        //Debug.Log("Facing direction: " + facingDirection);
        // Check if there is an enemy in the facing direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, AtkRange, enemyLayer);
        if (hit)
        {
            //Debug.Log("Enemy spotted in direction " + facingDirection + " at distance " + hit.distance);
            // Shoot a bullet in the facing direction
            Shoot(facingDirection);
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
        //Debug.Log("Facing direction: " + facingDirection + "direction to face: " + directionToFace + "rotation: " + rotation);
        // Create a new Vector2 from the Vector3, discarding the z component
        return new Vector2(facingDirection.x, facingDirection.y);
    }

    // Shoot one bullet into direction
    private void Shoot(Vector2 direction)
    {
        GameObject bulletInstance = Instantiate(bullet, transform.position, transform.rotation);
        bulletInstance.GetComponent<Rigidbody2D>().velocity = direction.normalized * bullet_speed;
    }

}

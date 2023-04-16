using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerController : CharacterController
{
    //SO variables
    protected int energyCost;
    protected Quaternion curAngle;
    protected float bullet_speed;    // bullet flying speed
    [SerializeField] protected GameObject bullet;

    //run-time variables
    protected bool isEnemySpotted;
    protected EnemyContainer enemyContainer;
    protected float AtkTimer;        // Timer

    protected ConstructionMode constructionMode;
    
    protected void Update()
    {
        TestDrop();
    }
    //protected abstract void TowerLoop(); 

    // Find nearest enmey in the enemy array
    protected virtual Transform SenseNearestEnemyTransform()
    {
        var enemyTransforms = enemyContainer.GetComponentsInChildren<Transform>();

        var minDistance = float.MaxValue;
        Transform nearestTarget = transform;

        // find nearest enemy transform
        foreach (var enemyTransform in enemyTransforms)
        {
            if (enemyTransform == enemyContainer)
            {
                continue;
            }

            var currentDistance = Vector3.Distance(transform.position, enemyTransform.position);

            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                nearestTarget = enemyTransform;
            }
        }

        isEnemySpotted = minDistance < AtkRange;

        return nearestTarget;
    }

    protected bool CheckIfEnemyInRange()
    {
        return Vector3.Distance(transform.position, SenseNearestEnemyTransform().position) < AtkRange;
    }

    public override void death()
    {
        constructionMode = FindObjectOfType<ConstructionMode>();
        constructionMode.EnergyConsumption(energyCost*-1);
        Destroy(gameObject);
        OnObjectDestroyed();
    }   

    public void TestDrop() 
    {
        if (Input.GetMouseButtonDown(1)) // Check if right mouse button was clicked
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Create a ray from the mouse position
            RaycastHit2D hitInfo = Physics2D.Raycast(mousePosition, Vector2.zero); // Raycast from the mouse position
            Collider2D collider = GetComponent<Collider2D>(); // Get the collider of the object you want to check
            if (hitInfo.collider != null && hitInfo.collider == collider) // Check if the collider was hit by the ray
            {
                // Execute your function here
                death();
            }
        }
    }

}

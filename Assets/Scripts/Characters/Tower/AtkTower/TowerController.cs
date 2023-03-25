using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerController : CharacterController
{
    //SO variables
    protected int energyCost;
    protected float bullet_speed;    // bullet flying speed
    [SerializeField] protected GameObject bullet;
    protected GameObject shadowPrefab;

    
    //run-time variables
    protected bool isEnemySpotted;
    protected EnemyContainer enemyContainer;
    protected float AtkTimer;        // Timer


    // Find nearest enmey in the enemy array
    protected Transform SenseNearestEnemyTransform()
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

    protected void CheckIfEnemyInRange()
    {
        isEnemySpotted = Vector3.Distance(transform.position, SenseNearestEnemyTransform().position) < AtkRange;
    }

    public override void death()
    {
        Destroy(gameObject);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackTowerController : TowerController
{

    protected float bullet_speed;    // bullet flying speed
    [SerializeField] protected GameObject bullet;

    //run-time variables
    protected bool isEnemySpotted;
    protected EnemyContainer enemyContainer;
    protected float AtkTimer;        // Timer


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

}

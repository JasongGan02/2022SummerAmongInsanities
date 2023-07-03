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

    protected float SensingRange;
    public List<string> Hatred = new List<string>();
    public GameObject tempTarget;
    public Collider2D[] colliders;

    protected int layerMask = (1 << 8) | (1 << 9) | (1 << 10);

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

    public GameObject WhatToAttack()
    {
        GameObject target = null;
        if (Hatred.Count > 0)
        {
            //Debug.Log(Hatred.Count);
            for (int i = 0; i < Hatred.Count; i++)
            {
                if (CouldSense(Hatred[i], SensingRange))
                {
                    return tempTarget;
                }
            }
        }
        else { Debug.Log("Hatred is less than 0"); }
        return target;
    }

    public bool CouldSense(string name, float range)
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, range, layerMask);
        //Debug.Log(colliders.Length);
        foreach (Collider2D collider in colliders)
        {
            //Debug.Log("collider's name is " + collider.gameObject.name);
            if (collider.gameObject.GetComponent(name) != null)
            {
                // Found a component with the specified name on the GameObject
                tempTarget = collider.gameObject;
                return true;
            }
        }
        //Debug.Log("didn't find target");
        return false;
    }

}

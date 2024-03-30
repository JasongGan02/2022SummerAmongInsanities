using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.EventSystems.EventTrigger;

public abstract class AttackTowerController : TowerController
{

    //run-time variables
    protected bool isEnemySpotted;
     

    // Find nearest enemy in the enemy array
    protected virtual GameObject WhatToAttack()
    {
        if (Hatred.Count == 0)
        {
            Debug.Log("Hatred List is empty");
            isEnemySpotted = false;
            return null; // No enemy types to target
        }

        float minDistance = float.MaxValue;
        GameObject nearestTarget = null;

        // Get all enemies from the container
        var enemies = MobSpawner.FindEnemyNearby(transform.position); // Assuming 'Enemy' is your enemy script

        // Iterate over each hated type
        foreach (var hatedType in Hatred)
        {
            Type type = Type.GetType(hatedType.name);
            if (type == null) continue;

            // Check each enemy against the hated types
            foreach (var enemy in enemies)
            {
                if (type.IsAssignableFrom(enemy.GetType()) || type.Equals(enemy.GetType()))
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < minDistance && distance < _atkRange)
                    {
                        minDistance = distance;
                        nearestTarget = enemy.gameObject;
                    }
                }
            }
        }

        isEnemySpotted = nearestTarget != null;
        return nearestTarget; // Returns null if no hated enemy type is found within range
    }

}

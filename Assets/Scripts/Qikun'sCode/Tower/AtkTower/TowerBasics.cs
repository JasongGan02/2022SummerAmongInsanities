using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerBasics : MonoBehaviour
{
    public float AtkRange;        // When enemy enters the atk range, the tower will start fire
    public float AtkIntervalTime; // During AtkIntervalTime, the tower will automatically attack once
    public float AtkTimer;        // Timer
    public float bullet_speed;    // bullet flying speed
    public GameObject bullet;
    public bool isEnemySpotted;
    public EnemyContainer enemyContainer;

    // Find nearest enmey in the enemy array
    protected Transform SenseNearestEnemyTransform()
    {
        Transform[] enemyTransforms = enemyContainer.GetComponentsInChildren<Transform>();
        
        float min_distance = float.MaxValue;
        Transform nearest_Target = transform;

        // find nearsest enemy transform
        foreach(Transform e in enemyTransforms)
        {
            if(e == enemyTransforms[0])
            {
                continue;
            }
            float current_distance = CalculateDistanceFromEnemyToTower(e);
            if(current_distance < min_distance)
            {
                min_distance = current_distance;
                nearest_Target = e;
            }
        }

        if(min_distance < AtkRange)
        {
            isEnemySpotted = true;
        }else{
            isEnemySpotted = false;
        }
        
        return nearest_Target;
    }

    protected float CalculateDistanceFromEnemyToTower(Transform enemyTransform)
    {
        Vector3 towerPosition = transform.position;
        Vector3 enemyPosition = enemyTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    private Playermovement player;
    private TowerContainer towerContainer;
    private float movingSpeed = 1;
    bool isFindTower;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Playermovement>();
        towerContainer = FindObjectOfType<TowerContainer>();
        isFindTower = false;
    }

    // Update is called once per frame
    void Update()
    {
        SenseNearestTarget();
    }

    // Find nearest player or tower. When begin sense nearest target? 1. Under attack; 2. Find tower or player
    void SenseNearestTarget()
    {
        float distance_to_player = CalculateDistanceToPlayer();
        Transform nearest_tower = FindNearestTower();
        float distance_to_nearest_tower = CalculateDistanceFromEnemyToTower(nearest_tower);
        
        if(isFindTower)
        {
            if(distance_to_player <= distance_to_nearest_tower)
            {
                if(distance_to_player > 1)
                {
                    // Approaching player
                    ApproachingTarget(player.transform);
                }else
                {
                    // Attack player
                    print("Atking player");
                }
                
            }else
            {
                if(distance_to_nearest_tower > 1)
                {
                    // Approaching tower
                    ApproachingTarget(nearest_tower);
                }else
                {
                    // Attack tower
                    print("Atking tower");
                }
            }
        }else
        {
            // only consider player position
        }
    }

    // Approaching target with moving speed, this will be complicated when the land becomes complex
    void ApproachingTarget(Transform target_transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, movingSpeed*Time.deltaTime);
    }

    Transform FindNearestTower()
    {
        Transform[] towerTransforms = towerContainer.GetComponentsInChildren<Transform>();
        Transform nearest_Transform = transform;
        float min_distance = 0;
        foreach(Transform e in towerTransforms)
        {
            if(e==towerTransforms[0])
            {
                isFindTower = false;
                continue;
            }
            // for the first tower
            if(e==towerTransforms[1])
            {
                isFindTower = true;
                min_distance = CalculateDistanceFromEnemyToTower(e);
                nearest_Transform = e;
                continue;
            }

            // for other towers
            float distance = CalculateDistanceFromEnemyToTower(e);
            if(distance < min_distance)
            {
                min_distance = distance;
                nearest_Transform = e;
            }
        }

        return nearest_Transform;
    }

    // Calculate the distance between tower and enemy
    float CalculateDistanceFromEnemyToTower(Transform towerTransform)
    {
        Vector3 enemyPosition = transform.position;
        Vector3 towerPosition = towerTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }

    
    float CalculateDistanceToPlayer()
    {
        Vector3 player_position = player.gameObject.transform.position;
        float x1 = transform.position.x;
        float y1 = transform.position.y;
        float x2 = player_position.x;
        float y2 = player_position.y;
        float distance = Mathf.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));

        return distance;
    }
}

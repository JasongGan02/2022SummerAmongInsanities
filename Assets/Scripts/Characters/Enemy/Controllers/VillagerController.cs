using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerController : EnemyController
{ 
    new void Update()
    {
        EnemyLoop();
    }
    
    protected override void EnemyLoop()
    {
        if(IsPlayerSensed())
        {
            if(IsPlayerInAtkRange())
            {
                Debug.Log(timer);
                attack();
            }else
            {
                // approaching player
                ApproachingTarget(player.transform);
            }
        }else if(IsTowerSensed())
        {
            if(IsTowerInAtkRange( (int) AtkRange))
            {
                // atk tower
                print("atking tower");
                attackTower(NearestTowerTransform);
            }else
            {
                // approaching tower
                ApproachingTarget(NearestTowerTransform);
            }
        }else
        {
            // Patrol
            //print("patroling");
        }
    }

    public override void death()
    {
        Destroy(this.gameObject);
    }

    bool IsTowerInAtkRange(int AtkRange)
    {
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        if (distance <= AtkRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void attackTower(Transform target)
    {
        transform.position = Vector2.MoveTowards(transform.position, NearestTowerTransform.position, MovingSpeed * 2);
        if (CalculateDistanceFromEnemyToTower(NearestTowerTransform) < 0.2f)
        {
            NearestTowerTransform.gameObject.GetComponent<TowerHealth>().DecreaseHealth((int)AtkDamage);
        }
    }

}

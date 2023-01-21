using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingZombie : EnemyBasics
{
    void Update()
    {
        EnemyLoop();
    }

    protected override void EnemyLoop()
    {
        if(IsPlayerSensed())
        {
            if(IsPlayerInAtkRange())
            {
                // atk player
                print("atking player");
            }else
            {
                // approaching player
                ApproachingTarget(player.transform);
            }
        }else if(IsTowerSensed())
        {
            if(IsTowerInAtkRange())
            {
                // atk tower
                print("atking tower");
            }else
            {
                // approaching tower
                ApproachingTarget(NearestTowerTransform);
            }
        }else
        {
            // Patrol
            print("patroling");
        }
    }

}

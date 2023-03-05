using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VillagerController : EnemyController
{
    bool rest = false;
    float cooldown = 0;

    protected override void EnemyLoop()
    {
        //Debug.Log(Vector2.Distance(transform.position, player.transform.position));
        if(IsPlayerSensed())
        {
            if(IsPlayerInAtkRange())
            {
                //Debug.Log(timer);
                attack();
                Debug.Log("attack");
            }else
            {
                // approaching player
                ApproachingTarget(player.transform);
                Debug.Log("approach");
            }
        }
        else if(IsTowerSensed())
        {
            Debug.Log("attack tower");
            if (IsTowerInAtkRange( (int) AtkRange))
            {
                // atk tower
                print("atking tower");
                attackTower(NearestTowerTransform);
            }else
            {
                // approaching tower
                ApproachingTarget(NearestTowerTransform);
            }
        }
        else
        {
            Debug.Log("patrol");
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

    // rotate when attack
    IEnumerator rotateZombie()      
    {
        transform.Rotate(0, 0, 30);
        yield return new WaitForSeconds(0.2f);
        transform.Rotate(0, 0, -30);
    }

    new void attack()
    {
        if (rest)
        {
            if (cooldown > AtkInterval)
            {
                cooldown = 0;
                rest = false;
            }
            else { cooldown += Time.deltaTime; }
            Debug.Log("rest");
        }

        if (Vector2.Distance(transform.position, player.transform.position) > 0.7f)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, MovingSpeed * 2 * Time.deltaTime);
        }
        
        if (Vector2.Distance(transform.position, player.transform.position) < 1f && !rest)
        {
            StartCoroutine(rotateZombie());
            Debug.Log("hit");
            player.GetComponent<PlayerController>().takenDamage(AtkDamage);
            rest = true;
        }
    }

}

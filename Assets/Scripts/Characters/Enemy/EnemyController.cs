using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : CharacterController
{
    protected EnemyObject EnemyData;
    protected TowerContainer towerContainer;
    protected Playermovement player;
    protected float timer;
    protected Transform NearestTowerTransform;

    protected bool isFindTower;
    protected bool isTouchTower;
    protected bool isFindPlayer;
    protected bool isTouchPlayer;
    protected float SensingRange;

    public float curHP;

    public void Initialize(CharacterObject character, float HP, float AtkDamage, float AtkInterval, float MovingSpeed, float SensingRange)
    {
        base.Initialize(character, HP, AtkDamage, AtkInterval, MovingSpeed);
        this.SensingRange = SensingRange;
        

    }

    protected void Start()
    {
        player = FindObjectOfType<Playermovement>();
        towerContainer = FindObjectOfType<TowerContainer>();
        timer = 0;
  
    }

    
    protected bool IsTowerSensed()
    {
        UpdateNearestTower();
        if(NearestTowerTransform == transform)
        {
            return false;
        }
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        if(distance <= 15)
        {
            return true;
        }else
        {
            return false;
        }
    }

    protected bool IsTowerInAtkRange()
    {
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        if(distance <= 15)
        {
            return true;
        }else
        {
            return false;
        }
    }

    protected bool IsPlayerSensed()
    {
        float distance = CalculateDistanceToPlayer();
        if(distance <= 15)
        {
            return true;
        }else
        {
            return false;
        }
    }

    protected bool IsPlayerInAtkRange()
    {
        float distance = CalculateDistanceToPlayer();
        if(distance <= 1)
        {
            return true;
        }else
        {
            return false;
        }
    }

    protected abstract void EnemyLoop();
    
    // protected abstract void Patrol();

    protected void ApproachingTarget(Transform target_transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, EnemyData.MovingSpeed*Time.deltaTime);
        SenseFrontBlock();
        // transform directiron change
        if(target_transform.position.x >= transform.position.x)
        {
            transform.eulerAngles = new Vector3(0,180,0);
        }else{
            transform.eulerAngles = new Vector3(0,0,0);
        }
    }

    // Shooting a 2D rayline, if sense a ground tag, then jump
    protected void SenseFrontBlock()
    {
        Vector3 shooting_direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position - new Vector3(0,0.3f,0);
        Debug.DrawRay(origin, shooting_direction, Color.green);

        LayerMask ground_mask = LayerMask.GetMask("ground");
        RaycastHit2D hit = Physics2D.Raycast(origin, shooting_direction, 1f, ground_mask);
        if(hit.collider != null && hit.collider.gameObject.tag == "ground")
        {
            Vector2 up_force = new Vector2(0,30);
            gameObject.GetComponent<Rigidbody2D>().AddForce(up_force);
        }
    }

    protected void UpdateNearestTower()
    {
        Transform[] towerTransforms = towerContainer.GetComponentsInChildren<Transform>();
        Transform nearest_Transform = transform;
        float min_distance = 0;
        foreach(Transform e in towerTransforms)
        {
            if(e==towerTransforms[0])
            {
                continue;
            }
            // for the first tower
            if(e==towerTransforms[1])
            {
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
        NearestTowerTransform = nearest_Transform;
    }

    protected float CalculateDistanceFromEnemyToTower(Transform towerTransform)
    {
        Vector3 enemyPosition = transform.position;
        Vector3 towerPosition = towerTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }

    protected float CalculateDistanceToPlayer()
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

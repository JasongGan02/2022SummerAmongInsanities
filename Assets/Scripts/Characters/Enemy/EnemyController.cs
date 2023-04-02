using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Constants;

public abstract class EnemyController : CharacterController
{

    protected float SensingRange;
    

    //run-time variables
    public TowerContainer towerContainer;   // Changed from protected to public
    protected GameObject player;
    protected float timer;
    protected Transform NearestTowerTransform;

    protected bool isFindTower;
    protected bool isTouchTower;
    protected bool isFindPlayer;
    protected bool isTouchPlayer;


    protected void Awake()
    {
        //towerContainer = FindObjectOfType<TowerContainer>();
        timer = 0;
 
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
    }

    protected void Update()
    {
        if (player == null) 
        { 
            player = GameObject.Find("Player"); 
        }
        EnemyLoop();
    }

    protected bool IsTowerSensed()
    {
        if (towerContainer == null) { Debug.Log("0"); return false; }  // Nathan's only change
        
        UpdateNearestTower();
        if(NearestTowerTransform == transform)
        {

            //Debug.Log("1");
            return false; 

        }
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        //Debug.Log("distance: " + distance.ToString()); 
        if(distance <= SensingRange)
        {
            //Debug.Log("2");
            return true;
        }else
        {
            //Debug.Log("3");
            return false;
        }
    }

    protected bool IsTowerInAtkRange()
    {
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        if(distance <= AtkRange)
        {
            return true;
        }else
        {
            return false;
        }
    }

    protected bool IsPlayerSensed()
    {
        if (player == null) 
        { 
            return false;
        }

        float distance = CalculateDistanceToPlayer();
        if(distance <= SensingRange)
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
        if(distance <= AtkRange)
        {
            return true;
        }else
        {
            timer =0;
            return false;
        }
    }

    protected abstract void EnemyLoop();
    
    // protected abstract void Patrol();

    protected void ApproachingTarget(Transform target_transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, MovingSpeed*Time.deltaTime);
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
        Vector3 origin = transform.position - new Vector3(0, 0.5f, 0);

        LayerMask ground_mask = LayerMask.GetMask("ground");

        RaycastHit2D hit = Physics2D.Raycast(origin, shooting_direction, 0.5f, ground_mask);
        Debug.DrawRay(origin, shooting_direction * 0.5f, Color.green); // infront
        Vector3 left = transform.position - new Vector3(-0.3f, 0.2f, 0);
        RaycastHit2D bottomLeft = Physics2D.Raycast(left, Vector3.down, 0.2f, ground_mask);
        Debug.DrawRay(left, Vector3.down * 0.2f, Color.blue);        // bottom left
        Vector3 right = transform.position + new Vector3(0.2f, 0.2f, 0);
        RaycastHit2D bottomRight = Physics2D.Raycast(right, Vector3.down, 0.2f, ground_mask);
        Debug.DrawRay(right, Vector3.down * 0.2f, Color.blue);        // bottom right

        if (hit.collider != null && 
            bottomLeft.collider != null && 
            bottomRight.collider != null)
        {
            //Vector2 up_force = new Vector2(0, JumpForce);
            //gameObject.GetComponent<Rigidbody2D>().AddForce(up_force); 
            //Debug.Log("up_force: " + up_force);
            if (hit.collider.gameObject.tag == "ground" &&
                bottomLeft.collider.gameObject.tag == "ground" &&
                bottomRight.collider.gameObject.tag == "ground")
            {
                Vector2 up_force = new Vector2(0, JumpForce);
                Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                rb.AddForce(up_force, ForceMode2D.Impulse);
                Debug.Log("up_force: " + up_force);
                StartCoroutine(StopJump(rb, 0.5f)); //stop the jump after 0.5 seconds
            }
        }
        
    }
    IEnumerator StopJump(Rigidbody2D rb, float duration)
    {
        yield return new WaitForSeconds(duration);
        rb.velocity = new Vector2(rb.velocity.x, 0);
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
    
    protected void attack()
    {
        if(timer >= AtkInterval)
        {
            player.GetComponent<PlayerController>().takenDamage(AtkDamage);
            timer = 0f;
        }
    }

    public override void death()
    {
        Destroy(this.gameObject);
        OnObjectDestroyed();
    }

}

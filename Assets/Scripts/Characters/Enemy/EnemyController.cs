using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Constants;
using System;
using System.Linq;
using System.Linq.Expressions;
using static UnityEngine.GraphicsBuffer;

public abstract class EnemyController : CharacterController
{
    protected float SensingRange;

    protected static int globalEnemyLevel = 1;
    public GameObject tempTarget;
    public Collider2D[] colliders;

    //run-time variables
    public TowerContainer towerContainer;   // Changed from protected to public
    protected GameObject player;
    protected GameObject nearestTower;
    protected float timer;
    protected Transform NearestTowerTransform;

    protected Rigidbody2D rb;
    protected bool isFindTower;
    protected bool isTouchTower;
    protected bool isFindPlayer;
    protected bool isTouchPlayer;

    protected int layerMask = (1 << 8) | (1 << 9) | (1 << 10);

    Type type;

    protected override void Awake()
    {
        //towerContainer = FindObjectOfType<TowerContainer>();
        base.Awake();
        timer = 0;
    }


    public void LevelUp()
    {
        Reinitialize();
    }



    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
    }


    protected override void Update()
    {
        base.Update();
        if (player == null) 
        { 
            player = GameObject.Find("Player"); 
        }
        if (this.transform.position.y < -400) death();
        SetEnemyContainer();
        EnemyLoop();
    }

    protected bool IsTowerSensed()
    {
        if (towerContainer == null) { /*Debug.Log("0");*/ return false; }  // Nathan's only change
        
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
        if(distance <= _atkRange)
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
        if(distance <= _atkRange)
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
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, _movingSpeed*Time.deltaTime);
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
                Vector2 up_force = new Vector2(0, _jumpForce);
                Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                rb.AddForce(up_force, ForceMode2D.Impulse);
                //Debug.Log("up_force: " + up_force);
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
        Transform nearest_Transform = towerTransforms[0];
        float min_distance = Vector2.Distance(towerTransforms[0].position, transform.position);
        foreach(Transform e in towerTransforms)
        {
            Debug.Log(e);
            if (e.transform.gameObject.CompareTag("tower"))
            {
                Debug.Log(e);

                if ((Vector2.Distance(e.position, transform.position) < min_distance) && (e.name != "FiringPoint"))
                {
                    nearest_Transform = e;
                    min_distance = Vector2.Distance(e.position, transform.position);
                }
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

    protected float DistanceToTarget(Transform target)
    {
        Vector2 targetPosition = target.position;
        Vector2 transformPosition = transform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((targetPosition.x - transformPosition.x), 2) + Mathf.Pow((targetPosition.y - transformPosition.y), 2));
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
        if(timer >= _atkSpeed)
        {
            ApplyDamage(player.GetComponent<CharacterController>());
            timer = 0f;
        }
    }

    public GameObject WhatToAttack()
    {
        GameObject target = null;
        if (Hatred.Count > 0)
        {
            //Debug.Log(Hatred.Count);
            for (int i = 0; i < Hatred.Count; i++)
            {
                if (CouldSense(Hatred[i].name, SensingRange))
                {
                    return tempTarget;
                }
            }
        }
        else { //Debug.Log("Hatred is empty");
               }
        return target;
    }

    public bool CouldSense(string name, float range)
    {
        type = Type.GetType(name);

        colliders = Physics2D.OverlapCircleAll(transform.position, range, layerMask);
        //Debug.Log(colliders.Length);
        foreach (Collider2D collider in colliders)
        {
            MonoBehaviour[] components = collider.gameObject.GetComponents<CharacterController>();
            foreach (MonoBehaviour component in components)
            {
                if (component != null )
                {
                    if (type.IsAssignableFrom(component.GetType()) || type.Equals(component.GetType()))
                    {
                        tempTarget = collider.gameObject;
                        return true;
                    }
                }
            }

        }
        //Debug.Log("didn't find target");
        return false; 
    }

    public Vector3 GetPosition()
    {
        return transform.position; // Return the GameObject's position
    }

    protected void SetEnemyContainer()
    {
        int chunkCoord = WorldGenerator.GetChunkCoordsFromPosition(transform.position);
        if (WorldGenerator.ActiveChunks.ContainsKey(chunkCoord))
            transform.SetParent(WorldGenerator.ActiveChunks[chunkCoord].transform.Find("MobContainer").Find("EnemyContainer"), true);
    }

    public void ShakePlayerOverHead()
    {
        if (Math.Abs(player.transform.position.x - this.transform.position.x) < 0.1f)
        {
            if (Math.Abs(player.transform.position.y - this.transform.position.y) < 0.3f)
            {
                Debug.Log("shaking the player over head");
                if (UnityEngine.Random.Range(0f, 1f) <= 0.5f)
                {
                    this.rb.velocity = new Vector2(-2, rb.velocity.y);
                }
                else
                {
                    this.rb.velocity = new Vector2(2, rb.velocity.y);
                }
            }
        }
    }

}

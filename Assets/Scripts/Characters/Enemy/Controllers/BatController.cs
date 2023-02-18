using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BatController : EnemyController
{
    private bool facingRight = false;
    private float waitTime;
    private Transform moveTo;
    private bool planned;
    private bool prepare_dash;
    private bool is_dashing;
    private Vector2 dash_end;
    private Vector2 stop_point;
    private Animator animator;

    private bool attacked;

    private TrailRenderer Tr;
    private ParticleSystem Ps;
    private new float timer;

    // Start is called before the first frame update
    protected void Start()
    {
        // these 3 lines from EnemyBasics
        towerContainer = FindObjectOfType<TowerContainer>();

        GameObject destination = new GameObject("newObject");
        moveTo = destination.transform;
        // initial moveTo

        waitTime = 3;
        timer = 0;
        moveTo.position = new Vector2(Random.Range(30, 60), Random.Range(16, 23));

        planned = false;
        prepare_dash = true;
        is_dashing = false;
        attacked = false;
        stop_point = player.transform.position;

    }

    new void Awake()
    {
        animator = GetComponent<Animator>();
        Tr = GetComponent<TrailRenderer>();
        Ps = GetComponent<ParticleSystem>();
        //if (Ps.isPlaying) Ps.Stop();
    }
    void Update()
    {
        EnemyLoop();
    }

    protected override void EnemyLoop()
    {
        if (player == null)
        {
            // Patrol
            if(animator.GetBool("is_attacking") == true) { animator.SetBool("is_attacking", false); }
            if(Tr.emitting == true) { Tr.emitting = false; }
            Patrol();
            if (moveTo.position.x < transform.position.x && facingRight || moveTo.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
        }
        else 
        {
            if (IsPlayerSensed() || planned)
            {
                if (IsPlayerInAtkRange() || planned)
                {
                    // atk player
                    DashAttack();
                }
                else
                {
                    // approaching player
                    ApproachingTarget(player.transform);
                }
            }
            else
            {
                // Patrol
                Patrol();
                if (moveTo.position.x < transform.position.x && facingRight || moveTo.position.x > transform.position.x && !facingRight)
                {
                    Flip();
                }
            }
        }
    }

    protected new bool IsPlayerSensed()
    {
        float distance = CalculateDistanceToPlayer();
        if (distance <= SensingRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected new bool IsPlayerInAtkRange()
    {
        float distance = CalculateDistanceToPlayer();
        if (distance <= 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, moveTo.position, 3 * Time.deltaTime);

        if (Vector2.Distance(transform.position, moveTo.position) < 0.2f)
        {
            if (waitTime <= 0)
            {
                moveTo.position = new Vector2(Random.Range(30, 60), Random.Range(16, 23));
                waitTime = 3;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

    new void ApproachingTarget(Transform target_transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, MovingSpeed * Time.deltaTime);
        if (target_transform.position.x < transform.position.x && facingRight || target_transform.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    void DashAttack()
    {

        if (prepare_dash)           
        {
            if (!Ps.isPlaying) Ps.Play();

            if (!planned)
            {         // set attack route
                PlanRoute();
                planned = true;
            }
            
            if (timer > 0.8f) 
            {
                prepare_dash = false;
                is_dashing = true;
                PlanRoute();
            }
            else
            {
                timer += Time.deltaTime;
            }

        }
        else if (is_dashing)        // dash through the player and attack
        {
            timer = 0;
            if (Ps.isPlaying) Ps.Stop();
            transform.position = Vector2.MoveTowards(transform.position, dash_end, MovingSpeed * 5 * Time.deltaTime);
            animator.SetBool("is_attacking", true); 
            Tr.emitting = true;
            if (Vector2.Distance(transform.position, player.transform.position) < 0.4f && !attacked)
            {
                player.GetComponent<PlayerController>().takenDamage(AtkDamage);
                attacked = true;
            }
            if (CloseEnough(transform.position, dash_end))
            {
                animator.SetBool("is_attacking", false);    
                Tr.emitting = false;
                is_dashing = false;
            }
        }
        else                        // return to the higher point and attack again
        {
            if (CloseEnough(transform.position, stop_point))
            {
                //timer = 0;
                prepare_dash = true;
                attacked = false;
                planned = false;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, stop_point, MovingSpeed * Time.deltaTime);
            }
            if (player.transform.position.x < transform.position.x && facingRight || player.transform.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
        }

    }
    // make attack plan (dash_start -> dash_End -> stop_point -> dash_start...)
    void PlanRoute()
    {
        dash_end = player.transform.position;
        stop_point = player.transform.position;
        if (player.transform.position.x > transform.position.x)           // player is on the right side
        {
            dash_end.x += player.transform.position.x - transform.position.x;
            dash_end.y -= transform.position.y - player.transform.position.y;
            stop_point.x += 1;
            stop_point.y += 3;
        }
        else                                                    // player is on the left side
        {
            dash_end.x -= (transform.position.x - player.transform.position.x);
            dash_end.y -= (transform.position.y - player.transform.position.y);
            stop_point.x -= 1;
            stop_point.y += 3;
        }
    }


    void Flip()
    {
        facingRight = !facingRight;

        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }


    // check for collision with player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("contacted");
        }
    }

    // check if two points are close enough
    bool CloseEnough(Vector2 first, Vector2 second)
    {
        if (Vector2.Distance(first, second) < 0.2f) { return true; }
        return false;
    }

}


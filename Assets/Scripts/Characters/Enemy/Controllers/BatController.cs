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
    private float BatTimer;

    private GameObject target;

    private float startX;
    private float startY;
    private Transform GroupApproachTarget;

    // Start is called before the first frame update
    protected void Start()
    {
        GameObject destination = new GameObject("newObject");
        moveTo = destination.transform;
        // initial moveTo

        waitTime = 3;
        BatTimer = 0;
        startX = this.transform.position.x;
        startY = this.transform.position.y;
        moveTo.position = new Vector2(Random.Range(startX-3, startX+3), Random.Range(startY-3, startY+3));

        planned = false;
        prepare_dash = true;
        is_dashing = false;
        attacked = false;

        stop_point = new Vector3(0,0,0); //place holder


    }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        Tr = GetComponent<TrailRenderer>();
        Ps = GetComponent<ParticleSystem>();
        
    }
    

    protected override void UpdateEnemyBehavior()
    {
        if (!planned) { target = SearchForTargetObject(); } // attacking behavior is uninterruptable
        else if (target == null) { target = SearchForTargetObject(); } // once target is destroied but an attack is planned

        if (target == null)
        {
            // Patrol
            //Debug.Log("Bat is patroling");
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
            if (Vector2.Distance(target.transform.position, transform.position) < enemyStats.sensingRange || planned)
            {
                if (Vector2.Distance(target.transform.position, transform.position) < currentStats.attackRange || planned)
                {
                    // atk player
                    DashAttack(target.transform);
                }
                else
                {
                    // approaching player
                    ApproachingTarget(target.transform);
                }
            }
            else
            {
                Debug.Log("Bat is patroling 2");
                Patrol();
                if (moveTo.position.x < transform.position.x && facingRight || moveTo.position.x > transform.position.x && !facingRight)
                {
                    Flip();
                }
            }
        }
    }

    void Patrol()
    {
        if (IsGroupAttacking) 
        { 
            transform.position = Vector2.MoveTowards(transform.position, GroupApproachTarget.position, 3 * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, moveTo.position, 3 * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, moveTo.position) < 0.2f || waitTime <= 0)
        {
            if (waitTime <= 0)
            {
                moveTo.position = new Vector2(Random.Range(startX - 3, startX + 3), Random.Range(startY - 3, startY + 3));  // cause startX I don't know need to change
                waitTime = 3;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

        waitTime -= Time.deltaTime;
    }

    new void ApproachingTarget(Transform target_transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, currentStats.movingSpeed * Time.deltaTime);
        if (target_transform.position.x < transform.position.x && facingRight || target_transform.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    void DashAttack(Transform destination)
    {
        //Debug.Log("is attacking");
        if (prepare_dash)           
        {
            if (!Ps.isPlaying) Ps.Play();

            if (!planned)
            {         // set attack route
                PlanRoute(destination);
                planned = true;
            }
            
            if (BatTimer > 0.8f) 
            {
                prepare_dash = false;
                is_dashing = true;
                PlanRoute(destination);
            }
            else
            {
                BatTimer += Time.deltaTime;
            }

        }
        else if (is_dashing)        // dash through the player and attack
        {
            BatTimer = 0;
            if (Ps.isPlaying) Ps.Stop();
            transform.position = Vector2.MoveTowards(transform.position, dash_end, currentStats.movingSpeed * 5 * Time.deltaTime);
            animator.SetBool("is_attacking", true); 
            Tr.emitting = true;
            if (Vector2.Distance(transform.position, destination.position) < 0.4f && !attacked)
            {
                ApplyDamage(target.GetComponent<CharacterController>());
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
                //BatTimer = 0;
                prepare_dash = true;
                attacked = false;
                planned = false;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, stop_point, currentStats.movingSpeed * Time.deltaTime);
            }
            if (destination.position.x < transform.position.x && facingRight || destination.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
        }

    }
    // make attack plan (dash_start -> dash_End -> stop_point -> dash_start...)
    void PlanRoute(Transform destination)
    {
        dash_end = destination.position;
        stop_point = destination.position;
        if (destination.position.x > transform.position.x)           // player is on the right side
        {
            dash_end.x += destination.position.x - transform.position.x;
            dash_end.y -= transform.position.y - destination.position.y;
            stop_point.x += 1;
            stop_point.y += 3;
        }
        else                                                    // player is on the left side
        {
            dash_end.x -= (transform.position.x - destination.position.x);
            dash_end.y -= (transform.position.y - destination.position.y);
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
            //Debug.Log("contacted");
        }
    }

    // check if two points are close enough
    bool CloseEnough(Vector2 first, Vector2 second)
    {
        if (Vector2.Distance(first, second) < 0.2f) { return true; }
        return false;
    }

    protected override void MoveTowards(Transform targetTransform)
    {
        GroupApproachTarget = targetTransform;
        IsGroupAttacking = true;
    }
}


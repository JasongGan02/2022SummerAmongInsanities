using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BatController : EnemyController
{
    private float WaitTime;
    private Transform MoveTo;
    private bool Planned;
    private bool PrepareDash;
    private bool IsDashing;
    private Vector2 DashEnd;
    private Vector2 StopPoint;
    private new Animator animator;

    private bool Attacked;

    private TrailRenderer Tr;
    private ParticleSystem Ps;
    private float BatTimer;

    private new GameObject target;

    private float StartHori;
    private float StartVir;
    private Transform GroupApproachTarget;

    // Start is called before the first frame update
    protected override void Start()
    {
        MoveTo = transform.Find("destination").transform;
        // initial MoveTo

        WaitTime = 3;
        BatTimer = 0;
        StartHori = this.transform.position.x;
        StartVir = this.transform.position.y;
        MoveTo.position = new Vector2(Random.Range(StartHori-3, StartHori+3), Random.Range(StartVir-3, StartVir+3));

        Planned = false;
        PrepareDash = true;
        IsDashing = false;
        Attacked = false;

        StopPoint = new Vector3(0,0,0); //place holder


    }

    protected override string IdleAnimationState { get; }
    protected override string AttackAnimationState { get; }
    protected override string MoveAnimationState { get; }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        Tr = GetComponent<TrailRenderer>();
        Ps = GetComponent<ParticleSystem>();
        
    }
    

    protected override void UpdateEnemyBehavior()
    {
        if (!Planned) { target = SearchForTargetObject(); } // attacking behavior is uninterruptable
        else if (target == null) { target = SearchForTargetObject(); } // once target is destroied but an attack is Planned

        if (target == null)
        {
            // Patrol
            //Debug.Log("Bat is patroling");
            if(animator.GetBool("is_attacking") == true) { animator.SetBool("is_attacking", false); }
            if(Tr.emitting == true) { Tr.emitting = false; }
            Patrol();
            if (MoveTo.position.x < transform.position.x && facingRight || MoveTo.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
        }
        else 
        {
            if (Vector2.Distance(target.transform.position, transform.position) < enemyStats.sensingRange || Planned)
            {
                if (Vector2.Distance(target.transform.position, transform.position) < currentStats.attackRange || Planned)
                {
                    // atk target
                    DashAttack(target.transform);
                }
                else
                {
                    // approaching target
                    Approach(currentStats.movingSpeed, target.transform.position);
                }
            }
            else
            {
                //Debug.Log("Bat is patroling 2");
                if (TimeSystemManager.Instance.IsRedMoon){
                    // Debug.Log("It's a red moon night! The bats approach core!");
                    Approach(currentStats.movingSpeed, corePosition);
                }else{
                    Patrol();
                    if (MoveTo.position.x < transform.position.x && facingRight || MoveTo.position.x > transform.position.x && !facingRight)
                    {
                        Flip();
                    }
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
            transform.position = Vector2.MoveTowards(transform.position, MoveTo.position, 3 * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, MoveTo.position) < 0.2f || WaitTime <= 0)
        {
            if (WaitTime <= 0)
            {
                MoveTo.position = new Vector2(Random.Range(StartHori - 3, StartHori + 3), Random.Range(StartVir - 3, StartVir + 3));  // cause StartHori I don't know need to change
                WaitTime = 3;
            }
            else
            {
                WaitTime -= Time.deltaTime;
            }
        }

        WaitTime -= Time.deltaTime;
    }

    // private new void Approach(float speed, Vector2 targetTransform)
    // {
    //     transform.position = Vector2.MoveTowards(transform.position, targetTransform, speed * Time.deltaTime);
    //     if (targetTransform.x < transform.position.x && facingRight || targetTransform.x > transform.position.x && !facingRight)
    //     {
    //         Flip();
    //     }
    // }

    void DashAttack(Transform destination)
    {
        //Debug.Log("is attacking");
        if (PrepareDash)           
        {
            if (!Ps.isPlaying) Ps.Play();

            if (!Planned)
            {         // set attack route
                PlanRoute(destination);
                Planned = true;
            }
            
            if (BatTimer > 0.8f) 
            {
                PrepareDash = false;
                IsDashing = true;
                PlanRoute(destination);
            }
            else
            {
                BatTimer += Time.deltaTime;
            }

        }
        else if (IsDashing)        // dash through the player and attack
        {
            BatTimer = 0;
            if (Ps.isPlaying) Ps.Stop();
            transform.position = Vector2.MoveTowards(transform.position, DashEnd, currentStats.movingSpeed * 5 * Time.deltaTime);
            animator.SetBool("is_attacking", true); 
            Tr.emitting = true;
            if (Vector2.Distance(transform.position, destination.position) < 0.4f && !Attacked)
            {
                ApplyDamage(target.GetComponent<CharacterController>());
                Attacked = true;
            }
            if (CloseEnough(transform.position, DashEnd))
            {
                animator.SetBool("is_attacking", false);    
                Tr.emitting = false;
                IsDashing = false;
            }
        }
        else                        // return to the higher point and attack again
        {
            if (CloseEnough(transform.position, StopPoint))
            {
                //BatTimer = 0;
                PrepareDash = true;
                Attacked = false;
                Planned = false;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, StopPoint, currentStats.movingSpeed * Time.deltaTime);
            }
            if (destination.position.x < transform.position.x && facingRight || destination.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
        }

    }
    // make attack plan (dash_start -> dash_End -> StopPoint -> dash_start...)
    void PlanRoute(Transform destination)
    {
        DashEnd = destination.position;
        StopPoint = destination.position;
        if (destination.position.x > transform.position.x)           // player is on the right side
        {
            DashEnd.x += destination.position.x - transform.position.x;
            DashEnd.y -= transform.position.y - destination.position.y;
            StopPoint.x += 1;
            StopPoint.y += 3;
        }
        else                                                    // player is on the left side
        {
            DashEnd.x -= (transform.position.x - destination.position.x);
            DashEnd.y -= (transform.position.y - destination.position.y);
            StopPoint.x -= 1;
            StopPoint.y += 3;
        }
    }

    // check if two points are close enough
    bool CloseEnough(Vector2 first, Vector2 second)
    {
        if (Vector2.Distance(first, second) < 0.2f) { return true; }
        return false;
    }

}


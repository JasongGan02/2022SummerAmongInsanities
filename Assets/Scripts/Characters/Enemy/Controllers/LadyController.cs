using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadyController : EnemyController, IRangedAttacker
{
    public float AttackRange => currentStats.attackRange; // Implementing the IRangedAttacker interface
    public ProjectileObject ProjectileObject =>  ((LadyObject)characterObject).projectileObject;

    private Transform startPosition;


    private GameObject arrow;
    public new Animator animator;
    
    private float nextFire; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire


    public new GameObject target;

    public float patroltime = 0f;
    public float patrolRest = 2f;
    private bool patrolToRight = false;

    public Transform groundCheckLeft;
    public Transform groundCheckRight;
    LayerMask ground_mask;

    new void Start()
    {
        startPosition = transform;
        animator = GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    protected override string IdleAnimationState { get; }
    protected override string AttackAnimationState { get; }
    protected override string MoveAnimationState { get; }

    protected override void Awake()
    {
        base.Awake();
        ground_mask = LayerMask.GetMask("ground");
        groundCheckLeft = transform.Find("groundCheckLeft");
        groundCheckCenter = transform.Find("groundCheckCenter");
        groundCheckRight = transform.Find("groundCheckRight");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");
        damageDisplay = gameObject.AddComponent<DamageDisplay>();
    }


    // Implement the FireProjectile method as specified by the IRangedAttacker interface
    public void FireProjectiles(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null.");
            return;
        }

        // Instantiate the arrow
        GameObject projectileGameObject = PoolManager.Instance.Get(ProjectileObject);
        projectileGameObject.transform.position = startPosition.position;
        projectileGameObject.transform.SetParent(transform, true);
        ProjectileController projectileControllerComponent = projectileGameObject.GetComponent<ProjectileController>();

        if (projectileControllerComponent != null)
        {
            // Initialize and launch the projectile
            projectileControllerComponent.Initialize(this, ProjectileObject);
            projectileControllerComponent.Launch(target, startPosition);
        }
        else
        {
            Debug.LogError("Projectile component not found on arrow prefab.");
        }
    }


    protected override void UpdateEnemyBehavior() 
    {
        // Escape from the tower
        //
        if (Core == null){Core = GameObject.Find("CoreArchitecture");}
        // Approaches and escapes from the player

        target = SearchForTargetObject();

        if (rb.linearVelocity.x != 0) SenseFrontBlock();
        if (target == null)
        {
            if (TimeSystemManager.Instance.IsRedMoon){
                Debug.Log("It's a red moon night! The villagers are spooked!");
                approach(currentStats.movingSpeed, Core.transform, currentStats.attackRange - 0.2f);
            }else{
                patrol();
            }
        }
        else
        {
            /*Debug.Log("not patroling");*/
            Vector2 direction = (target.transform.position - transform.position);
            if (direction.x > 0)
            {
                transform.right = Vector2.left;
            }
            else if (direction.x < 0)
            {
                transform.right = Vector2.right;
            }

            approach(currentStats.movingSpeed, target.transform, currentStats.attackRange - 0.4f);   // debug
            
            // Target Taken Damage
            if (arrow != null)
            {
                if (Vector2.Distance(arrow.transform.position, target.transform.position) < 0.3)
                {
                    ApplyDamage(target.GetComponent<CharacterController>());
                    Destroy(arrow);
                }
            }

            if (Vector2.Distance(transform.position, target.transform.position) <= currentStats.attackRange)
            {
                // Check if the archer can fire
                if (canFire)
                {
                    // Debug.Log(target);
                    // Fire an arrow
                    FireProjectiles(target);

                    // Set the next fire time
                    nextFire = Time.time + currentStats.attackInterval;

                    // Set the canFire flag to false
                    canFire = false;
                    animator.SetBool("canFire", false);
                }

                // Check if the fire rate has passed
                if (Time.time > nextFire)
                {
                    // Set the canFire flag to true
                    canFire = true;
                    animator.SetBool("canFire", true);

                }
            }

        }
        
    }

    private void patrol()
    {
        if (patroltime <= 0f)
        {
            animator.SetFloat("movingSpeed", 0);
            patrolRest = 1.5f;
            patroltime = Random.Range(1f, 3f);
            if (Random.Range(0f, 1f) < 0.5) // go left
            {
                patrolToRight = false;
            }
            else                          // go right
            {
                patrolToRight = true;
            }
        }
        else if (patrolRest > 0)
        {
            patrolRest -= Time.deltaTime;
        }
        else
        {
            animator.SetFloat("movingSpeed", 1f);
            patroltime -= Time.deltaTime;
            if (patrolToRight)
            {
                if (MoveForwardDepthCheck() == true)
                {
                    rb.linearVelocity = new Vector2(currentStats.movingSpeed, rb.linearVelocity.y);
                    if (!facingRight) { Flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck() == true)
                {
                    rb.linearVelocity = new Vector2(-currentStats.movingSpeed, rb.linearVelocity.y);
                    if (facingRight) { Flip(); }
                }
            }
        }
    }

    
    void approach(float speed, Transform target, float distance)
    {
        float currentDistance = Mathf.Abs(transform.position.x - target.position.x);
        //Debug.Log("currentDistance" + currentDistance);
        if (currentDistance < distance)
        {
            if (RetreatDepthCheck() == true) // check if there isn't abyss on the back
            {
                if (target.position.x > transform.position.x)
                {
                    rb.linearVelocity = new Vector2(-1f * speed, rb.linearVelocity.y); animator.SetFloat("movingSpeed", 1f);
                    //Debug.Log("going Left");
                }
                else
                {
                    rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y); animator.SetFloat("movingSpeed", 1f);
                    //Debug.Log("going Right");
                }
            }
        }
        else { animator.SetFloat("movingSpeed", 0f); }
    }
    
    private bool RetreatDepthCheck() // when retreat from Player, don't go to abyss
    {
        Vector2 backDepthDetector = new Vector2();
        if (facingRight)
        {
            backDepthDetector = new Vector2(backCheck.position.x - 0.35f, frontCheck.position.y);
        }
        else
        {
            backDepthDetector = new Vector2(backCheck.position.x + 0.35f, frontCheck.position.y);
        }
        
        RaycastHit2D hit = Physics2D.Raycast(backDepthDetector, Vector2.down, 3f, ground_mask);
        if (hit.collider != null) { return true; }
        return false;
    }
}

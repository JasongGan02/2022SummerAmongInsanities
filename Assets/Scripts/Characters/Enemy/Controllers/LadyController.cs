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
    private float nextMove; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire
    private bool canMove = true;


    private float nextJump;
    private bool canJump = true;
    private bool isJump = false;
    private float distance;

    public new GameObject target;

    public float patroltime = 0f;
    public float patrolRest = 2f;
    private bool patrolToRight = false;
    private bool facingright = false;

    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public Transform frontCheck;
    public Transform backCheck;
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

        // Approaches and escapes from the player

        target = SearchForTargetObject();

        if (rb.velocity.x != 0) SenseFrontBlock();
        if (target == null)
        {
            patrol();
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

            if (Vector2.Distance(transform.position, target.transform.position) <= currentStats.attackRange && lady_sight())
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
                    rb.velocity = new Vector2(currentStats.movingSpeed, rb.velocity.y);
                    if (!facingright) { flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck() == true)
                {
                    rb.velocity = new Vector2(-currentStats.movingSpeed, rb.velocity.y);
                    if (facingright) { flip(); }
                }
            }
        }
    }

    void flip()
    {
        if (facingright)
        {
            facingright = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            facingright = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    private bool lady_sight()
    {
        Rigidbody2D targetRB = target.GetComponent<Rigidbody2D>();
        Vector2 targetTop = targetRB.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerTop = rb.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 targetBottom = targetRB.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerBottom = rb.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;

        Debug.DrawRay(targetTop, villagerTop - targetTop, Color.red);   // top
        Debug.DrawRay(targetBottom, villagerBottom - targetBottom, Color.red);   // bottom

        float distance1 = Vector2.Distance(targetTop, villagerTop);
        float distance2 = Vector2.Distance(targetBottom, villagerBottom);

        RaycastHit2D checkTop = Physics2D.Raycast(targetTop, villagerTop - targetTop, distance1, ground_mask);
        RaycastHit2D checkBottom = Physics2D.Raycast(targetBottom, villagerBottom - targetBottom, distance2, ground_mask);
        if (checkTop.collider != null &&
            checkBottom.collider != null &&
            checkTop.collider.gameObject.CompareTag("ground") &&
            checkBottom.collider.gameObject.CompareTag("ground"))
        {
            //Debug.Log("there is ground block");
            return false;
        }
        return true;
    }
    void SenseFrontBlock()
    {
        if (MoveForwardDepthCheck() == false) { return; }
        headCheck();
        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, ground_mask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, ground_mask);

        Debug.DrawRay(frontCheck.position, Vector2.left * 0.05f, Color.red); // Debug statement to display the raycast
        Debug.DrawRay(backCheck.position, Vector2.right * 0.05f, Color.red); // Debug statement to display the raycast

        if (hitCenter.transform != null)
        {
            if ((facingright && rb.velocity.x > 0) || (!facingright && rb.velocity.x < 0))
            {
                if (hitFront.transform != null)
                {
                    if (headCheck())
                    {
                        Jump();
                    }
                }
            }
            else if ((facingright && rb.velocity.x < 0) || (!facingright && rb.velocity.x > 0))
            {
                if (hitBack.transform != null)
                {
                    if (headCheck())
                    {
                        Jump();
                    }
                }
            }
        }
        //else { Debug.Log("foot in the air"); }
    }
    public bool headCheck()
    {
        Vector3 direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position + new Vector3(0, -0.1f, 0);
        RaycastHit2D headRay = Physics2D.Raycast(origin, direction, 0.3f, ground_mask);
        Debug.DrawRay(origin, direction * 0.3f, Color.red);        // bottom right
        if (headRay.collider != null && headRay.collider.gameObject.tag == "ground")
        {
            return false;
        }

        return true;
    }
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x * 1.0f, currentStats.jumpForce);
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
                    rb.velocity = new Vector2(-1f * speed, rb.velocity.y); animator.SetFloat("movingSpeed", 1f);
                    //Debug.Log("going Left");
                }
                else
                {
                    rb.velocity = new Vector2(speed, rb.velocity.y); animator.SetFloat("movingSpeed", 1f);
                    //Debug.Log("going Right");
                }
            }
        }
        else { animator.SetFloat("movingSpeed", 0f); }
    }

    protected override void MoveTowards(Transform targetTransform)
    {
        Vector2 direction = (targetTransform.position - transform.position).normalized;
        rb.velocity = direction * currentStats.movingSpeed;
    }
    private bool MoveForwardDepthCheck() // when walking forward, don't go to abyss
    {
        Vector2 frontDepthDetector = new Vector2(frontCheck.position.x + 0.35f, frontCheck.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontDepthDetector, Vector2.down, 3f, ground_mask);
        if (hit.collider != null) { return true; }
        return false;
    }
    private bool RetreatDepthCheck() // when retreat from Player, don't go to abyss
    {
        Vector2 backDepthDetector = new Vector2();
        if (facingright)
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

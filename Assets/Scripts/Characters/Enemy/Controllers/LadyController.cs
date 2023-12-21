using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadyController : EnemyController, IRangedAttacker
{
    protected WeaponObject weaponObject;
    public float AttackRange => _atkRange; // Implementing the IRangedAttacker interface
    public WeaponObject WeaponObject => weaponObject;

    private Transform startPosition;




    private GameObject arrow;
    public Animator animator;
    
    private float nextFire; // the time at which the archer can fire again
    private float nextMove; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire
    private bool canMove = true;


    private Rigidbody2D rb;
    private float nextJump;
    private bool canJump = true;
    private bool isJump = false;
    private float distance;

    public GameObject target;

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

    void Start()
    {
        startPosition = transform;
        animator = GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    new void Awake()
    {
        ground_mask = LayerMask.GetMask("ground");
        groundCheckLeft = transform.Find("groundCheckLeft");
        groundCheckCenter = transform.Find("groundCheckCenter");
        groundCheckRight = transform.Find("groundCheckRight");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");
    }


    // Implement the FireProjectile method as specified by the IRangedAttacker interface
    public void FireProjectile(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null.");
            return;
        }

        // Instantiate the arrow
        GameObject projectileObject = ProjectilePoolManager.Instance.GetProjectile(weaponObject.getPrefab());
        projectileObject.transform.position = startPosition.position;
        projectileObject.transform.SetParent(transform, true);
        Projectile projectileComponent = projectileObject.GetComponent<Projectile>();

        if (projectileComponent != null)
        {
            // Initialize and launch the projectile
            projectileComponent.Initialize(this, WeaponObject);
            projectileComponent.Launch(target, startPosition);
        }
        else
        {
            Debug.LogError("Projectile component not found on arrow prefab.");
        }
    }


    protected override void EnemyLoop() 
    {
        // Escape from the tower
        //

        // Approaches and escapes from the player

        target = WhatToAttack();
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

            SenseFrontBlock();
            distance = Mathf.Abs(transform.position.x - target.transform.position.x);
            approach(_movingSpeed, target.transform, distance);
            
            // Target Taken Damage
            if (arrow != null)
            {
                if (Vector2.Distance(arrow.transform.position, target.transform.position) < 0.3)
                {
                    target.GetComponent<CharacterController>().takenDamage(AtkDamage);
                    Destroy(arrow);
                }
            }

            if (Vector2.Distance(transform.position, target.transform.position) <= _atkRange && lady_sight())
            {
                // Check if the archer can fire
                if (canFire)
                {
                    // Fire an arrow
                    FireProjectile(target);

                    // Set the next fire time
                    nextFire = Time.time + _atkSpeed;

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
                rb.velocity = new Vector2(_movingSpeed, rb.velocity.y);
                if (!facingright) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(-_movingSpeed, rb.velocity.y);
                if (facingright) { flip(); }
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
    new void SenseFrontBlock()
    {
        headCheck();
        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, ground_mask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, ground_mask);

        Debug.DrawRay(frontCheck.position, Vector2.left * 0.05f, Color.red); // Debug statement to display the raycast
        Debug.DrawRay(backCheck.position, Vector2.right * 0.05f, Color.red); // Debug statement to display the raycast

        if (hitLeft.transform != null
            || hitRight.transform != null
            || hitCenter.transform != null)
        {
            if (hitFront.transform != null || hitBack.transform != null)
            {
                if (headCheck())
                {
                    Jump();
                    Debug.Log("jumping.");
                }
                //else { Debug.Log("front or back obstacle too high!"); }
            }
            //else { Debug.Log("no obstacle in front or back"); }
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
        Vector2 jumpForce = new Vector2(rb.velocity.x, _jumpForce);
        rb.AddForce(jumpForce, ForceMode2D.Impulse);
    }
    void approach(float speed, Transform target, float distance)
    {
        if (distance < 0.5f * _atkRange)
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
        else { animator.SetFloat("movingSpeed", 0f); }
    }
}

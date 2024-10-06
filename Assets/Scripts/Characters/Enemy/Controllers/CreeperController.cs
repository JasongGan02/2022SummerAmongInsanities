using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CreeperController : EnemyController
{
    bool rest = false;
    bool facingright = false;
    float patroltime = 0f;
    private Animator animator;
    bool patrolToRight = true;
    float patrolRest = 2f;
    GameObject target;

    public Transform groundCheckCenter;
    public Transform frontCheck;
    public Transform backCheck;
    LayerMask ground_mask;
    LayerMask enemy_mask;
    LayerMask flyEnemy_mask;

    private CircleCollider2D Collider;

    private float Wait = 0.3f;
    bool isAttacking = false;
    bool booming = false;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        ground_mask = LayerMask.GetMask("ground");
        enemy_mask = LayerMask.GetMask("enemy");
        flyEnemy_mask = LayerMask.GetMask("flyenemy");
        groundCheckCenter = transform.Find("groundCheckCenter");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");
        Collider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void UpdateEnemyBehavior()
    {
        if (!booming) { 
            SenseFrontBlock();

            target = SearchForTargetObject();
            if (target == null) { patrol(); }
            else
            {
                if (villager_sight())
                {
                    if (DistanceToTarget(target.transform) < currentStats.attackRange && !isAttacking)
                    {
                        attack(target.transform, currentStats.attackInterval); // default:1;  lower -> faster
                    }
                    else if (!booming)
                    {
                        approach(2.0f * currentStats.movingSpeed, target.transform);
                        flip(target.transform);
                    }
                }
            }
        }
    }

    void attack(Transform target, float waitingTime)
    {
        if (rest)
        {
            if (Wait > 0)
            {
                Wait -= Time.deltaTime;
            }
            else
            {
                rest = false;
            }
        }

        else if (!isAttacking)
        {
            isAttacking = true;
            animator.Play("creeper_preBoom");
            //ChangeCollider("creeper_preBoom");
            StartCoroutine(WaitAndContinue(waitingTime));
        }

        flip(target);
    }

    private IEnumerator WaitAndContinue(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        booming = true;
        animator.Play("creeper_boom");
        //ChangeCollider("creeper_boom");
        yield return new WaitForSeconds(0.3f);
        float checkD = Vector2.Distance(transform.position, player.transform.position);
        if (checkD < currentStats.attackRange) // hurt target successfully
        {
            ApplyDamage(target.GetComponent<CharacterController>());
            Vector2 direction = player.transform.position - transform.position;
            direction.Normalize();
            player.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(2f * direction.x * currentStats.jumpForce, 2f * direction.y * currentStats.jumpForce); // effect on player
            rest = true;
            Wait = 1.0f * currentStats.attackInterval;
        }
        else // didn't hurt target
        {
            rest = true;
            Wait = 1.0f * currentStats.attackInterval;
        }
        BreakSurrounding(currentStats.attackRange, currentStats.attackDamage);
        animator.Play("creeper_posBoom");
        //ChangeCollider("creeper_posBoom");
        yield return new WaitForSeconds(0.8f);
        Die();
    }

    void BreakSurrounding(float range, float Damage)
    {
        int numberOfDirections = 30; // Number of directions to cast rays in (one for each degree in a circle)
        float angleStep = 360.0f / numberOfDirections; // Calculate the angle step based on the number of directions

        // Iterate over each direction based on the number of directions and angle step
        for (int i = 0; i < numberOfDirections; i++)
        {
            float angleInRadians = (angleStep * i) * Mathf.Deg2Rad; // Convert current angle to radians
                                                                    // Calculate the direction vector based on the current angle
            Vector2 dir = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            // Cast a ray in the current direction for the specified range
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, range, ground_mask);
            RaycastHit2D[] enemyHits = Physics2D.RaycastAll(transform.position, dir, range, enemy_mask);
            RaycastHit2D[] flyEnemyHits = Physics2D.RaycastAll(transform.position, dir, range, flyEnemy_mask);

            // Iterate over each hit in the current direction
            foreach (RaycastHit2D hit in hits)
            {
                var breakable = hit.transform.GetComponent<IDamageable>();
                if (breakable != null)
                {
                    ApplyDamage(breakable);
                }
            }
            
            foreach (RaycastHit2D hit in enemyHits)
            {
                var enemyController = hit.transform.GetComponent<CharacterController>();
                if (enemyController != null)
                {
                    ApplyDamage(enemyController);
                    //Debug.Log("Damaging Enemy");
                }
            }

            foreach (RaycastHit2D hit in flyEnemyHits)
            {
                var enemyController = hit.transform.GetComponent<CharacterController>();
                if (enemyController != null)
                {
                    ApplyDamage(enemyController);
                    //Debug.Log("Damaging Fly Enemy");
                }
            }
        }
    }

    void approach(float speed, Transform target)
    {
        if (target.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
    }

    protected override void MoveTowards(Transform targetTransform)
    {
        Vector2 direction = (targetTransform.position - transform.position).normalized;
        rb.velocity = direction * currentStats.movingSpeed;
    }
    void patrol()
    {
        if (patroltime <= 0f)
        {
            patrolRest = 2f;
            animator.Play("creeper_idle");
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
            animator.Play("creeper_idle");
            patroltime -= Time.deltaTime;
            if (patrolToRight)
            {
                rb.velocity = new Vector2(currentStats.movingSpeed, rb.velocity.y);
                if (!facingright) { flip(); }
            }
            else
            {
                rb.velocity = new Vector2(-currentStats.movingSpeed, rb.velocity.y);
                if (facingright) { flip(); }
            }
        }
    }
    void flip(Transform target)
    {
        if (target.position.x >= transform.position.x && !facingright)
        {
            facingright = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (target.position.x < transform.position.x && facingright)
        {
            facingright = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
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
    new void SenseFrontBlock()
    {
        headCheck();
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, ground_mask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, ground_mask);

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
    }
    bool headCheck()
    {
        Vector3 direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position + new Vector3(0, -0.2f, 0);
        RaycastHit2D headRay = Physics2D.Raycast(origin, direction, 0.34f, ground_mask);
        Debug.DrawRay(origin, direction * 0.34f, Color.red);        // bottom right
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
    private bool villager_sight()
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

    void ChangeCollider(string status)
    {
        float horizontal = transform.position.x;
        float vertical = transform.position.y;
        if (status == "creeper_idle")
        {
            Collider.offset = new Vector2 { x = -0.2630091f + horizontal, y = -3.62576f + vertical };
            Collider.radius = 9.307809f;
        }
        else if (status == "creeper_preBoom")
        {
            Collider.offset = new Vector2 { x = -0.05069017f + horizontal, y = -2.169847f + vertical };
            Collider.radius = 8.97416f;
        }
        else if (status == "creeper_boom")
        {
            //Collider.offset = new Vector2 { x = 0.8289242f + horizontal, y = -0.1376402f + vertical };
            //Collider.radius = 5.759026f;
            Collider.radius = 0.1f;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }
        else if (status == "creeper_posBoom")
        {
            Collider.radius = 0f;
            transform.localScale = new Vector3(0.4f, 0.4f, transform.localScale.z);
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        }
    }
}

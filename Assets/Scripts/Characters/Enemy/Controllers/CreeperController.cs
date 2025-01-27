using System.Collections;
using UnityEngine;


public class CreeperController : EnemyController
{
    bool rest = false;
    
    float patroltime = 0f;
    private new Animator animator;
    bool patrolToRight = true;
    float patrolRest = 2f;
    private float _targetTicker = 1f;
    new GameObject target;
    LayerMask ground_mask;
    LayerMask enemy_mask;
    LayerMask flyEnemy_mask;

    private CircleCollider2D Collider;

    private float Wait = 0.3f;
    new bool isAttacking = false;
    bool booming = false;

    protected override string IdleAnimationState { get; }
    protected override string AttackAnimationState { get; }
    protected override string MoveAnimationState { get; }
    public float defaultTargetTicker = 1f;

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

            // Periodically re-check for target
            if (target == null || _targetTicker < 0)  
            { 
                target = SearchForTargetObject(); 
                // Debug.Log("trying find target");
                _targetTicker = defaultTargetTicker; 
            }

            if (target == null) { patrol(); }
            else
            {
                // Debug.Log("target is " + target);
                if (HoriDistanceToTarget(target.transform) < currentStats.attackRange && !isAttacking)
                {
                    attack(target.transform, currentStats.attackInterval); // default:1;  lower -> faster
                }
                else if (HoriDistanceToTarget(target.transform) < enemyStats.sensingRange)
                {
                    approach(2.0f * currentStats.movingSpeed, target.transform);
                    flip(target.transform);
                }
                
            }
        }
        // Update tickers
        _targetTicker -= Time.deltaTime;
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
        if (DistanceToTarget(target.transform) < 0.5f * currentStats.attackRange)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else if (target.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
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
                if (!facingRight) { Flip(); }
            }
            else
            {
                rb.velocity = new Vector2(-currentStats.movingSpeed, rb.velocity.y);
                if (facingRight) { Flip(); }
            }
        }
    }
    void flip(Transform target)
    {
        if (target.position.x >= transform.position.x && !facingRight)
        {
            facingRight = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (target.position.x < transform.position.x && facingRight)
        {
            facingRight = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
    
    
}

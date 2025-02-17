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
    LayerMask combinedMask;
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
        combinedMask = LayerMask.GetMask("ground", "enemy", "flyenemy");
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
            player.transform.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(2f * direction.x * currentStats.jumpForce, 2f * direction.y * currentStats.jumpForce); // effect on player
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, combinedMask);
    
        foreach (Collider2D hit in hits)
        {
            // Apply damage based on component
            var damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                ApplyDamage(damageable);
            }
        }
    }

    void approach(float speed, Transform target)
    {
        if (DistanceToTarget(target.transform) < 0.5f * currentStats.attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (target.position.x > transform.position.x) { rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y); }
        else { rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y); }
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
                rb.linearVelocity = new Vector2(currentStats.movingSpeed, rb.linearVelocity.y);
                if (!facingRight) { Flip(); }
            }
            else
            {
                rb.linearVelocity = new Vector2(-currentStats.movingSpeed, rb.linearVelocity.y);
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

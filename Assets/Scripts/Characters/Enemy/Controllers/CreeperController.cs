using System.Collections;
using UnityEngine;


public class CreeperController : EnemyController
{
    bool Rest = false;
    
    float PatrolTime = 0f;
    private new Animator animator;
    bool PatrolToRight = true;
    float PatrolRest = 2f;
    private float TargetTicker = 1f;
    // new GameObject target;
    LayerMask combinedMask;

    private float Wait = 0.3f;
    private bool IsAttacking = false;
    private bool Booming = false;

    protected override string IdleAnimationState { get; }
    protected override string AttackAnimationState { get; }
    protected override string MoveAnimationState { get; }
    public float DefaultTargetTicker = 1f;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        combinedMask = LayerMask.GetMask("ground", "enemy", "flyenemy");
        groundCheckCenter = transform.Find("groundCheckCenter");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void UpdateEnemyBehavior()
    {
        if (!Booming) { 
            SenseFrontBlock();

            // Periodically re-check for target
            if (target == null || TargetTicker < 0)  
            { 
                target = SearchForTargetObject(); 
                // Debug.Log("trying find target");
                TargetTicker = DefaultTargetTicker; 
            }

            if (target == null) { 
                if (TimeSystemManager.Instance.IsRedMoon){
                    Debug.Log("It's a red moon night! The creepers approach core!");
                    Approach(currentStats.movingSpeed, corePosition);
                }else{
                    patrol();
                }
            }
            else
            {
                // Debug.Log("target is " + target);
                if (HoriDistanceToTarget(target.transform) < currentStats.attackRange && !IsAttacking)
                {
                    Attack(target.transform, currentStats.attackInterval); // default:1;  lower -> faster
                }
                else if (HoriDistanceToTarget(target.transform) < enemyStats.sensingRange)
                {
                    Approach(2.0f * currentStats.movingSpeed, target.transform.position);
                    Flip(target.transform);
                }
                
            }
        }
        // Update tickers
        TargetTicker -= Time.deltaTime;
    }

    void Attack(Transform target, float waitingTime)
    {
        if (Rest)
        {
            if (Wait > 0)
            {
                Wait -= Time.deltaTime;
            }
            else
            {
                Rest = false;
            }
        }

        else if (!IsAttacking)
        {
            IsAttacking = true;
            animator.Play("creeper_preBoom");
            //ChangeCollider("creeper_preBoom");
            StartCoroutine(WaitAndContinue(waitingTime));
        }

        Flip(target);
    }

    private IEnumerator WaitAndContinue(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Booming = true;
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
            Rest = true;
            Wait = 1.0f * currentStats.attackInterval;
        }
        else // didn't hurt target
        {
            Rest = true;
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

    private new void Approach(float speed, Vector2 target)
    {
        if (DistanceToTarget(target) < 0.5f * currentStats.attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (target.x > transform.position.x) { rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y); }
        else { rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y); }
    }
    
    void patrol()
    {
        if (PatrolTime <= 0f)
        {
            PatrolRest = 2f;
            animator.Play("creeper_idle");
            PatrolTime = Random.Range(1f, 3f);
            if (Random.Range(0f, 1f) < 0.5) // go left
            {
                PatrolToRight = false;
            }
            else                          // go right
            {
                PatrolToRight = true;
            }
        }
        else if (PatrolRest > 0)
        {
            PatrolRest -= Time.deltaTime;
        }
        else
        {
            animator.Play("creeper_idle");
            PatrolTime -= Time.deltaTime;
            if (PatrolToRight)
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
    
    
}

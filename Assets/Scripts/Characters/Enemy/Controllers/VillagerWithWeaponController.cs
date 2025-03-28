using System.Collections.Generic;
using UnityEngine;

public class VillagerWithWeaponController : EnemyController
{
    #region Fields and Variables

    private bool IsResting = false;
    private bool PatrolToRight = true;

    private float PatrolTime = 0f;
    private float PatrolRest = 2f;
    private float AttackingAnimationTimer = 0f;
    private float DamageStartTime = 0.17f;
    private float Wait = 0.3f;

    private float TargetTicker = 1f;
    private float PathTicker = 3f;

    private Animator _animator;
    // private BoxCollider2D _boxCollider;
    private CapsuleCollider2D CapsuleCollider;

    [Header("Check Transforms")]
    public Transform groundCheckLeft;
    public Transform groundCheckRight;
    public Transform attackStart;
    public Transform attackEnd;
    public Transform head;

    [Header("Timers / Cooldowns")]
    // Expose these for balancing; adjust as needed
    public float defaultChasingRemainder = 5f;
    public float defaultPathTicker = 2f;
    public float defaultTargetTicker = 1f;
    public float attackAnimationDuration = 0.25f;

    #endregion

    protected override string IdleAnimationState => "villagerWithWeapon_idle";
    protected override string AttackAnimationState => "villagerWithWeapon_attack";
    protected override string MoveAnimationState => "villagerWithWeapon_walk";

    #region Start Up

    protected override void Awake()
    {
        base.Awake();

        // Cache references
        _animator = GetComponent<Animator>();
        CapsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Automatically find child transform references if not manually assigned
        groundCheckLeft = groundCheckLeft ?? transform.Find("groundCheckLeft");
        groundCheckCenter = groundCheckCenter ?? transform.Find("groundCheckCenter");
        groundCheckRight = groundCheckRight ?? transform.Find("groundCheckRight");
        frontCheck = frontCheck ?? transform.Find("frontCheck");
        backCheck = backCheck ?? transform.Find("backCheck");
        attackStart = attackStart ?? transform.Find("attackStart");
        attackEnd = attackEnd ?? transform.Find("attackEnd");
        head = head ?? transform.Find("head");
    }

    #endregion

    #region Behavior Updates

    /// <summary>
    /// This is the high-level Behavior update. 
    /// Called from EnemyController - runs every frame or on fixed intervals if you want (depending on the architecture).
    /// </summary>
    protected override void UpdateEnemyBehavior()
    {
        // Periodically re-check for target
        if (target == null || TargetTicker < 0)
        {
            target = SearchForTargetObject();
            // Debug.Log("trying find target");
            TargetTicker = defaultTargetTicker;
        }

        // If no target in sight or memory
        if (target == null && TargetRemainder == null)
        {
            PathToTarget.Clear();
            RemovePathLine();
            if (TimeSystemManager.Instance.IsRedMoon)
            {
                Debug.Log("It's a red moon night! The weapon villagers approach core");
                Approach(currentStats.movingSpeed, corePosition);
            }
            else
            {
                Patrol();
            }
        }
        // If no target but we still remember the last known position
        else if (target == null && TargetRemainder != null)
        {
            FinishExistingPath();
            ChasingRemainder -= Time.deltaTime;
        }
        // If we have a target
        else
        {
            // Update the last known target position
            TargetRemainder = target.transform;
            ChasingRemainder = defaultChasingRemainder;

            // If path is empty or our pathTicker has run out, re-pathfind
            if (PathToTarget.Count == 0 || PathTicker < 0)
            {
                PathToTarget.Clear();
                PathFind();
                PathTicker = defaultPathTicker;
            }
            else
            {
                PathExecute();
            }

            // If close enough, try to attack
            if (DistanceToTarget(target.transform) < currentStats.attackRange ||
                target.transform.GetComponent<BreakableObjectController>() != null)
            {
                AttackHandler(target.transform, enemyStats.attackInterval);
            }

            // Additional behavior: Shake player overhead if they are above or below
            ShakePlayerOverHead();
        }

        // Update tickers
        PathTicker -= Time.deltaTime;
        TargetTicker -= Time.deltaTime;
    }

    #endregion

    #region Patrol / Movement

    /// <summary>
    /// Idle/Walk randomly for a short while.
    /// </summary>
    private void Patrol()
    {
        RemovePathLine();

        if (PatrolTime <= 0f)
        {
            // Prepare for a new patrol cycle
            PatrolRest = 2f;
            _animator.Play("villagerWithWeapon_idle");
            PatrolTime = UnityEngine.Random.Range(1f, 3f);

            PatrolToRight = (UnityEngine.Random.Range(0f, 1f) >= 0.5f);
        }
        else if (PatrolRest > 0)
        {
            // Rest for a moment in idle
            PatrolRest -= Time.deltaTime;
        }
        else
        {
            // Actually move
            _animator.Play("villagerWithWeapon_walk");
            SenseFrontBlock();
            if (!MoveForwardDepthCheck()) return;

            PatrolTime -= Time.deltaTime;

            // Move either left or right
            if (PatrolToRight)
            {
                rb.linearVelocity = new Vector2(currentStats.movingSpeed, rb.linearVelocity.y);
                if (!facingRight) Flip();
            }
            else
            {
                rb.linearVelocity = new Vector2(-currentStats.movingSpeed, rb.linearVelocity.y);
                if (facingRight) Flip();
            }
        }
    }

    /// <summary>
    /// Simple approach function. Adjusts orientation and sets velocity.
    /// </summary>
    private new void Approach(float speed, Vector2 targetTransform)
    {
        if (speed > enemyStats.movingSpeed)
        {
            _animator.Play("villagerWithWeapon_run");
        }
        else
        {
            _animator.Play("villagerWithWeapon_walk");
        }

        // Face the target
        if ((facingRight && targetTransform.x < transform.position.x)
            || (!facingRight && targetTransform.x > transform.position.x))
        {
            Flip();
        }

        // Move left or right
        if (targetTransform.x > transform.position.x)
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
        }
    }

    #endregion

    #region Attacking

    /// <summary>
    /// Manages the full attack sequence.
    /// </summary>
    /// <param name="targetTransform">Transform of target</param>
    /// <param name="attackFrequency">How quickly the next attack can happen</param>
    private void AttackHandler(Transform targetTransform, float attackFrequency)
    {
        if (!IsResting && AttackingAnimationTimer <= 0)
        {
            // Start the attack
            _animator.Play("villagerWithWeapon_attack");
            AttackingAnimationTimer = attackAnimationDuration;
        }
        else if (AttackingAnimationTimer > 0)
        {
            // Wait for the animation to reach damage frame
            AttackingAnimationTimer -= Time.deltaTime;

            // Check if time is within the damage window
            float current = AttackingAnimationTimer;
            float damageWindowCenter = attackAnimationDuration - DamageStartTime;

            if (current < (damageWindowCenter + 0.03f) && current > (damageWindowCenter - 0.01f))
            {
                var breakable = targetTransform.GetComponent<IDamageable>();
                if (breakable != null)
                {
                    // Breaking a destructible object
                    ApplyDamage(breakable);
                    EnterAttackCooldown(attackFrequency);
                }
                else
                {
                    // Possibly a player is within range
                    float checkD = Vector2.Distance(attackEnd.position, targetTransform.position);
                    if (checkD < 0.75f)
                    {
                        var character = targetTransform.GetComponent<CharacterController>();
                        ApplyDamage(character);
                    }

                    EnterAttackCooldown(attackFrequency);
                }
            }
        }
        else if (IsResting)
        {
            // Once the attack is done, rest for cooldown
            if (Wait > 0)
            {
                Wait -= Time.deltaTime;
                _animator.Play("villagerWithWeapon_idle");
            }
            else
            {
                IsResting = false;
            }
        }

        // Flip orientation if needed
        Flip(targetTransform);
    }

    /// <summary>
    /// Helper to set the villager into rest mode and reset the wait timer.
    /// </summary>
    private void EnterAttackCooldown(float frequency)
    {
        IsResting = true;
        AttackingAnimationTimer = 0f;
        Wait = frequency;
    }

    #endregion

    #region Shake Player Overhead

    /// <summary>
    /// Additional behavior that shakes the player overhead if they are basically vertical above or below the villager.
    /// </summary>
    private void ShakePlayerOverHead()
    {
        if (target == null) return;

        Vector2 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bool isVerticalAngle = (angle >= -95f && angle <= -85f) || (angle >= 85 && angle <= 95);
        if (isVerticalAngle && distance < 3f)
        {
            // Debug.Log("Angle and distance conditions met");
            float verticalOffset = Mathf.Abs(target.transform.position.y - transform.position.y);
            if (verticalOffset < 2f)
            {
                Vector2 start = transform.position;
                Vector2 end = target.transform.position;
                RaycastHit2D hit = Physics2D.Raycast(start, direction.normalized, distance, groundLayerMask);

                if (hit.collider == null)
                {
                    // Debug.Log("No obstruction detected. Applying horizontal force.");

                    // Apply random horizontal force
                    float randomForce = (UnityEngine.Random.Range(0f, 1f) <= 0.5f) ? -20f : 20f;
                    rb.linearVelocity = new Vector2(randomForce, rb.linearVelocity.y);
                }
                else
                {
                    // Debug.Log("Obstruction detected. No horizontal force applied.");
                }
            }
        }
    }

    #endregion
}
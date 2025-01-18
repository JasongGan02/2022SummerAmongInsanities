using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BatController : EnemyController
{
    [Header("Bat State")]
    private new bool facingRight = false;
    private float waitTime;
    private Vector2 moveTo;
    private bool planned;
    private bool prepareDash;
    private bool isDashing;
    private Vector2 dashEnd;
    private Vector2 stopPoint;
    private bool attacked;
    private float attackSpeed;

    [Header("References")]
    private new Animator animator;
    private TrailRenderer trailRenderer;
    private new ParticleSystem particleSystem;
    private new GameObject target;

    [Header("Timings")]
    private float batTimer;

    [Header("Positions")]
    private float startX;
    private float startY;
    private Transform groupApproachTarget;

    // Make sure to fill in the correct animation state names if needed
    protected override string IdleAnimationState { get; }
    protected override string AttackAnimationState { get; }
    protected override string MoveAnimationState { get; }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        // Create a temporary GameObject to serve as a patrol destination
        // GameObject destination = new GameObject("BatTempDestination");
        float randX = Random.Range(startX - enemyStats.sensingRange / 2f, startX + enemyStats.sensingRange / 2f);
        float randY = Random.Range(startY - enemyStats.sensingRange / 2f, startY + enemyStats.sensingRange / 2f);
        moveTo = new Vector2(randX, randY);

        // Initialize patrol positions / variables
        waitTime = enemyStats.attackInterval * 2;
        batTimer = 0f;
        attackSpeed = 1f / enemyStats.attackInterval;

        startX = transform.position.x;
        startY = transform.position.y;

        // Random initial patrol point
        moveTo = GetRandomPatrolPoint();

        planned = false;
        prepareDash = true;
        isDashing = false;
        attacked = false;

        stopPoint = Vector2.zero; // placeholder
    }

    private Vector2 GetRandomPatrolPoint()
    {
        float randX = Random.Range(startX - enemyStats.sensingRange/2f, startX + enemyStats.sensingRange/2f);
        float randY = Random.Range(startY - enemyStats.sensingRange/2f, startY + enemyStats.sensingRange/2f);
        return new Vector2(randX, randY);
    }

    protected override void UpdateEnemyBehavior()
    {
        // If no attack plan is currently in place, check for a new target
        if (!planned)
        {
            target = SearchForTargetObject();
        }
        else if (target == null)
        {
            // If our planned target was destroyed, find a new one
            target = SearchForTargetObject();
        }

        // If no target is found, proceed with patrol
        if (target == null)
        {
            HandlePatrol();
        }
        else
        {
            HandleTargetLogic();
        }
    }

    private void HandlePatrol()
    {
        // If currently set to attacking in animator, reset to false
        if (animator.GetBool("is_attacking"))
        {
            animator.SetBool("is_attacking", false);
        }

        // If the trail is emitting, turn it off
        if (trailRenderer.emitting)
        {
            trailRenderer.emitting = false;
        }

        Patrol();
        CheckFlipForMovement(moveTo.x);
    }

    private void HandleTargetLogic()
    {
        float distanceToTarget = Vector2.Distance(target.transform.position, transform.position);

        // If target within sensing range or already locked in attack plan
        if (distanceToTarget < enemyStats.sensingRange || planned)
        {
            // If target within attack range or an attack is planned
            if (distanceToTarget < currentStats.attackRange || planned)
            {
                DashAttack(target.transform);
            }
            else
            {
                ApproachingTarget(target.transform, enemyStats.movingSpeed * attackSpeed);
            }
        }
        else
        {
            // Target is too far, go back to patrol
            Patrol();
            CheckFlipForMovement(moveTo.x);
        }
    }

    private void Patrol()
    {
        if (IsGroupAttacking)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                groupApproachTarget.position, 
                currentStats.movingSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                moveTo, 
                currentStats.movingSpeed * Time.deltaTime
            );
        }

        // Check if we reached the patrol point or if waitTime is up
        if (Vector2.Distance(transform.position, moveTo) < 0.2f)
        {
            // Once we arrive, we let the enemy "wait" at that spot
            // then pick a new random patrol point
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                moveTo = GetRandomPatrolPoint();
                waitTime = 3 * enemyStats.attackInterval;
            }
        }
        else
        {
            // If not arrived yet, keep decreasing waitTime
            // but do not drive it below zero unless we are close to the patrol point
            waitTime -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Overriding the parent method. Moves the bat toward the target at normal moving speed.
    /// </summary>
    protected new void ApproachingTarget(Transform targetTransform, float speed)
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetTransform.position,
            speed * Time.deltaTime
        );
        CheckFlipForMovement(targetTransform.position.x);
    }

    private void DashAttack(Transform destination)
    {
        if (prepareDash)
        {
            // Play charge-up effect
            CheckFlipForMovement(target.transform.position.x);
            if (!particleSystem.isPlaying) 
            {
                particleSystem.Play();
            }

            // If no plan is set, create a route
            if (!planned)
            {
                PlanRoute(destination);
                planned = true;
            }

            // "Charge up" before dashing
            if (batTimer > 0.8f * enemyStats.attackInterval)
            {
                prepareDash = false;
                isDashing = true;
                // Refresh route before dash in case target moved
                PlanRoute(destination);
            }
            else
            {
                batTimer += Time.deltaTime;
            }
        }
        else if (isDashing)
        {
            // Execute dash
            batTimer = 0f;
            if (particleSystem.isPlaying) 
            {
                particleSystem.Stop();
            }

            transform.position = Vector2.MoveTowards(
                transform.position,
                dashEnd,
                currentStats.movingSpeed * 5f * Time.deltaTime * attackSpeed
            );

            animator.SetBool("is_attacking", true);
            trailRenderer.emitting = true;

            // If close enough to target, apply damage once
            if (Vector2.Distance(transform.position, destination.position) < 0.4f && !attacked)
            {
                var characterCtrl = target.GetComponent<CharacterController>();
                if (characterCtrl != null)
                {
                    ApplyDamage(characterCtrl);
                }
                attacked = true;
            }

            // If we've reached the end of dash
            if (CloseEnough(transform.position, dashEnd))
            {
                animator.SetBool("is_attacking", false);
                trailRenderer.emitting = false;
                isDashing = false;
            }
        }
        else
        {
            // Return to a "higher" point
            if (CloseEnough(transform.position, stopPoint))
            {
                prepareDash = true;
                attacked = false;
                planned = false;
            }
            else
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, 
                    stopPoint, 
                    currentStats.movingSpeed * Time.deltaTime * attackSpeed
                );
            }

            // Check if we need to flip while returning
            CheckFlipForMovement(destination.position.x);
        }
    }

    /// <summary>
    /// Plan the route for the dash. 
    /// The dashEnd is set to the player's position (with offset),
    /// and the stopPoint is set above that position to reset the bat.
    /// </summary>
    private void PlanRoute(Transform destination)
    {
        dashEnd = destination.position;
        stopPoint = destination.position;

        // If player is on the right side of the bat
        if (destination.position.x > transform.position.x)
        {
            dashEnd.x += destination.position.x - transform.position.x;
            dashEnd.y -= transform.position.y - destination.position.y;

            stopPoint.x += Random.Range(enemyStats.sensingRange / 2f + 1, enemyStats.sensingRange + 1);
            stopPoint.y += Random.Range(enemyStats.sensingRange / 2f + 1, enemyStats.sensingRange + 1);
        }
        else
        {
            dashEnd.x -= transform.position.x - destination.position.x;
            dashEnd.y -= transform.position.y - destination.position.y;

            stopPoint.x -= Random.Range(enemyStats.sensingRange / 2f + 1, enemyStats.sensingRange + 1);
            stopPoint.y += Random.Range(enemyStats.sensingRange / 2f + 1, enemyStats.sensingRange + 1);
        }
    }

    /// <summary>
    /// Check whether we should flip our facing direction based on a given X position.
    /// </summary>
    private void CheckFlipForMovement(float targetX)
    {
        bool shouldFlip =
            (targetX < transform.position.x && facingRight) ||
            (targetX > transform.position.x && !facingRight);

        if (shouldFlip)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Check if two vectors are within a small threshold of each other.
    /// </summary>
    private bool CloseEnough(Vector2 first, Vector2 second)
    {
        return Vector2.Distance(first, second) < 0.2f;
    }

    protected override void MoveTowards(Transform targetTransform)
    {
        groupApproachTarget = targetTransform;
        IsGroupAttacking = true;
    }
}

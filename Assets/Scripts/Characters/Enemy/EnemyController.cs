using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class EnemyController : CharacterController
{
    protected EnemyStats enemyStats => (EnemyStats)currentStats;
    protected GameObject player;
    protected Rigidbody2D rb;
    protected bool facingRight = true;
    
    protected LayerMask groundLayerMask;
    protected LayerMask targetLayerMask;
    
    protected GameObject target;
    private Vector2? lastKnownPosition = null;
    private float lastSeenTimestamp = 0f;
    protected Vector2? LastKnownPosition => lastKnownPosition;
    protected bool HasLastKnownPosition => lastKnownPosition.HasValue && (Time.time - lastSeenTimestamp < 5f);
    //Status variables
    public bool IsGroupAttacking { get; set; }
    public bool IsFrozen { get; set; } // Tracks if the enemy is frozen
    
    
    //Animation Properties
    protected abstract string IdleAnimationState { get; }
    protected abstract string AttackAnimationState { get; }
    protected abstract string MoveAnimationState { get; }
    //protected abstract string DeathAnimationState { get; }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        groundLayerMask = LayerMask.GetMask("ground");
        targetLayerMask = LayerMask.GetMask("player", "tower");
    }
    
    protected virtual void Start()
    {
        FindPlayer();
    }
    
    protected override void Update()
    {
        base.Update();
        FindPlayer();
        SetEnemyContainer();
        if (!IsFrozen)
            UpdateEnemyBehavior();
    }
    
    public void LevelUp()
    {
        Reinitialize();
    }

    public void PrintStats()
    {
        Debug.Log(enemyStats.ToString());
    }
    
    public override void TakeDamage(float amount, IDamageSource damageSource)
    {
        base.TakeDamage(amount, damageSource);
        audioEmitter.PlayClipFromCategory("InjureEnemy");
    }
    
    protected override void OnObjectReturned(bool isDestroyedByPlayer)
    {
        base.OnObjectReturned(isDestroyedByPlayer);
        (characterObject as EnemyObject)?.soulObject.GetDroppedSoul(enemyStats.soulValue, transform.position);
    }
    
    protected abstract void UpdateEnemyBehavior();
    
    protected float DistanceToTarget(Transform target)
    {
        return Vector2.Distance(transform.position, target.position);
    }
    
    #region Search For Target
    protected GameObject SearchForTargetObject()
    {
        if (Hatred == null || Hatred.Count == 0)
        {
            Debug.LogError("Hatred list is empty.");
            return null;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, enemyStats.sensingRange, targetLayerMask);

        // List to hold potential targets with their priority and distance
        List<PotentialTarget> potentialTargets = new List<PotentialTarget>();

        foreach (Collider2D collider in colliders)
        {
            var targetComponent = collider.GetComponent<IDamageable>();
            if (targetComponent != null)
            {
                Type targetType = targetComponent.GetType();
                int typePriority = Hatred.FindIndex(hatredType => hatredType == targetType);
                
                // If the type is not directly in the list, check inheritance
                if (typePriority == -1)
                {
                    for (int i = 0; i < Hatred.Count; i++)
                    {
                        Type hatredType = Hatred[i];
                        if (hatredType.IsAssignableFrom(targetType))
                        {
                            typePriority = i;
                            break;
                        }
                    }
                }

                if (typePriority != -1)
                {
                    float distanceSquared = ((Vector2)(collider.transform.position - transform.position)).sqrMagnitude;
                    potentialTargets.Add(new PotentialTarget
                    {
                        GameObject = collider.gameObject,
                        TypePriority = typePriority,
                        DistanceSquared = distanceSquared
                    });
                }
            }
        }

        if (potentialTargets.Count == 0)
        {
            return null;
        }

        // Sort the potential targets first by type priority, then by distance
        potentialTargets.Sort((a, b) =>
        {
            int priorityComparison = a.TypePriority.CompareTo(b.TypePriority);
            if (priorityComparison == 0)
            {
                return a.DistanceSquared.CompareTo(b.DistanceSquared);
            }
            return priorityComparison;
        });

        // Perform line-of-sight check
        foreach (var potentialTarget in potentialTargets)
        {
            if (HasLineOfSightToTarget(potentialTarget.GameObject))
            {
                // Target is visible
                lastSeenTimestamp = Time.time;
                return potentialTarget.GameObject;
            }
        }
        
        // No visible targets; keep last known position if within memory duration
        if (HasLastKnownPosition)
        {
            return null; // No current target, but we have a last known position
        }
        else
        {
            // Reset last known position if memory duration has passed
            lastKnownPosition = null;
            return null;
        }
    }
    
    private Vector2 GetEyePosition()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            float eyeHeight = bounds.max.y - 0.1f; // Adjust 0.1f to place the eye slightly below the top
            return new Vector2(transform.position.x, eyeHeight);
        }
        // Fallback if no collider is found
        return transform.position;
        
    }
    
    private bool HasLineOfSightToTarget(GameObject target)
    {
        Vector2 eyePosition = GetEyePosition();
        Vector2 targetPosition = target.transform.position;

        Vector2 direction = targetPosition - eyePosition;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit2D hit = Physics2D.Raycast(eyePosition, direction, distance, groundLayerMask);
        //Debug.DrawLine(eyePosition, targetPosition, Color.red);
        if (hit.collider == null)
        {
            // Line of sight is clear
            return true;
            
        }
        // Line of sight is obstructed
        return false;
    }
    
    private class PotentialTarget
    {
        public GameObject GameObject { get; set; }
        public int TypePriority { get; set; }
        public float DistanceSquared { get; set; }
    }
    #endregion

    protected abstract void MoveTowards(Transform targetTransform);

    protected virtual void Approach(Vector2? targetPosition, bool isRunning)
    {
        if (targetPosition != null)
        {
            Vector2 direction = (targetPosition.Value - (Vector2) transform.position).normalized;
            direction = new Vector2(direction.x,  rb.velocity.y).normalized;
            float speed = isRunning ? currentStats.movingSpeed : currentStats.movingSpeed / 2f;
            rb.velocity = direction * speed;
            //Debug.Log(speed);
            Flip(direction.x);
            ChangeAnimationState(MoveAnimationState);
        }
        else
        {
            Debug.LogError("targetPosition is null");
        }
    }
    
    protected bool isAttacking;
    protected bool isGrounded;

    protected virtual void Attack(GameObject target)
    {
        if (!(DistanceToTarget(target.transform) < currentStats.attackRange)) return; //没进攻击距离
        if (!isAttacking)
        {
            isAttacking = true;
            if (isGrounded)
            {
                //TODO: 要不就有空中伤害动画要不就没有这段
            }
                    
            animator.speed = currentStats.attackInterval / characterObject.baseStats.attackInterval;
            ChangeAnimationState(AttackAnimationState);
            var damageable = target.GetComponent<IDamageable>();
            ApplyDamage(damageable);
        }
                
        Invoke(nameof(AttackComplete), currentStats.attackInterval);
    }
    
    private void AttackComplete()
    {
        isAttacking = false;
        animator.speed = 1.0f;
        ChangeAnimationState(IdleAnimationState);
    }
    
    protected virtual void Flip(float moveDirection)
    {
        switch (moveDirection)
        {
            case > 0 when !facingRight:
                facingRight = true;
                transform.eulerAngles = new Vector3(0, 0, 0);
                break;
            case < 0 when facingRight:
                facingRight = false;
                transform.eulerAngles = new Vector3(0, 180, 0);
                break;
        }
    }
    
    private void SetEnemyContainer()
    {
        var chunkCoord = WorldGenerator.GetChunkCoordsFromPosition(transform.position);
        if (!WorldGenerator.ActiveChunks.TryGetValue(chunkCoord, out var chunk)) return;
        var enemyContainer = chunk.transform.Find("MobContainer/EnemyContainer");
        if (enemyContainer != null)
        {
            transform.SetParent(enemyContainer, true);
        }
        else
        {
            Debug.LogError("EnemyContainer not found in chunk.");
        }
    }
    private void FindPlayer()
    {
        if (player == null) 
        { 
            player = GameObject.FindWithTag("Player");
        }
    }
    
    /*protected void SenseFrontBlock()    
    {
        Vector3 shooting_direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position - new Vector3(0, 0.5f, 0);

        LayerMask ground_mask = LayerMask.GetMask("ground");

        RaycastHit2D hit = Physics2D.Raycast(origin, shooting_direction, 0.5f, ground_mask);
        Debug.DrawRay(origin, shooting_direction * 0.5f, Color.green); // infront
        Vector3 left = transform.position - new Vector3(-0.3f, 0.2f, 0);
        RaycastHit2D bottomLeft = Physics2D.Raycast(left, Vector3.down, 0.2f, ground_mask);
        Debug.DrawRay(left, Vector3.down * 0.2f, Color.blue);        // bottom left
        Vector3 right = transform.position + new Vector3(0.2f, 0.2f, 0);
        RaycastHit2D bottomRight = Physics2D.Raycast(right, Vector3.down, 0.2f, ground_mask);
        Debug.DrawRay(right, Vector3.down * 0.2f, Color.blue);        // bottom right

        if (hit.collider != null && 
            bottomLeft.collider != null && 
            bottomRight.collider != null)
        {
            //Vector2 up_force = new Vector2(0, JumpForce);
            //gameObject.GetComponent<Rigidbody2D>().AddForce(up_force); 
            //Debug.Log("up_force: " + up_force);
            if (hit.collider.gameObject.tag == "ground" &&
                bottomLeft.collider.gameObject.tag == "ground" &&
                bottomRight.collider.gameObject.tag == "ground")
            {
                Vector2 up_force = new Vector2(0, currentStats.jumpForce);
                Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                rb.AddForce(up_force, ForceMode2D.Impulse);
                //Debug.Log("up_force: " + up_force);
                StartCoroutine(StopJump(rb, 0.5f)); //stop the jump after 0.5 seconds
            }
        }
        
    }
    IEnumerator StopJump(Rigidbody2D rb, float duration)
    {
        yield return new WaitForSeconds(duration);
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;


public abstract class EnemyController : CharacterController
{
    protected EnemyStats enemyStats => (EnemyStats)currentStats;
    protected GameObject player;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected bool facingRight = true;
    
    protected float lastAttackTime = -Mathf.Infinity;
    
    private Vector2? lastKnownPosition = null;
    private float lastSeenTimestamp = 0f;
    
    protected LayerMask groundLayerMask;
    protected LayerMask targetLayerMask;
    
    protected Vector2? LastKnownPosition => lastKnownPosition;
    protected bool HasLastKnownPosition => lastKnownPosition.HasValue && (Time.time - lastSeenTimestamp < 3f);
    public bool IsGroupAttacking { get; set; } = false;
    
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        UpdateEnemyBehavior();
    }
    
    public void LevelUp()
    {
        Reinitialize();
    }
    
    public override void TakeDamage(float amount, IDamageSource damageSource)
    {
        base.TakeDamage(amount, damageSource);
        audioEmitter.PlayClipFromCategory("InjureEnemy");
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

    protected virtual void Approach(Transform targetTransform)
    {
        Vector2 direction = (targetTransform.position - transform.position).normalized;
        rb.velocity = direction * currentStats.movingSpeed;
        Flip(rb.velocity.x);
    }

    protected virtual void Attack(Transform targetTransform, String attackAnimationName)
    {
        if (Time.time >= lastAttackTime + enemyStats.attackInterval)
        {
            lastAttackTime = Time.time;
            
            if (animator != null)
            {
                animator.speed = characterObject.baseStats.attackInterval / enemyStats.attackInterval;
                // Play the attack animation
                animator.Play(attackAnimationName, -1, 0f);
            }

            // Implement attack logic here
            //Debug.Log($"{gameObject.name} is attacking {target.name}");
        }
    }
    
    protected float GetAnimationClipLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0;
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

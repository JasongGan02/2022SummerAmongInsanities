using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Rendering;


public abstract class EnemyController : CharacterController
{
    protected EnemyStats enemyStats => (EnemyStats)currentStats;
    protected GameObject player;
    protected Rigidbody2D rb;
    protected bool facingRight = false;
    public Transform groundCheckCenter;
    public Transform frontCheck;
    public Transform backCheck;
    protected LayerMask groundLayerMask;
    protected LayerMask targetLayerMask;

    public List<Vector2> PathToTarget = new List<Vector2>();
    public int PathCounter;
    public float ChasingRemainder = 5f;
    public Transform TargetRemainder;
    public float inAir = 0f;
    public LineRenderer lineRenderer;
    public GameObject _lineObj; // For creating a line renderer in runtime
    public float _breakObstaclesCD = 1f;
    public float breakObstacleCDReset = 1f;
    public Transform tileDetect1;
    public Transform tileDetect2;
    public Transform tileDetect3;
    public Transform tileDetect4;
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

    protected bool isApproachingCore;
    protected Vector3 corePosition;

    public void ApproachCore(Vector3 corePosition)
    {
        this.corePosition = corePosition;
        isApproachingCore = true;
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        groundLayerMask = LayerMask.GetMask("ground");
        targetLayerMask = LayerMask.GetMask("player", "tower");
        tileDetect1 = tileDetect1 ?? transform.Find("tileDetect1");
        tileDetect2 = tileDetect2 ?? transform.Find("tileDetect2");
        tileDetect3 = tileDetect3 ?? transform.Find("tileDetect3");
        tileDetect4 = tileDetect4 ?? transform.Find("tileDetect4");
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

    protected float DistanceToTarget(Vector3 target)
    {
        return Vector2.Distance(transform.position, target);
    }

    protected float HoriDistanceToTarget(Transform target)
    {
        return Mathf.Abs(transform.position.x - target.position.x);
    }

    protected void SenseFrontBlock()
    {
        if (!MoveForwardDepthCheck()) return;
        HeadCheck();

        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, groundLayerMask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, groundLayerMask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, groundLayerMask);

        // Only do the jump logic if center is on ground
        if (hitCenter.transform != null)
        {
            bool movingForward = (facingRight && rb.linearVelocity.x > 0) || (!facingRight && rb.linearVelocity.x < 0);
            bool movingBackward = (facingRight && rb.linearVelocity.x < 0) || (!facingRight && rb.linearVelocity.x > 0);

            // If blocked in the front
            if (movingForward && hitFront.transform != null)
            {
                if (HeadCheck())
                {
                    Jump();
                }
            }
            // If blocked in the back
            else if (movingBackward && hitBack.transform != null)
            {
                if (HeadCheck())
                {
                    Jump();
                }
            }
        }
    }

    /// <summary>
    /// Prevent running into a big hole or abyss.
    /// </summary>
    protected bool MoveForwardDepthCheck()
    {
        // Slightly forward from frontCheck
        Vector2 frontDepthDetector = new Vector2(frontCheck.position.x + 0.35f, frontCheck.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontDepthDetector, Vector2.down, 3f, groundLayerMask);
        return hit.collider != null;
    }

    protected bool HeadCheck()
    {
        Vector3 direction = transform.TransformDirection(-Vector3.right);
        Vector3 origin = transform.position + new Vector3(0, -0.2f, 0);
        RaycastHit2D headRay = Physics2D.Raycast(origin, direction, 0.34f, groundLayerMask);
        Debug.DrawRay(origin, direction * 0.34f, Color.red); // bottom right
        if (headRay.collider != null && headRay.collider.gameObject.tag == "ground")
        {
            return false;
        }

        return true;
    }

    protected void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 1.0f, currentStats.jumpForce);
    }

    protected void Flip()
    {
        if (facingRight)
        {
            facingRight = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            facingRight = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    /// <summary>
    /// Flip based on a target transform's position.
    /// </summary>
    protected void Flip(Transform targetTransform)
    {
        if (targetTransform.position.x >= transform.position.x && !facingRight)
        {
            facingRight = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (targetTransform.position.x < transform.position.x && facingRight)
        {
            facingRight = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    #region Search For Target

    protected GameObject SearchForTargetObject()
    {
        if (Hatred == null || Hatred.Count == 0)
        {
            // Debug.LogError("Hatred list is empty.");
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
        if ((hit.collider == null && distance < enemyStats.sensingRange) || target.gameObject.name == "CoreArchitecture") // because core position is too low
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

    protected void Approach(float speed, Vector2 targetTransform)
    {
        if (targetTransform != null){
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
    }

    public void Jump(float horizontal)
    {
        // If you want horizontal velocity to carry into jump, uncomment
        // rb.velocity = new Vector2(horizontal, currentStats.jumpForce);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, groundLayerMask);
        if (hitCenter.transform != null)
        {
            rb.linearVelocity = new Vector2(horizontal, enemyStats.jumpForce);
        }
    }

    public bool VillagerCloseToLocation(Vector2 location)
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y - 0.25f);
        return Vector2.Distance(currentLoc, location) < 1.2f;
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

    #region PathFinding

    public void PathFind()
    {
        // Ensure there is a valid target, and we are outside attack range
        if (target == null || DistanceToTarget(target.transform) < currentStats.attackRange)
        {
            PathToTarget.Clear();
            PathCounter = 0;
            return;
        }

        // Get start and end positions
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y - 0.25f); // Adjust for collider height
        Vector2 targetPosition = target.transform.position;

        // Compute bounds with sensing range margins
        int minX = Mathf.FloorToInt(Mathf.Min(currentPosition.x, targetPosition.x) - enemyStats.sensingRange);
        int maxX = Mathf.FloorToInt(Mathf.Max(currentPosition.x, targetPosition.x) + enemyStats.sensingRange);
        int minY = Mathf.FloorToInt(Mathf.Min(currentPosition.y, targetPosition.y) - enemyStats.sensingRange);
        int maxY = Mathf.FloorToInt(Mathf.Max(currentPosition.y, targetPosition.y) + enemyStats.sensingRange);

        // Grid dimensions
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        // Initialize health grid
        int[,] healthGrid = InitializeHealthGrid(minX, maxX, minY, maxY, width, height, currentPosition.y);

        // Convert world coordinates to grid indices
        Vector2Int start = WorldToGridCoords(currentPosition, minX, minY);
        Vector2Int end = WorldToGridCoords(targetPosition, minX, minY);

        // Debug: log the health grid
        Pathfinding.LogHealthGrid(healthGrid);

        // Compute the A* path
        List<Vector2Int> path = Pathfinding.AstarPath(start, end, healthGrid, width, height, minX, minY);

        if (path.Count > 0)
        {
            int totalPathCost = Pathfinding.CalculatePathCost(path, healthGrid, minX, minY);
            if (totalPathCost >= 99)
            {
                // Debug.Log("Path cost too high, clearing path.");
                PathToTarget.Clear();
                return;
            }

            // Convert path to world coordinates (centered)
            PathToTarget = Pathfinding.PathPointToCenter(path);
            PathCounter = 0;
        }
        else
        {
            // Debug.Log("No path found.");
        }
    }

    /// <summary>
    /// Initializes the health grid with costs for each cell based on terrain and conditions.
    /// </summary>
    private int[,] InitializeHealthGrid(int minX, int maxX, int minY, int maxY, int width, int height, float currentY)
    {
        int[,] healthGrid = new int[width, height];

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2Int gridCoords = new Vector2Int(x - minX, y - minY);

                // Determine grid cost based on terrain
                int distanceToGround = Pathfinding.DistanceToGround(x, y);
                if (distanceToGround > 0)
                {
                    // Assign costs based on proximity to the ground
                    healthGrid[gridCoords.x, gridCoords.y] = distanceToGround <= enemyStats.jumpForce - 1 ? distanceToGround : 99;
                }
                else
                {
                    // Check for specific tile properties
                    TileObject tile = WorldGenerator.GetDataFromWorldPos(new Vector2Int(x, y));
                    if (tile == null)
                    {
                        // check if there is a wall 
                        healthGrid[gridCoords.x, gridCoords.y] = 99; // Impassable
                    }
                    else if (Pathfinding.IsLadder(x, y))
                    {
                        healthGrid[gridCoords.x, gridCoords.y] = 1;
                    }
                    else if (Pathfinding.IsNeighborTileReachable(x, y))
                    {
                        healthGrid[gridCoords.x, gridCoords.y] = 1 + tile.getHP();
                    }
                    else if (y < currentY)
                    {
                        healthGrid[gridCoords.x, gridCoords.y] = 1 + tile.getHP();
                    }
                    else
                    {
                        healthGrid[gridCoords.x, gridCoords.y] = 99; // Impassable
                    }
                }
            }
        }

        return healthGrid;
    }

    /// <summary>
    /// Converts a world position to grid coordinates.
    /// </summary>
    private Vector2Int WorldToGridCoords(Vector2 position, int minX, int minY)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x) - minX,
            Mathf.FloorToInt(position.y) - minY
        );
    }


    /// <summary>
    /// Execute path movement if needed.
    /// </summary>
    public void PathExecute()
    {
        if (AutoLanding())
        {
            return;
        } // Prevent dead stacking

        if (DistanceToTarget(target.transform) < currentStats.attackRange)
        {
            // Debug.Log("Close to target, clearing path.");
            PathToTarget.Clear();
            RemovePathLine();
            PathCounter = 0;
            return;
        }
        else
        {
            // Debug.Log("Not close enough to target: " + DistanceToTarget(target.transform));
        }

        // DrawPath();

        // Follow path
        if (PathCounter < PathToTarget.Count)
        {
            if (PathToTarget[PathCounter].y > transform.position.y)
            {
                Jump(enemyStats.movingSpeed);
            }

            BreakObstaclesByAngle(target.transform);
            Approach(2 * enemyStats.movingSpeed, target.transform.position);

            // Move to next waypoint
            if (VillagerCloseToLocation(PathToTarget[PathCounter]))
            {
                PathCounter++;
            }
        }
        else
        {
            // No more waypoints, just approach directly
            Approach(2 * enemyStats.movingSpeed, target.transform.position);
            PathToTarget.Clear();
            PathCounter = 0;
        }
    }

    /// <summary>
    /// Continue chasing the last known target position for a while.
    /// </summary>
    public void FinishExistingPath()
    {
        if (AutoLanding())
        {
            return;
        } // Prevent dead stacking

        if (ChasingRemainder < 0f)
        {
            TargetRemainder = null;
            return;
        }

        if (DistanceToTarget(TargetRemainder) < currentStats.attackRange)
        {
            PathToTarget.Clear();
            RemovePathLine();
            PathCounter = 0;
            return;
        }

        // DrawPath();

        if (PathCounter < PathToTarget.Count)
        {
            Approach(2 * enemyStats.movingSpeed, TargetRemainder.position);
            // SenseFrontBlock();
            BreakObstaclesByAngle(TargetRemainder);

            if (VillagerCloseToLocation(PathToTarget[PathCounter]))
            {
                PathCounter++;
            }
        }
        else
        {
            Approach(2 * enemyStats.movingSpeed, TargetRemainder.position);
            PathToTarget.Clear();
            PathCounter = 0;
        }
    }

    public bool AutoLanding()
    {
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, groundLayerMask);
        if (hitCenter.transform == null)
        {
            inAir += Time.deltaTime;
            if (inAir > 0.9f)
            {
                float randomDirection = (UnityEngine.Random.Range(0f, 1f) <= 0.5f) ? -1f : 1f;
                rb.linearVelocity = new Vector2(randomDirection * enemyStats.movingSpeed * 5, -1f * rb.mass);
                //Debug.Log("auto landing");
                return true;
            }
        }
        else
        {
            inAir = 0f;
        }

        return false;
    }
    #endregion

    #region Path Drawing

    public void DrawPath()
    {
        if (lineRenderer == null)
        {
            // If no line renderer, create one
            _lineObj = new GameObject("PathLine");
            lineRenderer = _lineObj.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0.5f, 0f, 0.5f, 1f);
            lineRenderer.endColor = new Color(0.5f, 0f, 0.5f, 1f);
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;

            // Setup sorting
            _lineObj.layer = 11;
            lineRenderer.sortingOrder = 11;
        }

        if (lineRenderer == null)
        {
            // Debug.LogError("LineRenderer is not initialized!");
            return;
        }

        if (PathToTarget != null)
        {
            Vector3[] positions = new Vector3[PathToTarget.Count];
            for (int i = 0; i < PathToTarget.Count; i++)
            {
                positions[i] = new Vector3(PathToTarget[i].x, PathToTarget[i].y, 0);
            }

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
    }

    public void RemovePathLine()
    {
        if (_lineObj != null)
        {
            Destroy(_lineObj);
            lineRenderer = null;
        }
    }

    #endregion

    #region Obstacle Breaking

    /// <summary>
    /// Pick whether we break top/bottom/horizontal obstacles by analyzing the angle to the target.
    /// </summary>
    private void BreakObstaclesByAngle(Transform t)
    {
        Vector2 direction = t.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= 75 && angle <= 105)
        {
            BreakObstacles("top");
        }
        else if (angle >= -105 && angle <= -75)
        {
            BreakObstacles("bottom");
        }
        else
        {
            BreakObstacles("horizontal");
        }
    }

    /// <summary>
    /// Attempt to detect breakable objects and set them as target.
    /// </summary>
    public void BreakObstacles(string command)
    {
        _breakObstaclesCD -= Time.deltaTime;
        if (_breakObstaclesCD > 0f) return;

        Vector2 directionSide = facingRight ? Vector2.right : Vector2.left;
        Vector2 directionUpSide = (facingRight ? Vector2.right : Vector2.left) + Vector2.up;
        directionUpSide.Normalize();

        float rayLength = 0.05f;

        switch (command)
        {
            case "horizontal":
                RaycastHit2D hitTileDetect2 = Physics2D.Raycast(tileDetect2.position, directionSide, rayLength, groundLayerMask);
                RaycastHit2D hitTileDetect1 = Physics2D.Raycast(tileDetect1.position, directionSide, rayLength, groundLayerMask);
                if (hitTileDetect2.transform != null)
                {
                    var breakable1 = hitTileDetect2.transform.GetComponent<BreakableObjectController>(); // ground tile
                    var breakable11 = hitTileDetect2.transform.GetComponent<TowerController>(); // wall tile
                    if (breakable1 != null)
                    {
                        target = breakable1.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                        return;
                    }
                    else if (breakable11 != null)
                    {
                        target = breakable11.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                        return;
                    }
                }
                else if (hitTileDetect1.transform != null)
                {
                    var breakable2 = hitTileDetect1.transform.GetComponent<BreakableObjectController>();
                    var breakable22 = hitTileDetect1.transform.GetComponent<TowerController>();
                    if (breakable2 != null)
                    {
                        target = breakable2.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                        return;
                    }
                    else if (breakable22 != null)
                    {
                        target = breakable22.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                        return;
                    }
                }

                break;

            case "top":
                RaycastHit2D hitTileDetect3 = Physics2D.Raycast(tileDetect3.position, directionUpSide, rayLength, groundLayerMask);
                RaycastHit2D hitTileDetect4 = Physics2D.Raycast(tileDetect4.position, Vector2.up, rayLength, groundLayerMask);

                if (hitTileDetect3.transform != null)
                {
                    var breakable3 = hitTileDetect3.transform.GetComponent<BreakableObjectController>();
                    if (breakable3 != null)
                    {
                        target = breakable3.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                        return;
                    }
                }

                if (hitTileDetect4.transform != null)
                {
                    var breakable4 = hitTileDetect4.transform.GetComponent<BreakableObjectController>();
                    if (breakable4 != null)
                    {
                        target = breakable4.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                    }
                }

                break;

            case "bottom":
                RaycastHit2D hitTileDetect5 = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, rayLength, groundLayerMask);
                if (hitTileDetect5.transform != null)
                {
                    var breakable5 = hitTileDetect5.transform.GetComponent<BreakableObjectController>();
                    if (breakable5 != null)
                    {
                        target = breakable5.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                    }
                }

                break;
        }
    }
    #endregion
}
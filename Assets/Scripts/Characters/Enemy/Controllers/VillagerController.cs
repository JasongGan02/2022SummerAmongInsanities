using System.Collections.Generic;
using UnityEngine;

public class VillagerController : EnemyController
{
    #region Fields and Variables

    private bool _isResting = false;
    private bool _patrolToRight = true;

    private float _patrolTime = 0f;
    private float _patrolRest = 2f;
    private float _attackingAnimationTimer = 0f;
    private float _damageStartTime_0 = 0.17f;
    private float _wait = 0.3f;

    private float _targetTicker = 1f;
    private float _pathTicker = 3f;
    private float _chasingRemainder = 5f;
    private float _breakObstaclesCD = 1f;

    private int _pathCounter;

    private Animator _animator;
    private CapsuleCollider2D _capsuleCollider;

    public List<Vector2> PathToTarget = new List<Vector2>();

    [Header("Check Transforms")]
    public Transform groundCheckLeft;
    public Transform groundCheckRight;
    public Transform attackStart;
    public Transform attackEnd;
    public Transform head;
    public Transform tileDetect1;
    public Transform tileDetect2;
    public Transform tileDetect3;
    public Transform tileDetect4;
    public Transform TargetRemainder;

    [Header("Path Visualization")]
    public LineRenderer lineRenderer;
    private GameObject _lineObj; // For creating a line renderer in runtime

    [Header("Timers / Cooldowns")]
    // Expose these for balancing; adjust as needed
    public float defaultChasingRemainder = 5f;
    public float defaultPathTicker = 2f;
    public float defaultTargetTicker = 1f;
    public float attackAnimationDuration = 0.25f; 
    public float pathFindingRadiusExtra = 3f;
    public float breakObstacleCDReset = 1f;
    public float inAir = 0f;

    #endregion

    #region Animation States

    protected override string IdleAnimationState => "villager_idle";
    protected override string AttackAnimationState => "villager_attack";
    protected override string MoveAnimationState => "villager_walk";

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        // Cache references
        _animator = GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
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
        tileDetect1 = tileDetect1 ?? transform.Find("tileDetect1");
        tileDetect2 = tileDetect2 ?? transform.Find("tileDetect2");
        tileDetect3 = tileDetect3 ?? transform.Find("tileDetect3");
        tileDetect4 = tileDetect4 ?? transform.Find("tileDetect4");
    }

    #endregion

    #region Behavior Updates

    /// <summary>
    /// This is the high-level Behavior update. 
    /// Called from EnemyController - runs every frame or on fixed intervals if you want (depending on the architecture).
    /// </summary>
    protected override void UpdateEnemyBehavior()
    {
        UpdateColliderBasedOnAnimation();
        
        // Periodically re-check for target
        if (target == null || _targetTicker < 0) 
        { 
            target = SearchForTargetObject(); 
            // Debug.Log("trying find target");
            _targetTicker = defaultTargetTicker; 
        }

        // If no target in sight or memory
        if (target == null && TargetRemainder == null) 
        {
            PathToTarget.Clear();
            Patrol();
            RemovePathLine();
        }
        // If no target but we still remember the last known position
        else if (target == null && TargetRemainder != null) 
        {
            FinishExistingPath();
            _chasingRemainder -= Time.deltaTime;
        }
        // If we have a target
        else
        {
            // Update the last known target position
            TargetRemainder = target.transform;
            _chasingRemainder = defaultChasingRemainder;

            // If path is empty or our pathTicker has run out, re-pathfind
            if (PathToTarget.Count == 0 || _pathTicker < 0)
            {
                PathToTarget.Clear();
                PathFind();
                _pathTicker = defaultPathTicker;
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
        _pathTicker -= Time.deltaTime;
        _targetTicker -= Time.deltaTime;
    }

    #endregion

    #region Patrol / Movement

    /// <summary>
    /// Idle/Walk randomly for a short while.
    /// </summary>
    private void Patrol()
    {
        RemovePathLine();

        if (_patrolTime <= 0f)
        {
            // Prepare for a new patrol cycle
            _patrolRest = 2f;
            _animator.Play("villager_idle");
            _patrolTime = UnityEngine.Random.Range(1f, 3f);

            _patrolToRight = (UnityEngine.Random.Range(0f, 1f) >= 0.5f);
        }
        else if (_patrolRest > 0)
        {
            // Rest for a moment in idle
            _patrolRest -= Time.deltaTime;
        }
        else
        {
            // Actually move
            _animator.Play("villager_walk");
            SenseFrontBlock();
            if (!MoveForwardDepthCheck()) return;

            _patrolTime -= Time.deltaTime;

            // Move either left or right
            if (_patrolToRight)
            {
                rb.velocity = new Vector2(currentStats.movingSpeed, rb.velocity.y);
                if (!facingRight) Flip();
            }
            else
            {
                rb.velocity = new Vector2(-currentStats.movingSpeed, rb.velocity.y);
                if (facingRight) Flip();
            }
        }
    }

    /// <summary>
    /// Simple approach function. Adjusts orientation and sets velocity.
    /// </summary>
    private void Approach(float speed, Transform targetTransform)
    {
        if (speed > currentStats.movingSpeed)
        {
            _animator.Play("villager_run");
        }
        else
        {
            _animator.Play("villager_walk");
        }

        // Face the target
        if ((facingRight && targetTransform.position.x < transform.position.x)
             || (!facingRight && targetTransform.position.x > transform.position.x))
        {
            Flip();
        }

        // Move left or right
        if (targetTransform.position.x > transform.position.x)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
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
        if (!_isResting && _attackingAnimationTimer <= 0)
        {
            // Start the attack
            _animator.Play("villager_attack");
            _attackingAnimationTimer = attackAnimationDuration;
        }
        else if (_attackingAnimationTimer > 0)
        {
            // Wait for the animation to reach damage frame
            _attackingAnimationTimer -= Time.deltaTime;

            // Check if time is within the damage window
            float current = _attackingAnimationTimer;
            float damageWindowCenter = attackAnimationDuration - _damageStartTime_0;

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
                    // Possibly a player or some other character
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
        else if (_isResting)
        {
            // Once the attack is done, rest for cooldown
            if (_wait > 0)
            {
                _wait -= Time.deltaTime;
                _animator.Play("villager_rest");
            }
            else
            {
                _isResting = false;
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
        _isResting = true;
        _attackingAnimationTimer = 0f;
        _wait = frequency;
    }

    #endregion

    #region Flips / Colliders / Jump

    /// <summary>
    /// Update the collider shape if idle vs. moving vs. crouching, etc.
    /// </summary>
    private void UpdateColliderBasedOnAnimation()
    {
        // If idle state, set to "Sit" collider
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("villager_idle"))
        {
            ChangeCollider("Stand");
        }
        else
        {
            ChangeCollider("Sit");
        }
    }

    public void ChangeCollider(string status)
    {
        // Adjust the capsule size/offset for different states
        if (status == "Stand")
        {
            _capsuleCollider.offset = new Vector2(0.05061817f, -0.05666396f);
            _capsuleCollider.size = new Vector2(0.1316872f, 0.9257745f);
        }
        else // "Sit"
        {
            _capsuleCollider.offset = new Vector2(0.02169657f, -0.05424142f);
            _capsuleCollider.size = new Vector2(0.1895304f, 0.7271314f);
        }
    }

    /// <summary>
    /// Attempt to jump over a small obstacle. 
    /// </summary>
    private void Jump(float horizontal)
    {
        // If you want horizontal velocity to carry into jump, uncomment
        // rb.velocity = new Vector2(horizontal, currentStats.jumpForce);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, groundLayerMask);
        if (hitCenter.transform != null)
        {
            rb.velocity = new Vector2(horizontal, enemyStats.jumpForce);
        }
    }

    private bool AutoLanding(){
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, groundLayerMask);
        if (hitCenter.transform == null){
            inAir += Time.deltaTime;
            if (inAir > 0.9f){
                float randomDirection = (UnityEngine.Random.Range(0f, 1f) <= 0.5f) ? -1f : 1f;
                rb.velocity = new Vector2(randomDirection * enemyStats.movingSpeed * 5, -1f * rb.mass);
                //Debug.Log("auto landing");
                return true;
            }
        }else{
            inAir = 0f;
        }
        return false;
    }

    #endregion

    #region Obstacle / Sensing

    /// <summary>
    /// Check for obstacles in the forward/back direction, ground checks, etc.
    /// </summary>
    

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
                    rb.velocity = new Vector2(randomForce, rb.velocity.y);
                }
                else
                {
                    // Debug.Log("Obstruction detected. No horizontal force applied.");
                }
            }
        }
    }

    #endregion

    #region Pathfinding Methods

    /// <summary>
    /// Attempts to compute an A* path to the target. Stores points in PathToTarget.
    /// </summary>
    public void PathFind()
    {
        // Ensure there is a valid target, and we are outside attack range
        if (target == null || DistanceToTarget(target.transform) < currentStats.attackRange)
        {
            PathToTarget.Clear();
            _pathCounter = 0;
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
            _pathCounter = 0;
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
        if (AutoLanding()) {return;} // Prevent dead stacking
        if (DistanceToTarget(target.transform) < currentStats.attackRange )
        {
            Debug.Log("Close to target, clearing path.");
            PathToTarget.Clear();
            RemovePathLine();
            _pathCounter = 0;
            return;
        }else{
            Debug.Log("Not close enough to target: " + DistanceToTarget(target.transform));
        }

        // DrawPath();

        // Follow path
        if (_pathCounter < PathToTarget.Count)
        {
            
            if (PathToTarget[_pathCounter].y > transform.position.y){
                Jump(enemyStats.movingSpeed);
            }
            BreakObstaclesByAngle(target.transform);
            Approach(2 * currentStats.movingSpeed, target.transform);

            // Move to next waypoint
            if (CloseToLocation(PathToTarget[_pathCounter]))
            {
                _pathCounter++;
            }
        }
        else
        {
            // No more waypoints, just approach directly
            Approach(2 * currentStats.movingSpeed, target.transform);
            PathToTarget.Clear();
            _pathCounter = 0;
        }
    }

    /// <summary>
    /// Continue chasing the last known target position for a while.
    /// </summary>
    public void FinishExistingPath()
    {
        if (AutoLanding()) {return;} // Prevent dead stacking
        if (_chasingRemainder < 0f)
        {
            TargetRemainder = null;
            return;
        }

        if (DistanceToTarget(TargetRemainder) < currentStats.attackRange)
        {
            PathToTarget.Clear();
            RemovePathLine();
            _pathCounter = 0;
            return;
        }

        // DrawPath();

        if (_pathCounter < PathToTarget.Count)
        {
            Approach(2 * currentStats.movingSpeed, TargetRemainder);
            // SenseFrontBlock();
            BreakObstaclesByAngle(TargetRemainder);

            if (CloseToLocation(PathToTarget[_pathCounter]))
            {
                _pathCounter++;
            }
        }
        else
        {
            Approach(2 * currentStats.movingSpeed, TargetRemainder);
            PathToTarget.Clear();
            _pathCounter = 0;
        }
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
                    var breakable1 = hitTileDetect2.transform.GetComponent<BreakableObjectController>();        // ground tile
                    var breakable11 = hitTileDetect2.transform.GetComponent<TowerController>();                  // wall tile
                    if (breakable1 != null)
                    {
                        target = breakable1.gameObject;
                        _breakObstaclesCD = breakObstacleCDReset;
                        return;
                    } else if (breakable11 != null)
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
                    } else if (breakable22 != null){
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

    #region Utility

    public bool CloseToLocation(Vector2 location)
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y - 0.25f);
        return Vector2.Distance(currentLoc, location) < 1.2f;
    }

    #endregion
}

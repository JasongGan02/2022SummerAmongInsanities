using System.Collections.Generic;
using UnityEngine;

public class VillagerController : EnemyController
{
    bool rest = false;
    bool facingright = false;
    float patroltime = 0f;
    private Animator animator;
    bool patrolToRight = true;
    float patrolRest = 2f;
    

    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public Transform frontCheck;
    public Transform backCheck;
    public Transform attackStart;
    public Transform attackEnd;
    public Transform head;

    //private BoxCollider2D boxCollider;
    private CapsuleCollider2D capsuleCollider;

    private float Wait = 0.3f;
    private float attacking_animation_timer = 0f;
    float damage_start_time_0 = 0.17f;
    float TargetTicker = 1f;
    public List<Vector2> PathToTarget = new List<Vector2>();
    public float PathTicker = 3f;
    public int PathCounter;
    public LineRenderer lineRenderer;
    public Transform tileDetect1;
    public Transform tileDetect2;
    public Transform tileDetect3;
    public Transform tileDetect4;
    public float BreakObstaclesCD = 1f;
    public Transform TargetRemainder;
    public float ChasingRemainder = 5f;
    
    protected override string IdleAnimationState => "villager_idle";
    protected override string AttackAnimationState => "villager_attack";
    protected override string MoveAnimationState => "villager_walk";
    //protected override string DeathAnimationState => "Zombie_Death";

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        groundCheckLeft = transform.Find("groundCheckLeft");
        groundCheckCenter = transform.Find("groundCheckCenter");
        groundCheckRight = transform.Find("groundCheckRight");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");
        attackStart = transform.Find("attackStart");
        attackEnd = transform.Find("attackEnd");
        head = transform.Find("head");
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        tileDetect1 = transform.Find("tileDetect1");
        tileDetect2 = transform.Find("tileDetect2");
        tileDetect3 = transform.Find("tileDetect3");
        tileDetect4 = transform.Find("tileDetect4");
    }

    
    
    /*protected override void UpdateEnemyBehavior()
    {
        target = SearchForTargetObject(); //attempt to search for target
        if (target == null && !HasLastKnownPosition) //如果现在既没有找到目标也没有上一个目标遗留的地址，则脱离仇恨，开始散步。
        {
            Patrol();
        }
        else if (target == null && HasLastKnownPosition) //如果现在没有目标但是有上一个目标遗留的地址，则继续前进。
        {
            Approach(LastKnownPosition, true);
        }
        else //有目标，追就完事了
        {
            Approach(target.transform.position, true);
            Attack(target);
        }
    }*/

    protected void Patrol()
    {
        
    }

    
    protected override void UpdateEnemyBehavior()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("villager_idle") == false)
        { ChangeCollider("Stand"); }
        else { ChangeCollider("Sit"); }


        if (target == null || TargetTicker < 0) { target = SearchForTargetObject(); TargetTicker = 1f; } // if doesn't have target
        if (target == null && TargetRemainder == null) { PathToTarget.Clear(); patrol(); RemovePathLine(); }
        else if (target == null && TargetRemainder != null) { FinishExistingPath(); ChasingRemainder -= Time.deltaTime; } // when target out of range, keep chasing for a while
        else
        {
            TargetRemainder = target.transform;
            ChasingRemainder = 5f;  // chasing time after losing target in visual
            if (PathToTarget.Count == 0 || PathTicker < 0) { PathToTarget.Clear(); PathFind(); PathTicker = 2f; }   // find a path to target
            else { PathExecute(); }         // continue current path
           
            if (DistanceToTarget(target.transform) < currentStats.attackRange || target.transform.GetComponent<BreakableObjectController>() != null)
            {
                attack(target.transform, 1f / currentStats.attackInterval); // default:1;  lower -> faster
            }
            else
            {
                //Debug.Log("approaching target " + target.transform);
                //flip(target.transform);
            }
            
            ShakePlayerOverHead();
        }
        PathTicker -= Time.deltaTime;  // update ticker for path tracking
        TargetTicker -= Time.deltaTime; // update ticker for target tracking
    }
    

    void attack(Transform target, float frequency)
    {
        // start attack
        if (!rest && attacking_animation_timer <= 0)
        {
            animator.Play("villager_attack");
            attacking_animation_timer = 0.25f; // Time & Speed of animation
        }

        // wait for attack behavior finish
        else if (attacking_animation_timer > 0) // make sure the attack behavior animation is complete
        {
            attacking_animation_timer -= Time.deltaTime;

            if (attacking_animation_timer < (0.25f - damage_start_time_0 + 0.03f) && attacking_animation_timer > (0.25f - damage_start_time_0 - 0.01f))
            {
                var breakable = target.transform.GetComponent<IDamageable>();
                if (breakable != null)
                {
                    ApplyDamage(breakable);
                }
                else
                {
                    float checkD = Vector2.Distance(attackEnd.position, target.transform.position);
                    if (checkD < 0.75f) // hurt target successfully
                    {
                        ApplyDamage(target.GetComponent<CharacterController>());
                    }
                    rest = true;
                    attacking_animation_timer = 0f;
                    Wait = frequency;
                }
            }

        }

        // finished attack and wait for next, this else if should be changed to else later!
        else if (rest)
        {
            if (Wait > 0)
            {
                Wait -= Time.deltaTime;
                animator.Play("villager_rest");
            }
            else
            {
                rest = false;
            }
        }

        flip(target);
    }
    void approach(float speed, Transform target)
    {
        if (speed > currentStats.movingSpeed)
        {
            animator.Play("villager_run");
        }
        else
        {
            animator.Play("villager_walk");
        }
        if ((facingright && target.position.x < transform.position.x) || (!facingright && target.position.x > transform.position.x)) { flip(); }
        
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
        RemovePathLine();
        if (patroltime <= 0f)
        {
            patrolRest = 2f;
            animator.Play("villager_idle");
            patroltime = UnityEngine.Random.Range(1f, 3f);
            if (UnityEngine.Random.Range(0f, 1f) < 0.5) // go left
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
            animator.Play("villager_walk");
            SenseFrontBlock();
            if (MoveForwardDepthCheck() == false) { return; }
            patroltime -= Time.deltaTime;
            if (patrolToRight)
            {
                if (MoveForwardDepthCheck()) 
                {
                    rb.velocity = new Vector2(currentStats.movingSpeed, rb.velocity.y);
                    if (!facingright) { flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck())
                {
                    rb.velocity = new Vector2(-currentStats.movingSpeed, rb.velocity.y);
                    if (facingright) { flip(); }
                }
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
        if (MoveForwardDepthCheck() == false) { return; }
        headCheck();
        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.05f, groundLayerMask);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, groundLayerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.05f, groundLayerMask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, groundLayerMask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, groundLayerMask);

        if (hitCenter.transform != null)
        {
            if ((facingright && rb.velocity.x > 0) || (!facingright && rb.velocity.x < 0))
            {
                if (hitFront.transform != null)
                {
                    if (headCheck())
                    {
                        Jump(2 * currentStats.movingSpeed);
                    }
                }
            }
            else if ((facingright && rb.velocity.x < 0) || (!facingright && rb.velocity.x > 0))
            {
                if (hitBack.transform != null)
                {
                    if (headCheck())
                    {
                        Jump(2 * currentStats.movingSpeed);
                    }
                }
            }
        }
    }
    bool headCheck()
    {
        Vector3 direction = transform.TransformDirection(-Vector3.right);
        RaycastHit2D headRay = Physics2D.Raycast(head.position, direction, 0.34f, groundLayerMask);
        Debug.DrawRay(head.position, direction * 0.34f, Color.red);        // bottom right
        if (headRay.collider != null && headRay.collider.gameObject.tag == "ground")
        {
            //Debug.Log("headCheck return false");
            return false;
        }

        return true;
    }
    private void Jump(float horizontal)
    {
        rb.velocity = new Vector2(horizontal * 1.0f, currentStats.jumpForce);
    }
    
    public void ChangeCollider(string status)
    {
        // Enable or disable the colliders based on the state
        // boxCollider.enabled = !isSitting;
        if (status == "Stand")
        {
            //boxCollider.size = new Vector2(0.1875544f, 1.0f);
            capsuleCollider.offset = new Vector2(0.05061817f, -0.03f);
            capsuleCollider.size = new Vector2(0.1316872f, 0.9791025f);
        }
        else
        {
            //boxCollider.size = new Vector2(0.1875544f, 0.718245f);
            capsuleCollider.offset = new Vector2(0.02169657f, -0.05424142f);
            capsuleCollider.size = new Vector2(0.1895304f, 0.7271314f);
        }
    }
    private bool MoveForwardDepthCheck() // when walking forward, don't go to abyss
    {
        Vector2 frontDepthDetector = new Vector2(frontCheck.position.x + 0.35f, frontCheck.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontDepthDetector, Vector2.down, 3f, groundLayerMask);
        if (hit.collider != null) { return true; }

        return false;
    }

    new void ShakePlayerOverHead()
    {
        Vector2 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;

        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Check the conditions
        if ((angle >= -95f && angle <= -85f) || (angle >= 85 && angle <= 95) && distance < 3f)
        {
            Debug.Log("Angle and distance conditions met");
            if (Mathf.Abs(target.transform.position.x - transform.position.x) <= 0.3f)
            {
                Debug.Log("Within horizontal range");
                if (Mathf.Abs(target.transform.position.y - transform.position.y) < 2f)
                {
                    Debug.Log("Shaking the player overhead");
                    if (UnityEngine.Random.Range(0f, 1f) <= 0.5f)
                    {
                        rb.velocity = new Vector2(-12f, rb.velocity.y);
                    }
                    else
                    {
                        rb.velocity = new Vector2(12f, rb.velocity.y);
                    }
                }
            }
        }
        else
        {
            //Debug.Log("distance: " + distance + " angle: " + angle);
        }
    }
    
    public void PathFind()
    {
        if (target == null || DistanceToTarget(target.transform) < currentStats.attackRange) { PathToTarget.Clear(); PathCounter = 0; return; }

        float TX = target.transform.position.x; float TY = target.transform.position.y;
        float CX = transform.position.x; float CY = transform.position.y - 0.25f;

        int startX = (int)CX;
        int startY = (int)CY;
        int endX = (int)TX;
        int endY = (int)TY;

        int minX = Mathf.Min(startX, endX) - 3;
        int maxX = Mathf.Max(startX, endX) + 3;
        int minY = Mathf.Min(startY, endY) - 4;
        int maxY = Mathf.Max(startY, endY) + 4;

        int[,] healthGrid = new int[maxX - minX + 1, maxY - minY + 1];

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int disToGround = Pathfinding.DistanceToGround(x, y);
                if (disToGround != 0)
                {
                    if (disToGround == 1) { healthGrid[x - minX, y - minY] = 1; continue; }
                    else if (disToGround == 2) { healthGrid[x - minX, y - minY] = 2; continue; }
                    else if (disToGround > 2 && Pathfinding.IsNeighborOfTile(x, y) && y <= startY) { healthGrid[x - minX, y - minY] = 2; continue; }
                    else { healthGrid[x - minX, y - minY] = 99; continue; }
                }
                else
                {
                    Vector2Int position = new Vector2Int(x, y);
                    TileObject tile = WorldGenerator.GetDataFromWorldPos(position);
                    if (Pathfinding.IsLadder(x, y)) { healthGrid[x - minX, y - minY] = 1; continue; }
                    else if (Pathfinding.IsNeighborTileReachable(x, y)) { healthGrid[x - minX, y - minY] = 1 + tile.getHP(); continue; }
                    else if (y < startY) { healthGrid[x - minX, y - minY] = 1 + tile.getHP(); continue; }
                    else if (y >= startY) { healthGrid[x - minX, y - minY] = 99; continue; }
                    else { healthGrid[x - minX, y - minY] = 99; continue; }
                }
            }
        }
        healthGrid[startX - minX, startY - minY] = 0;
        Vector2Int start = new Vector2Int(startX - minX, startY - minY);
        Vector2Int end = new Vector2Int(endX - minX, endY - minY);

        Pathfinding.LogHealthGrid(healthGrid);

        List<Vector2Int> path = Pathfinding.AstarPath(start, end, healthGrid, maxX - minX + 1, maxY - minY + 1, minX, minY);
        if (path.Count > 0)
        {
            int totalPathCost = Pathfinding.CalculatePathCost(path, healthGrid, minX, minY);
            if (totalPathCost >= 99) { PathToTarget.Clear(); return; }
            PathToTarget = Pathfinding.PathPointToCenter(path);
            PathCounter = 0;

            string pathString = string.Join(", ", path);
            //Debug.Log(pathString);
        }
        else
        {
            Debug.Log("No path found.");
        }
    }

    public void PathExecute()
    {
        if (DistanceToTarget(target.transform) < currentStats.attackRange)
        {
            PathToTarget.Clear(); RemovePathLine(); PathCounter = 0;
            return;
        }
        DrawPath();
        if (PathCounter < PathToTarget.Count)
        {
            //Debug.Log("Go to: " + PathToTarget[PathCounter]);

            approach(2 * currentStats.movingSpeed, target.transform);
            SenseFrontBlock();

            Vector2 direction = target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (angle >= 75 && angle <= 105) { BreakObstacles("top"); }
            else if (angle >= -105 && angle <= -75) { BreakObstacles("bottom"); }
            else { BreakObstacles("horizontal"); }

            if (CloseToLocation(PathToTarget[PathCounter]))
            {
                PathCounter++;
            }
            else
            {
                //Debug.Log("Approaching path position: " + PathToTarget[PathCounter]);
            }
        }
        else
        {
            approach(2 * currentStats.movingSpeed, target.transform);
            PathToTarget.Clear();
            PathCounter = 0;
        }
    }

    public void FinishExistingPath()
    {
        if (ChasingRemainder < 0f)
        {
            TargetRemainder = null;
            return;
        }
        if (DistanceToTarget(TargetRemainder) < currentStats.attackRange)
        {
            PathToTarget.Clear(); RemovePathLine(); PathCounter = 0;
            return;
        }
        DrawPath();
        if (PathCounter < PathToTarget.Count)
        {
            //Debug.Log("Go to: " + PathToTarget[PathCounter]);

            approach(2 * currentStats.movingSpeed, TargetRemainder);
            SenseFrontBlock();

            Vector2 direction = TargetRemainder.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (angle >= 75 && angle <= 105) { BreakObstacles("top"); }
            else if (angle >= -105 && angle <= -75) { BreakObstacles("bottom"); }
            else { BreakObstacles("horizontal"); }

            if (CloseToLocation(PathToTarget[PathCounter]))
            {
                PathCounter++;
            }
            else
            {
                //Debug.Log("Approaching path position: " + PathToTarget[PathCounter]);
            }
        }
        else
        {
            approach(2 * currentStats.movingSpeed, TargetRemainder);
            PathToTarget.Clear();
            PathCounter = 0;
        }
    }

    private GameObject lineObj;
    public void DrawPath()
    {
        if (lineRenderer == null)
        {
            lineObj = new GameObject("PathLine");
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0.5f, 0f, 0.5f, 1f);
            lineRenderer.endColor = new Color(0.5f, 0f, 0.5f, 1f);
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            lineObj.layer = 11;
            lineRenderer.sortingOrder = 11;
        }
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer is not initialized!");
            return;
        }
        else if (PathToTarget != null)
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
        if (lineObj != null)
        {
            Destroy(lineObj);
            lineRenderer = null;
        }
    }

    public bool CloseToLocation(Vector2 location)
    {
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y - 0.25f);
        if (Vector2.Distance(currentLoc, location) < 1.2f)
        {
            return true;
        }
        return false;
    }

    public void BreakObstacles(string command)
    {
        BreakObstaclesCD -= Time.deltaTime;
        if (BreakObstaclesCD < 0f)
        {
            Vector2 direction1 = Vector2.left;
            Vector2 direction2 = Vector2.left;
            Vector2 direction3 = (Vector2.left + Vector2.up).normalized;
            Vector2 direction4 = Vector2.up;
            if (facingright)
            {
                direction1 = Vector2.right;
                direction2 = Vector2.right;
                direction3 = (Vector2.right + Vector2.up).normalized;
            }
            float rayLength = 0.05f;

            if (command == "horizontal")
            {
                RaycastHit2D hitTileDetect1 = Physics2D.Raycast(tileDetect1.position, direction1, rayLength, groundLayerMask);
                Debug.DrawRay(tileDetect1.position, direction1 * rayLength, Color.red);
                RaycastHit2D hitTileDetect2 = Physics2D.Raycast(tileDetect2.position, direction2, rayLength, groundLayerMask);
                Debug.DrawRay(tileDetect2.position, direction2 * rayLength, Color.green);
                if (hitTileDetect2.transform != null)
                {
                    var breakable1 = hitTileDetect2.transform.GetComponent<BreakableObjectController>();
                    if (breakable1 != null)
                    {
                        target = breakable1.gameObject; BreakObstaclesCD = 1f;
                    }
                }
                else if (hitTileDetect1.transform != null)
                {
                    var breakable2 = hitTileDetect1.transform.GetComponent<BreakableObjectController>();
                    if (breakable2 != null)
                    {
                        target = breakable2.gameObject; BreakObstaclesCD = 1f;
                    }
                }
            }
            else if (command == "top")
            {
                RaycastHit2D hitTileDetect3 = Physics2D.Raycast(tileDetect3.position, direction3, rayLength, groundLayerMask);
                Debug.DrawRay(tileDetect3.position, direction3 * rayLength, Color.blue);
                RaycastHit2D hitTileDetect4 = Physics2D.Raycast(tileDetect4.position, direction4, rayLength, groundLayerMask);
                Debug.DrawRay(tileDetect4.position, direction4 * rayLength, Color.yellow);
                if (hitTileDetect3.transform != null)
                {
                    var breakable3 = hitTileDetect3.transform.GetComponent<BreakableObjectController>();
                    if (breakable3 != null)
                    {
                        target = breakable3.gameObject; BreakObstaclesCD = 1f;
                    }
                }
                else if (hitTileDetect4.transform != null)
                {
                    var breakable4 = hitTileDetect4.transform.GetComponent<BreakableObjectController>();
                    if (breakable4 != null)
                    {
                        target = breakable4.gameObject; BreakObstaclesCD = 1f;
                    }
                }
            }
            else if (command == "bottom")
            {
                RaycastHit2D hitTileDetect5 = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, rayLength, groundLayerMask);
                Debug.DrawRay(groundCheckCenter.position, Vector2.down * rayLength, Color.blue);
                if (hitTileDetect5.transform != null)
                {
                    var breakable5 = hitTileDetect5.transform.GetComponent<BreakableObjectController>();
                    if (breakable5 != null)
                    {
                        target = breakable5.gameObject; BreakObstaclesCD = 1f;
                    }
                }
            }
        }
    }
        
    
}

using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class VillagerController : EnemyController
{
    bool rest = false;
    bool facingright = true;
    float patroltime = 0f;
    private Animator animator;
    bool patrolToRight = true;
    float patrolRest = 2f;
    GameObject target;

    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public Transform frontCheck;
    public Transform backCheck;
    public Transform attackStart;
    public Transform attackEnd;
    public Transform head;
    LayerMask ground_mask;

    //private BoxCollider2D boxCollider;
    private CapsuleCollider2D capsuleCollider;

    private float Wait = 0.3f;
    private float attacking_animation_timer = 0f;
    float damage_start_time_0 = 0.17f;
    float TargetTicker = 1f;
    public List<Vector2> PathToTarget = new List<Vector2>();
    public Vector2 TargetDirection = Vector2.zero;
    public float TargetAngle = 0f;
    public float PathTicker = 3f;
    public int PathCounter;
    public int bodyHeight = 2;
    public int bodyWidth = 1;
    public LineRenderer lineRenderer;
    public Transform tileDetect1;
    public Transform tileDetect2;
    public Transform tileDetect3;
    public Transform tileDetect4;
    public float BreakObstaclesCD = 1f;
    public Transform TargetRemainder;
    public float ChasingRemainder = 5f;
    public Transform groundCheckCorners;

    public float isJumping = 1f;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        towerContainer = FindObjectOfType<TowerContainer>();
        ground_mask = LayerMask.GetMask("ground");
        groundCheckLeft = transform.Find("groundCheckLeft");
        groundCheckCenter = transform.Find("groundCheckCenter");
        groundCheckRight = transform.Find("groundCheckRight");
        frontCheck = transform.Find("frontCheck");
        backCheck = transform.Find("backCheck");
        attackStart = transform.Find("attackStart");
        attackEnd = transform.Find("attackEnd");
        head = transform.Find("head");
        //boxCollider = GetComponent<BoxCollider2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        tileDetect1 = transform.Find("tileDetect1");
        tileDetect2 = transform.Find("tileDetect2");
        tileDetect3 = transform.Find("tileDetect3");
        tileDetect4 = transform.Find("tileDetect4");
        groundCheckCorners = transform.Find("groundCheckCorners");
    }

    protected override void EnemyLoop()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("villager_attack") == true)
        { ChangeCollider("villager_attack"); }
        else { ChangeCollider(" "); }

        Debug.Log("target is " + target);
        if (target == null || TargetTicker < 0) { target = WhatToAttack(); TargetTicker = 1f; } // if doesn't have target
        if (target == null && TargetRemainder == null) { PathToTarget.Clear(); patrol(); RemovePathLine(); }
        else if (target == null && TargetRemainder != null) { FinishExistingPath(); ChasingRemainder -= Time.deltaTime; } // when target out of range, keep chasing for a while
        else
        {
            Debug.Log("target " + target);
            CheckStuckAndApplyForces();
            TargetRemainder = target.transform;
            ChasingRemainder = 5f;  // chasing time after losing target in visual
            if (PathToTarget.Count == 0 || PathTicker < 0) { PathToTarget.Clear(); PathFind(); PathTicker = 2f; }   // find a path to target
            else { PathExecute(); }         // continue current path
            if (true || villager_sight())   // see target clearly
            {
                if (DistanceToTarget(target.transform) < currentStats.attackRange || target.transform.GetComponent<BreakableObjectController>() != null)
                {
                    attack(target.transform, 1f / currentStats.attackInterval); // default:1;  lower -> faster
                }
                else
                {
                    //Debug.Log("approaching target " + target.transform);
                    //flip(target.transform);
                }
            }
            ShakePlayerOverHead();
        }
        PathTicker -= Time.deltaTime;  // update ticker for path tracking
        TargetTicker -= Time.deltaTime; // update ticker for target tracking
        isJumping -= Time.deltaTime; // update isJumping
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

    
    void patrol()
    {
        if (GroupApproaching)
        {
            patroltime = 0.2f; patrolRest = 0f;
            if (GroupApproachTarget.position.x > transform.position.x) { patrolToRight = true; }
            else { patrolToRight = false; }
            Debug.Log("Villager is group approach something");
        }
        //else
        //{
        //    patroltime = 0f;
        //}

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
                if (MoveForwardDepthCheck() == true) 
                {
                    rb.velocity = new Vector2(currentStats.movingSpeed, rb.velocity.y);
                    if (!facingright) { flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck() == true)
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
        facingright = !facingright;

        if (facingright)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
    new void SenseFrontBlock()
    {
        if (MoveForwardDepthCheck() == false) { return; }
        headCheck();
        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, ground_mask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, ground_mask);

        if (hitCenter.transform != null)
        {
            if ((facingright && rb.velocity.x > 0) || (!facingright && rb.velocity.x < 0))  // move forward
            {
                if (hitFront.transform != null)
                {
                    if (headCheck())
                    {
                        Jump(2 * currentStats.movingSpeed);
                    }
                }
            }
            else if ((facingright && rb.velocity.x < 0) || (!facingright && rb.velocity.x > 0)) // move back
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
        Vector2 direction = Vector2.right;
        if (!facingright) { direction = Vector2.left; }
        RaycastHit2D footRay = Physics2D.Raycast(tileDetect1.position, direction, 0.05f, ground_mask);
        RaycastHit2D headRay = Physics2D.Raycast(tileDetect2.position, direction, 0.05f, ground_mask);
        Debug.DrawRay(tileDetect1.position, direction * 0.05f, Color.red);
        Debug.DrawRay(tileDetect2.position, direction * 0.05f, Color.blue);
        if (footRay.collider != null && footRay.collider.gameObject.tag == "ground" &&
            (headRay.collider == null || headRay.collider.gameObject.tag != "ground"))
        {
            //Debug.Log("allow jump");
            return true;
        }

        return false;
    }

    private void Jump(float horizontal)
    {
        rb.velocity = new Vector2(horizontal * 1.0f, currentStats.jumpForce);
        isJumping = 1f;
    }

    private bool villager_sight()
    {
        if (target.transform.GetComponent<BreakableObjectController>() != null) { return true; } // this is tile object, should see by default
         
        Rigidbody2D targetRB = target.GetComponent<Rigidbody2D>();
        Vector2 targetTop = targetRB.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerTop = rb.position + Vector2.up * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 targetBottom = targetRB.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 villagerBottom = rb.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;

        Debug.DrawRay(targetTop, villagerTop - targetTop, Color.red);   // top
        Debug.DrawRay(targetBottom, villagerBottom - targetBottom, Color.red);   // bottom

        float distance1 = Vector2.Distance(targetTop, villagerTop);
        float distance2 = Vector2.Distance(targetBottom, villagerBottom);

        RaycastHit2D checkTop = Physics2D.Raycast(targetTop, villagerTop - targetTop, distance1, ground_mask);
        RaycastHit2D checkBottom = Physics2D.Raycast(targetBottom, villagerBottom - targetBottom, distance2, ground_mask);
        if (checkTop.collider != null &&
            checkBottom.collider != null &&
            checkTop.collider.gameObject.CompareTag("ground") &&
            checkBottom.collider.gameObject.CompareTag("ground"))
        {
            //Debug.Log("there is ground block");
            return false;
        }
        return true;
    }
    public void ChangeCollider(string status)
    {
        // Enable or disable the colliders based on the state
        // boxCollider.enabled = !isSitting;
        if (status == "villager_attack")
        {
            capsuleCollider.offset = new Vector2(-0.005420446f, -0.02473235f);
            capsuleCollider.size = new Vector2(0.2200007f, 0.9838557f);
            transform.localScale = new Vector2(2f, 2f);
        }
        else
        {
            capsuleCollider.offset = new Vector2(0.03022671f, -0.04200697f);
            capsuleCollider.size = new Vector2(0.8412695f, 2.709069f);
            transform.localScale = new Vector2(0.7f, 0.7f);
        }
    }
    private bool MoveForwardDepthCheck() // when walking forward, don't go to abyss
    {
        Vector2 frontDepthDetector = new Vector2(frontCheck.position.x + 0.35f, frontCheck.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontDepthDetector, Vector2.down, 3f, ground_mask);
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
            //Debug.Log("Angle and distance conditions met");
            if (Mathf.Abs(target.transform.position.x - transform.position.x) <= 0.3f)
            {
                //Debug.Log("Within horizontal range");
                if (Mathf.Abs(target.transform.position.y - transform.position.y) < 2f)
                {
                    //Debug.Log("Shaking the player overhead");
                    if (UnityEngine.Random.Range(0f, 1f) <= 0.5f)
                    {
                        rb.velocity = new Vector2(-40f, rb.velocity.y);
                    }
                    else
                    {
                        rb.velocity = new Vector2(40f, rb.velocity.y);
                    }
                }
            }
        }
        else
        {
            //Debug.Log("distance: " + distance + " angle: " + angle);
        }
    }

    public float DistanceToPoint(Vector2Int a)
    {
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);

        // Convert Vector2Int 'a' to Vector2 for distance calculation
        Vector2 targetPosition = new Vector2(a.x, a.y);

        // Calculate and return the Euclidean distance
        return Vector2.Distance(currentPosition, targetPosition);
    }

    void approach(float speed, float x, float y)
    {
        if (speed > currentStats.movingSpeed)
        {
            animator.Play("villager_run");
        }
        else
        {
            animator.Play("villager_walk");
        }
        if (x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
        if ((x > transform.position.x && !facingright) || (x < transform.position.x && facingright))
        {
            flip();
        }
    }

    public GameObject GetTileGameObject(Vector2Int tilePosition)
    {
        Vector2 rayOrigin = tilePosition;
        Vector2 rayDirection = Vector2.down;
        float rayLength = 1.0f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, ground_mask);
        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        return null;
    }











    public void PathFind()  // only find path, not execute
    {
        if (target == null || DistanceToTarget(target.transform) < currentStats.attackRange) { PathToTarget.Clear(); PathCounter = 0; return; }

        float TX = target.transform.position.x; float TY = target.transform.position.y; // target x y
        float CX = transform.position.x; float CY = transform.position.y - 0.25f; // currect x y

        // Convert positions to integers for indexing
        int startX = (int)CX;
        int startY = (int)CY;
        int endX = (int)TX;
        int endY = (int)TY;

        // Ensure minimum and maximum are correct regardless of direction
        int minX = Mathf.Min(startX, endX) - 3; // increase fault tolerance
        int maxX = Mathf.Max(startX, endX) + 3;
        int minY = Mathf.Min(startY, endY) - 4;
        int maxY = Mathf.Max(startY, endY) + 4;
        //Debug.Log("min x" + minX + "min y" + minY + "max x" + maxX + "max y" + maxY);
        // Create 2D array to store health values
        int[,] healthGrid = new int[maxX - minX + 1, maxY - minY + 1];

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {   // consider gravity limitation
                //if (y > minY && healthGrid[x - minX, y - minY - 1] == 99) { healthGrid[x - minX, y - minY] = 99; continue; }    // ignore too high
                int disToGround = DistanceToGround(x, y);
                if (disToGround != 0)
                {   // air
                    if (disToGround == 1) { healthGrid[x - minX, y - minY] = 1; continue; } // surface
                    else if (disToGround == 2) { healthGrid[x - minX, y - minY] = 1 + 1; continue; }    // jump attainable
                    else if (disToGround > 2 && IsNeighborOfTile(x, y) && y <= startY) { healthGrid[x - minX, y - minY] = 2; continue; } // next to tile object, possible to reach
                    else { healthGrid[x - minX, y - minY] = 99; continue; }
                }
                else
                {   // Tile, 1: what tile, 2: altitude
                    Vector2Int position = new Vector2Int(x, y);
                    TileObject tile = WorldGenerator.GetDataFromWorldPos(position);
                    if (IsLadder(x, y)) { healthGrid[x - minX, y - minY] = 1; continue; }
                    else if (IsNeighborTileReachable(x, y)) { healthGrid[x - minX, y - minY] = 1 + tile.getHP(); continue; }
                    else if (y < startY) { healthGrid[x - minX, y - minY] = 1 + tile.getHP(); continue; }
                    else if (y >= startY) { healthGrid[x - minX, y - minY] = 99; continue; }
                    else { healthGrid[x - minX, y - minY] = 99; continue; }
                }
            }
        }
        healthGrid[startX - minX, startY - minY] = 0; // starting point doesn't have cost
        Vector2Int start = new Vector2Int(startX - minX, startY - minY);
        Vector2Int end = new Vector2Int(endX - minX, endY - minY);

        LogHealthGrid(healthGrid); // Print the formatted grid

        List<Vector2Int> path = AstarPath(start, end, healthGrid, maxX - minX + 1, maxY - minY + 1, minX, minY);
        if (path.Count > 0)
        {
            int totalPathCost = CalculatePathCost(path, healthGrid, minX, minY);
            if (totalPathCost >= 99) { PathToTarget.Clear(); return; }
            PathToTarget = PathPointToCenter(path);      // update Path
            PathCounter = 0;         // reset ticker

            //Debug.Log("start position " + startX + "  " + startY + " target position " + endX + " " + endY);
            string pathString = string.Join(", ", path);
            //Debug.Log(pathString);
        }
        else
        {
            //Debug.Log("No path found.");
        }
    }
    public void PathExecute()       // execute path to target
    {
        //Debug.Log("Path found with steps: " + path.Count);
        if (DistanceToTarget(target.transform) < currentStats.attackRange)
        {
            PathToTarget.Clear(); RemovePathLine(); PathCounter = 0;
            return;
        }
        DrawPath();
        if (PathCounter < PathToTarget.Count)
        {
            Debug.Log("Go to: " + PathToTarget[PathCounter]);

            approach(2 * currentStats.movingSpeed, target.transform);
            SenseFrontBlock();

            TargetDirection = target.transform.position - transform.position;
            TargetAngle = Mathf.Atan2(TargetDirection.y, TargetDirection.x) * Mathf.Rad2Deg;
            if (TargetAngle < 0) { TargetAngle += 360; }
            Debug.Log("angle: " + TargetAngle);
            if (TargetAngle >= 75 && TargetAngle <= 105) { BreakObstacles(); }
            else if (TargetAngle <= -75 && TargetAngle >= -105) { BreakObstacles(); }
            else { BreakObstacles(); }

            if (CloseToLocation(PathToTarget[PathCounter])) // needs testing
            {
                PathCounter++;
            }
            else
            {
                //Debug.Log("Approaching path position: " + PathToTarget[PathCounter]);
            }
        }
        else
        {   // reset Path
            approach(2 * currentStats.movingSpeed, target.transform);
            PathToTarget.Clear();
            PathCounter = 0;
        }
    }
    public void FinishExistingPath()    // execute the path after visually losed target
    {
        if (ChasingRemainder < 0f)
        {
            TargetRemainder = null; // erase Target Remainder
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

            if (angle >= 75 && angle <= 105) { BreakObstacles(); }
            else if (angle >= -105 && angle <= -75) { BreakObstacles(); }
            else if (headCheck() == false) { BreakObstacles(); }
                
            if (CloseToLocation(PathToTarget[PathCounter])) // needs testing
            {
                PathCounter++;
            }
            else
            {
                //Debug.Log("Approaching path position: " + PathToTarget[PathCounter]);
            }
        }
        else
        {   // reset Path
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
                // Convert Vector2Int to Vector3, adjust y to z if required by your project's coordinate system
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
            lineRenderer = null; // Ensure to nullify the lineRenderer after destroying the GameObject
        }
        else
        {
            //Debug.Log("PathLine GameObject does not exist or was already destroyed.");
        }
    }
    

    public bool CloseToLocation(Vector2 location)
    {
        //int x1 = (int)transform.position.x;
        //int y1 = (int)(transform.position.y - 0.25f);
        //if (x1 == location.x && y1 == location.y)
        Vector2 currentLoc = new Vector2(transform.position.x, transform.position.y - 0.25f);
        if (Vector2.Distance(currentLoc, location) < 1.2f)
        {
            return true;
        }
        //Debug.Log("x " + x1 + " y " + y1 + " target x: " + location.x + " target y: " + location.y);
        return false;
    }
    public void BreakObstacles()
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

            if ((TargetAngle > 340 || TargetAngle < 20) ||
                (TargetAngle > 160 && TargetAngle < 200))  // horizontal
            {
                RaycastHit2D hitTileDetect1 = Physics2D.Raycast(tileDetect1.position, direction1, rayLength, ground_mask);
                Debug.DrawRay(tileDetect1.position, direction1 * rayLength, Color.red); // Visualize Raycast
                RaycastHit2D hitTileDetect2 = Physics2D.Raycast(tileDetect2.position, direction2, rayLength, ground_mask);
                Debug.DrawRay(tileDetect2.position, direction2 * rayLength, Color.green); // Visualize Raycast
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
            else if (TargetAngle > 65 && TargetAngle < 115) // upward
            {
                RaycastHit2D hitTileDetect3 = Physics2D.Raycast(tileDetect3.position, direction3, rayLength, ground_mask);
                Debug.DrawRay(tileDetect3.position, direction3 * rayLength, Color.blue);
                if (hitTileDetect3.transform != null)   // check if villager is stopped by this block
                {
                    var breakable3 = hitTileDetect3.transform.GetComponent<BreakableObjectController>();
                    if (breakable3 != null)
                    {
                        target = breakable3.gameObject; BreakObstaclesCD = 1f;
                    }
                }
            }
            else if (TargetAngle > 245 && TargetAngle < 295) // downward
            {
                RaycastHit2D hitTileDetect4 = Physics2D.Raycast(tileDetect4.position, direction4, rayLength, ground_mask);
                Debug.DrawRay(tileDetect4.position, direction4 * rayLength, Color.yellow); // Visualize Raycast
                if (hitTileDetect4.transform != null)   // check if villager is stopped by this block
                {
                    var breakable4 = hitTileDetect4.transform.GetComponent<BreakableObjectController>();
                    if (breakable4 != null)
                    {
                        target = breakable4.gameObject; BreakObstaclesCD = 1f;
                    }
                }
            }
            else if ((TargetAngle > 20 && TargetAngle <= 65) || (TargetAngle > 115 && TargetAngle < 160))   // half side upward
            {
                RaycastHit2D hitTileDetect2 = Physics2D.Raycast(tileDetect2.position, direction2, rayLength, ground_mask);
                Debug.DrawRay(tileDetect2.position, direction2 * rayLength, Color.green); // Visualize Raycast
                if (hitTileDetect2.transform != null)
                {
                    var breakable1 = hitTileDetect2.transform.GetComponent<BreakableObjectController>();
                    if (breakable1 != null)
                    {
                        target = breakable1.gameObject; BreakObstaclesCD = 1f;
                    }
                }
                RaycastHit2D hitTileDetect3 = Physics2D.Raycast(tileDetect3.position, direction3, rayLength, ground_mask);
                Debug.DrawRay(tileDetect3.position, direction3 * rayLength, Color.blue);
                if (hitTileDetect3.transform != null)   // check if villager is stopped by this block
                {
                    var breakable3 = hitTileDetect3.transform.GetComponent<BreakableObjectController>();
                    if (breakable3 != null)
                    {
                        target = breakable3.gameObject; BreakObstaclesCD = 1f;
                    }
                }
            }
            else if ((TargetAngle > 200 && TargetAngle <= 245) || (TargetAngle > 295 && TargetAngle < 340)) //half side downward
            {
                RaycastHit2D hitTileDetect1 = Physics2D.Raycast(tileDetect1.position, direction1, rayLength, ground_mask);
                Debug.DrawRay(tileDetect1.position, direction1 * rayLength, Color.red);
                if (hitTileDetect1.transform != null)
                {
                    var breakable2 = hitTileDetect1.transform.GetComponent<BreakableObjectController>();
                    if (breakable2 != null)
                    {
                        target = breakable2.gameObject; BreakObstaclesCD = 1f;
                    }
                }
                RaycastHit2D hitTileDetect4 = Physics2D.Raycast(tileDetect4.position, direction4, rayLength, ground_mask);
                Debug.DrawRay(tileDetect4.position, direction4 * rayLength, Color.yellow); // Visualize Raycast
                if (hitTileDetect4.transform != null)   // check if villager is stopped by this block
                {
                    var breakable4 = hitTileDetect4.transform.GetComponent<BreakableObjectController>();
                    if (breakable4 != null)
                    {
                        target = breakable4.gameObject; BreakObstaclesCD = 1f;
                    }
                }
            }    
        }
    }


    // NEED TEST!!!!!!!!!!!!!!!!!!!!!!!!
    public void CheckStuckAndApplyForces()
    {
        if (isJumping > 0) { return; }

        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.05f, ground_mask);

        if (hitLeft.collider == null && hitCenter.collider == null && hitRight.collider == null)
        { // in air
            //Debug.Log("In the air");
          // Using horizontal rays to check for ground presence
            RaycastHit2D hitHorizontalLeft = Physics2D.Raycast(groundCheckCorners.position, Vector2.left, 0.8f, ground_mask);
            RaycastHit2D hitHorizontalRight = Physics2D.Raycast(groundCheckCorners.position, Vector2.right, 0.8f, ground_mask);
            Debug.DrawLine(
                new Vector3(groundCheckCorners.position.x, groundCheckCorners.position.y, 0),
                new Vector3(groundCheckCorners.position.x, groundCheckCorners.position.y, 0) + new Vector3(Vector2.left.x, Vector2.left.y, 0) * 0.8f,
                Color.red
            );
            Debug.DrawLine(
                new Vector3(groundCheckCorners.position.x, groundCheckCorners.position.y, 0),
                new Vector3(groundCheckCorners.position.x, groundCheckCorners.position.y, 0) + new Vector3(Vector2.right.x, Vector2.right.y, 0) * 0.8f,
                Color.blue
            );
            // Check if either horizontal ray hits a ground layer object
            if (hitHorizontalLeft.collider != null && hitHorizontalLeft.collider.gameObject.layer == LayerMask.NameToLayer("ground") ||
                hitHorizontalRight.collider != null && hitHorizontalRight.collider.gameObject.layer == LayerMask.NameToLayer("ground"))
            {
                //Debug.Log("apply downward force");
                ApplyDownwardForce();
            }
        }
    }

    public void ApplyDownwardForce()
    {
        if (facingright)
        {
            rb.velocity = new Vector2(-10f, rb.velocity.y); // Moves left with a specific speed
            rb.velocity = new Vector2(rb.velocity.x, -10f); // Apply a downward force
        }
        else
        {
            // Apply horizontal force by setting velocity directly
            rb.velocity = new Vector2(10f, rb.velocity.y); // Moves right with a specific speed
            rb.velocity = new Vector2(rb.velocity.x, -10f);
        }
        
    }

}

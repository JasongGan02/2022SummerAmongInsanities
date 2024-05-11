using System;
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
    float SearchTicker = 3f;
    public List<Vector2Int> PathToTarget = new List<Vector2Int>();
    public float PathTicker = 3f;
    public int PathCounter;
    public int bodyHeight = 2;
    public LineRenderer lineRenderer;

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
        if (lineRenderer == null)
        {
            GameObject lineObj = new GameObject("PathLine");
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
        }
    }

    protected override void EnemyLoop()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("villager_idle") == false)
        { SenseFrontBlock(); ChangeCollider("Stand"); }
        else { ChangeCollider("Sit"); }


        if (target == null || (DistanceToTarget(target.transform) > SensingRange && SearchTicker < 0)) { target = WhatToAttack(); SearchTicker = 1f; } // if doesn't have target or searched more than 3 sec
        if (target == null) { patrol(); }
        else
        {
            if (PathToTarget.Count == 0 || PathTicker < 0) { PathFind(); PathTicker = 1f; }   // find a path to target
            else { PathExecute(); } // continue current path
            if (true || villager_sight())   // see target clearly
            {
                if (DistanceToTarget(target.transform) < _atkRange)
                {
                    attack(target.transform, 1f / _atkSpeed); // default:1;  lower -> faster
                }
                else
                {
                    Debug.Log("approaching target " + target.transform); 
                    approach(2.0f * _movingSpeed, target.transform);
                    flip(target.transform);
                }
            }
        }
        ShakePlayerOverHead();
        SearchTicker -= Time.deltaTime; // update ticker for searching
        PathTicker -= Time.deltaTime;  // update ticker for path tracking
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
                var breakable = target.transform.GetComponent<BreakableObjectController>();
                if (breakable != null)
                {
                    breakable.OnClicked(_atkDamage);
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
        if (speed > _movingSpeed)
        {
            animator.Play("villager_run");
        }
        else
        {
            animator.Play("villager_walk");
        }
        
        if (target.position.x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
    }
    void patrol()
    {
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
            if (MoveForwardDepthCheck() == false) { return; }
            patroltime -= Time.deltaTime;
            if (patrolToRight)
            {
                if (MoveForwardDepthCheck() == true) 
                {
                    rb.velocity = new Vector2(_movingSpeed, rb.velocity.y);
                    if (!facingright) { flip(); }
                }
            }
            else
            {
                if (MoveForwardDepthCheck() == true)
                {
                    rb.velocity = new Vector2(-_movingSpeed, rb.velocity.y);
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
        RaycastHit2D hitLeft = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitCenter = Physics2D.Raycast(groundCheckCenter.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitRight = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.05f, ground_mask);
        RaycastHit2D hitFront = Physics2D.Raycast(frontCheck.position, Vector2.left, 0.1f, ground_mask);
        RaycastHit2D hitBack = Physics2D.Raycast(backCheck.position, Vector2.right, 0.1f, ground_mask);

        if (hitCenter.transform != null)
        {
            if ((facingright && rb.velocity.x > 0) || (!facingright && rb.velocity.x < 0))
            {
                if (hitFront.transform != null)
                {
                    if (headCheck())
                    {
                        Jump();
                    }
                }
            }
            else if ((facingright && rb.velocity.x < 0) || (!facingright && rb.velocity.x > 0))
            {
                if (hitBack.transform != null)
                {
                    if (headCheck())
                    {
                        Jump();
                    }
                }
            }
        }
    }
    bool headCheck()
    {
        Vector3 direction = transform.TransformDirection(-Vector3.right);
        RaycastHit2D headRay = Physics2D.Raycast(head.position, direction, 0.34f, ground_mask);
        Debug.DrawRay(head.position, direction * 0.34f, Color.red);        // bottom right
        if (headRay.collider != null && headRay.collider.gameObject.tag == "ground")
        {
            //Debug.Log("headCheck return false");
            return false;
        }

        return true;
    }
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x * 1.0f, _jumpForce);
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
        if (status == "Stand")
        {
            //boxCollider.size = new Vector2(0.1875544f, 1.0f);
            capsuleCollider.offset = new Vector2(0.02169657f, 0.0001685619f);
            capsuleCollider.size = new Vector2(0.1895304f, 1.017319f);
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
        RaycastHit2D hit = Physics2D.Raycast(frontDepthDetector, Vector2.down, 3f, ground_mask);
        if (hit.collider != null) { return true; }

        return false;
    }

    new void ShakePlayerOverHead()
    {
        if (System.Math.Abs(player.transform.position.x - transform.position.x) < 0.3f)
        {
            if (System.Math.Abs(player.transform.position.y - transform.position.y) < 2f)
            {
                Debug.Log("shaking the player over head");
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

    public float DistanceToPoint(Vector2Int a)
    {
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);

        // Convert Vector2Int 'a' to Vector2 for distance calculation
        Vector2 targetPosition = new Vector2(a.x, a.y);

        // Calculate and return the Euclidean distance
        return Vector2.Distance(currentPosition, targetPosition);
    }

    void approach(float speed, int x, int y)
    {
        if (speed > _movingSpeed)
        {
            animator.Play("villager_run");
        }
        else
        {
            animator.Play("villager_walk");
        }
        if (MoveForwardDepthCheck() == false) { return; }
        if (x > transform.position.x) { rb.velocity = new Vector2(speed, rb.velocity.y); }
        else { rb.velocity = new Vector2(-speed, rb.velocity.y); }
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









    public void PathExecute()       // execute path to target
    {
        //Debug.Log("Path found with steps: " + path.Count);
        if (DistanceToTarget(target.transform) < _atkRange) {
            PathToTarget.Clear(); PathCounter = 0; Debug.Log("close to target!"); return;
            //return;
        }
        DrawPath();
        if (PathCounter < PathToTarget.Count)
        {
            approach(2 * _movingSpeed, PathToTarget[PathCounter].x, PathToTarget[PathCounter].y);
            //Debug.Log("now approach position " + PathToTarget[PathCounter].x + " , " + PathToTarget[PathCounter].y);
            if (DistanceToPoint(PathToTarget[PathCounter]) < _atkRange ) // needs testing
            {
                Debug.Log("tempTarget " + PathToTarget[PathCounter]);
                TileObject tile = WorldGenerator.GetDataFromWorldPos(PathToTarget[PathCounter]);
                //Vector2Int temp = new Vector2Int(3, 66);
                //TileObject tile = WorldGenerator.GetDataFromWorldPos(temp);
                Debug.Log(tile);
                if (tile != null)
                {
                    Debug.Log(" will attack this tile" + PathToTarget[PathCounter]);
                    target = GetTileGameObject(PathToTarget[PathCounter]);
                }
                else
                {
                    PathCounter++;
                }
            }
        }
        else
        {   // reset Path
            PathToTarget.Clear();
            PathCounter = 0;
        }
    }

    public void PathFind()  // only find path, not execute
    {
        if (target == null || DistanceToTarget(target.transform) < _atkRange) { PathToTarget.Clear(); PathCounter = 0; return; }

        float TX = target.transform.position.x; float TY = target.transform.position.y; // target x y
        float CX = transform.position.x; float CY = transform.position.y; // currect x y

        // Convert positions to integers for indexing
        int startX = Mathf.FloorToInt(CX);
        int startY = Mathf.FloorToInt(CY);
        int endX = Mathf.FloorToInt(TX);
        int endY = Mathf.FloorToInt(TY);

        // Ensure minimum and maximum are correct regardless of direction
        int minX = Mathf.Min(startX, endX) - 3; // increase fault tolerance
        int maxX = Mathf.Max(startX, endX) + 3;
        int minY = Mathf.Min(startY, endY) - 2;
        int maxY = Mathf.Max(startY, endY) + 2;
        //Debug.Log("min x" + minX + "min y" + minY + "max x" + maxX + "max y" + maxY);
        // Create 2D array to store health values
        int[,] healthGrid = new int[maxX - minX + 1, maxY - minY + 1];

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                TileObject tile = WorldGenerator.GetDataFromWorldPos(position);
                if (tile != null)
                {
                    healthGrid[x - minX, y - minY] = 1 + tile.getHP();
                }
                else
                {
                    if (y > minY)
                    {
                        Vector2Int tempPosition = new Vector2Int(x, y-1);
                        TileObject checkTemp = WorldGenerator.GetDataFromWorldPos(tempPosition);
                        if (checkTemp != null) // there is a tile below, so attainable
                        {
                            healthGrid[x - minX, y - minY] = 1;
                        }
                        else
                        {
                            healthGrid[x - minX, y - minY] = 999; // there is an air block below, so villager can't reach this block position
                        }
                    }
                    else // y = minY
                    {
                        healthGrid[x - minX, y - minY] = 1;
                    }
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
            PathToTarget = path;    // update Path
            PathCounter = 0;         // reset ticker
            //Debug.Log("start position " + startX + "  " + startY + " target position " + endX + " " + endY);
            string pathString = string.Join(", ", path);
            Debug.Log(pathString);
        }
        else
        {
            Debug.Log("No path found.");
        }


        //// Calculate relative positions
        //int relativeStartX = startX - minX;
        //int relativeStartY = startY - minY;
        //int relativeEndX = endX - minX;
        //int relativeEndY = endY - minY;

        //// Example usage
        //Debug.Log($"Villager is at relative position ({relativeStartX}, {relativeStartY}) with HP: {healthGrid[relativeStartX, relativeStartY]}");
        //Debug.Log($"Target is at relative position ({relativeEndX}, {relativeEndY}) with HP: {healthGrid[relativeEndX, relativeEndY]}");

        //// Calculate the dimensions of the grid
        //int width = healthGrid.GetLength(0) - 1;
        //int height = healthGrid.GetLength(1) - 1;

        //// Log health at four corners of the grid
        //Debug.Log($"Health at top left corner ({minX}, {maxY}): {healthGrid[0, height]}");
        //Debug.Log($"Health at top right corner ({maxX}, {maxY}): {healthGrid[width, height]}");
        //Debug.Log($"Health at bottom left corner ({minX}, {minY}): {healthGrid[0, 0]}");
        //Debug.Log($"Health at bottom right corner ({maxX}, {minY}): {healthGrid[width, 0]}");
    }

    //public static List<Vector2Int> AstarPath(Vector2Int start, Vector2Int goal, int[,] costGrid, int width, int height, int minX, int minY)
    //{
    //    List<Vector2Int> Paths = new List<Vector2Int>();
        
    //}
    public class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public int G; // Cost from start to the current node
        public int H; // Estimated cost from the current node to the end
        public int F; // Total cost (G + H)

        public Node(Vector2Int pos, Node parent = null)
        {
            Position = pos;
            Parent = parent;
            G = H = F = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node node)
                return Position.Equals(node.Position);
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    public static List<Vector2Int> AstarPath(Vector2Int start, Vector2Int goal, int[,] costGrid, int width, int height, int minX, int minY)
    {
        Node startNode = new Node(start);
        Node endNode = new Node(goal);
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            int currentIndex = 0;

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].F < currentNode.F)
                {
                    currentNode = openList[i];
                    currentIndex = i;
                }
            }

            openList.RemoveAt(currentIndex);
            closedList.Add(currentNode);

            if (currentNode.Equals(endNode))
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Node current = currentNode;
                while (current != null)
                {
                    path.Add(new Vector2Int(current.Position.x + minX, current.Position.y + minY));
                    current = current.Parent;
                }
                path.Reverse();
                return path;
            }

            List<Node> children = new List<Node>();
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var direction in directions)
            {
                Vector2Int nodePosition = currentNode.Position + direction;
                if (nodePosition.x >= 0 && nodePosition.x < width && nodePosition.y >= 0 && nodePosition.y < height)
                {
                    Node newNode = new Node(nodePosition, currentNode);
                    if (!closedList.Contains(newNode))
                    {
                        children.Add(newNode);
                    }
                }
            }

            foreach (var child in children)
            {
                if (!openList.Contains(child))
                {
                    child.G = currentNode.G + costGrid[child.Position.x, child.Position.y];
                    child.H = (child.Position.x - endNode.Position.x) * (child.Position.x - endNode.Position.x) + (child.Position.y - endNode.Position.y) * (child.Position.y - endNode.Position.y);
                    child.F = child.G + child.H;

                    openList.Add(child);
                }
                else
                {
                    int newG = currentNode.G + costGrid[child.Position.x, child.Position.y];
                    if (newG < child.G) // Check if new path to child is better
                    {
                        child.G = newG;
                        child.F = child.G + child.H;
                        // Since the child's cost changed, it should be sorted again in the open list, but for simplicity, we just update the node.
                    }
                }
            }
        }

        return new List<Vector2Int>(); // Return empty if no path is found
    }

    public void DrawPath()
    {
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
    public void LogHealthGrid(int[,] healthGrid)
    {
        int width = healthGrid.GetLength(0); // Getting the first dimension size
        int height = healthGrid.GetLength(1); // Getting the second dimension size
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Loop through each row
        for (int y = height - 1; y >= 0; y--)
        {
            // Loop through each column in the current row
            for (int x = 0; x < width; x++)
            {
                // Append each cell value followed by a space for separation
                sb.AppendFormat("{0,5}", healthGrid[x, y]);
            }
            sb.AppendLine(); // Add a newline after each row
        }

        Debug.Log(sb.ToString()); // Log the entire grid at once
    }

}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Constants;
using System;
using System.Linq;
using System.Linq.Expressions;
using static UnityEngine.GraphicsBuffer;
using Mono.Cecil;

public abstract class EnemyController : CharacterController
{
    protected float SensingRange;
    public bool GroupApproaching = false;
    protected static int globalEnemyLevel = 1;
    public GameObject tempTarget;
    public Collider2D[] colliders;

    //run-time variables
    public TowerContainer towerContainer;   // Changed from protected to public
    protected GameObject player;
    protected GameObject nearestTower;
    protected float timer;
    protected Transform NearestTowerTransform;

    protected Rigidbody2D rb;
    protected bool isFindTower;
    protected bool isTouchTower;
    protected bool isFindPlayer;
    protected bool isTouchPlayer;

    protected int layerMask = (1 << 8) | (1 << 9) | (1 << 10);

    Type type;

    

    protected override void Awake()
    {
        //towerContainer = FindObjectOfType<TowerContainer>();
        base.Awake();
        timer = 0;
        
    }


    public void LevelUp()
    {
        Reinitialize();
    }



    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
    }

    protected override void Update()
    {
        base.Update();
        if (player == null) 
        { 
            player = GameObject.Find("Player"); 
        }
        if (this.transform.position.y < -400) death();
        SetEnemyContainer();
        EnemyLoop();
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }
    
    public override void TakeDamage(float amount, IDamageSource damageSource)
    {
        base.TakeDamage(amount, damageSource);
        _audioEmitter.PlayClipFromCategory("InjureEnemy");
    }
    
    protected bool IsTowerSensed()
    {
        if (towerContainer == null) { /*Debug.Log("0");*/ return false; }  // Nathan's only change
        
        UpdateNearestTower();
        if(NearestTowerTransform == transform)
        {

            //Debug.Log("1");
            return false; 

        }
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        //Debug.Log("distance: " + distance.ToString()); 
        if(distance <= SensingRange)
        {
            //Debug.Log("2");
            return true;
        }else
        {
            //Debug.Log("3");
            return false;
        }
    }

    protected bool IsTowerInAtkRange()
    {
        float distance = CalculateDistanceFromEnemyToTower(NearestTowerTransform);
        if(distance <= _atkRange)
        {
            return true;
        }else
        {
            return false;
        }
    }

    protected bool IsPlayerSensed()
    {
        if (player == null) 
        { 
            return false;
        }

        float distance = CalculateDistanceToPlayer();
        if(distance <= SensingRange)
        {
            return true;
        }else
        {
            return false;
        }
        
    }

    protected bool IsPlayerInAtkRange()
    {
        float distance = CalculateDistanceToPlayer();
        if(distance <= _atkRange)
        {
            return true;
        }else
        {
            timer =0;
            return false;
        }
    }

    protected abstract void EnemyLoop();
    
    // protected abstract void Patrol();

    protected void ApproachingTarget(Transform target_transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, target_transform.position, _movingSpeed*Time.deltaTime);
        SenseFrontBlock();
        // transform directiron change
        if(target_transform.position.x >= transform.position.x)
        {
            transform.eulerAngles = new Vector3(0,180,0);
        }else{
            transform.eulerAngles = new Vector3(0,0,0);
        }
    }

    // Shooting a 2D rayline, if sense a ground tag, then jump
    protected void SenseFrontBlock()    
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
                Vector2 up_force = new Vector2(0, _jumpForce);
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
    }

    protected void UpdateNearestTower() 
    {
        Transform[] towerTransforms = towerContainer.GetComponentsInChildren<Transform>();
        Transform nearest_Transform = towerTransforms[0];
        float min_distance = Vector2.Distance(towerTransforms[0].position, transform.position);
        foreach(Transform e in towerTransforms)
        {
            Debug.Log(e);
            if (e.transform.gameObject.CompareTag("tower"))
            {
                Debug.Log(e);

                if ((Vector2.Distance(e.position, transform.position) < min_distance) && (e.name != "FiringPoint"))
                {
                    nearest_Transform = e;
                    min_distance = Vector2.Distance(e.position, transform.position);
                }
            }
        }
        NearestTowerTransform = nearest_Transform;
    }

    protected float CalculateDistanceFromEnemyToTower(Transform towerTransform)
    {
        Vector3 enemyPosition = transform.position;
        Vector3 towerPosition = towerTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }

    protected float DistanceToTarget(Transform target)
    {
        Vector2 targetPosition = target.position;
        Vector2 transformPosition = transform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((targetPosition.x - transformPosition.x), 2) + Mathf.Pow((targetPosition.y - transformPosition.y), 2));
        return distance;
    }

    protected float CalculateDistanceToPlayer()
    {
        Vector3 player_position = player.gameObject.transform.position;
        float x1 = transform.position.x;
        float y1 = transform.position.y;
        float x2 = player_position.x;
        float y2 = player_position.y;
        float distance = Mathf.Sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));

        return distance;
    }
    
    protected void attack()
    {
        if(timer >= _atkSpeed)
        {
            ApplyDamage(player.GetComponent<CharacterController>());
            timer = 0f;
        }
    }

    public GameObject WhatToAttack()
    {
        GameObject target = null;
        if (Hatred.Count > 0)
        {
            //Debug.Log(Hatred.Count);
            for (int i = 0; i < Hatred.Count; i++)
            {
                if (CouldSense(Hatred[i].name, SensingRange))
                {
                    return tempTarget;
                }
            }
        }
        else { //Debug.Log("Hatred is empty");
               }
        return target;
    }

    public bool CouldSense(string name, float range)
    {
        type = Type.GetType(name);

        colliders = Physics2D.OverlapCircleAll(transform.position, range, layerMask);
        //Debug.Log(colliders.Length);
        foreach (Collider2D collider in colliders)
        {
            MonoBehaviour[] components = collider.gameObject.GetComponents<CharacterController>();
            foreach (MonoBehaviour component in components)
            {
                if (component != null )
                {
                    if (type.IsAssignableFrom(component.GetType()) || type.Equals(component.GetType()))
                    {
                        tempTarget = collider.gameObject;
                        return true;
                    }
                }
            }

        }
        //Debug.Log("didn't find target");
        return false; 
    }

    public Vector3 GetPosition()
    {
        return transform.position; // Return the GameObject's position
    }

    protected void SetEnemyContainer()
    {
        int chunkCoord = WorldGenerator.GetChunkCoordsFromPosition(transform.position);
        if (WorldGenerator.ActiveChunks.ContainsKey(chunkCoord))
            transform.SetParent(WorldGenerator.ActiveChunks[chunkCoord].transform.Find("MobContainer").Find("EnemyContainer"), true);
    }

    public void ShakePlayerOverHead()
    {
        if (Math.Abs(player.transform.position.x - this.transform.position.x) < 0.1f)
        {
            if (Math.Abs(player.transform.position.y - this.transform.position.y) < 0.3f)
            {
                Debug.Log("shaking the player over head");
                if (UnityEngine.Random.Range(0f, 1f) <= 0.5f)
                {
                    this.rb.velocity = new Vector2(-2, rb.velocity.y);
                }
                else
                {
                    this.rb.velocity = new Vector2(2, rb.velocity.y);
                }
            }
        }
    }
    public abstract void MoveTowards(Transform targetTransform);


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
    public static int CalculatePathCost(List<Vector2Int> path, int[,] costGrid, int minX, int minY)
    {
        int totalCost = 0;

        // Iterate over the path to sum up the costs
        foreach (var point in path)
        {
            // Adjust the position in the grid by subtracting the minimum X and Y
            int gridX = point.x - minX;
            int gridY = point.y - minY;

            // Add the cost of the current path point to the total cost
            totalCost += costGrid[gridX, gridY];
        }

        return totalCost;
    }
    public List<Vector2> PathPointToCenter(List<Vector2Int> path)
    {
        List<Vector2> convertedPath = new List<Vector2>();

        foreach (Vector2Int point in path)
        {
            Vector2 newPoint = new Vector2(point.x + 0.5f, point.y + 0.5f);
            convertedPath.Add(newPoint);
        }

        return convertedPath;
    }
    public int DistanceToGround(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (y == 0 || WorldGenerator.GetDataFromWorldPos(position) != null)
        {
            return 0;
        }
        for (int i = y - 1; i >= 0; i--)
        {
            Vector2Int position1 = new Vector2Int(x, i);
            if (WorldGenerator.GetDataFromWorldPos(position1) != null)
            {
                return (y - i);
            }
        }
        return -1;  // false value: input y is invalid
    }
    public bool IsNeighborOfTile(int x, int y)
    {   // check the left and right side of position is a tile object
        Vector2Int position = new Vector2Int(x + 1, y);
        TileObject tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        position = new Vector2Int(x - 1, y);
        tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        return false;
    }
    public bool IsLadder(int x, int y) { return false; }
    public bool IsNeighborTileReachable(int x, int y)
    {
        Vector2Int position = new Vector2Int(x + 1, y - 1);
        TileObject tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        position = new Vector2Int(x - 1, y - 1);
        tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        return false;
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
                sb.AppendFormat("{0,-2}  ", healthGrid[x, y]);
            }
            sb.AppendLine(); // Add a newline after each row
        }

        Debug.Log(sb.ToString()); // Log the entire grid at once
    }



    
}

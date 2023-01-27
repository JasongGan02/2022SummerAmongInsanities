using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Lady : MonoBehaviour
{
    public GameObject arrowPrefab; // reference to the arrow prefab
    public Transform arrowSpawnPoint; // reference to the point where the arrow will be instantiated
    public Transform player; // reference to the player's transform
    public GameObject Player; // reference to the player
    bool isFindTower;
    public float fireRate; // rate at which the archer fires arrows
    public float range = 10f; // the range at which the archer will fire arrows
    private float nextFire; // the time at which the archer can fire again
    private float nextMove; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire
    private bool canMove = true;
    private TowerContainer towerContainer;



    public float health = 100f;
    public float speed = 1f;
    public LayerMask wallLayer;
    private Rigidbody2D rb;
    private float nextJump;
    private bool canJump = true;


    




    void Start()
    {
        arrowSpawnPoint = transform;
        rb = gameObject.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Player = GameObject.FindGameObjectWithTag("Player");
        wallLayer = LayerMask.GetMask("ground");
        towerContainer = FindObjectOfType<TowerContainer>();

    }

    void Update()
    {
        // Escape from the tower
        //

        // Approaches and escapes from the player
        if (transform.position.x - player.position.x < 3 && transform.position.x - player.position.x > 0)
        {
            canFire = false;
        }
        if (transform.position.x - player.position.x < 3 && transform.position.x - player.position.x > 0)
        {
            canFire = false;
        }
        Vector2 direction = (player.position - transform.position);
        if (direction.x > 0)
        {
            transform.right = Vector2.left;
        }
        else if (direction.x < 0)
        {
            transform.right = Vector2.right;
        }

        if (canJump)
        {
            SenseFrontBlock();
            nextJump = Time.time + 5f;
        }
        if (Time.time > nextJump)
        {
            canJump = true;
        }
        
        if (canMove)
        {
            if (Vector2.Distance(transform.position, player.position) < 3f)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -speed * Time.deltaTime);
            }
            if (Vector2.Distance(transform.position, player.position) > 3f && Vector2.Distance(transform.position, player.position) < 10f)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
            }
            if (Vector2.Distance(transform.position, player.position) >= 10f)
            {
                // idle state
            }
        }
        



        if ((Vector2.Distance(transform.position, player.position) <= range) && (Vector2.Distance(transform.position, player.position) > 3))
        {
            // Check if the archer can fire
            if (canFire)
            {
                // Fire an arrow
                FireArrow();

                canMove = false;

                // Set the next fire time
                nextFire = Time.time + fireRate;
                nextMove = Time.time + 1f;

                // Set the canFire flag to false
                canFire = false;
            }

            // Check if the fire rate has passed
            if (Time.time > nextFire)
            {
                // Set the canFire flag to true
                canFire = true;
            }
            if (Time.time > nextMove)
            {
                canMove = true;
            }
        }
    }


    void SenseFrontBlock()
    {
        Vector2 shooting_direction = (player.position - transform.position);
        Vector3 origin = transform.position - new Vector3(0,0.2f,0);
        Debug.DrawRay(origin, shooting_direction, Color.green);

        LayerMask ground_mask = LayerMask.GetMask("ground");
        RaycastHit2D hit = Physics2D.Raycast(origin, shooting_direction, 1.5f, ground_mask);
        if(hit.collider != null && hit.collider.gameObject.tag == "ground")
        {
            Jump();
        }
    }

    void Jump()
    {
        Vector2 up_force = new Vector2(0,12.5f);
        gameObject.GetComponent<Rigidbody2D>().AddForce(up_force);
    }


    void FireArrow(){
    // Instantiate the arrow
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    Transform FindNearestTower()
    {
        Transform[] towerTransforms = towerContainer.GetComponentsInChildren<Transform>();
        Transform nearest_Transform = transform;
        float min_distance = 0;
        foreach(Transform e in towerTransforms)
        {
            if(e==towerTransforms[0])
            {
                isFindTower = false;
                continue;
            }
            // for the first tower
            if(e==towerTransforms[1])
            {
                isFindTower = true;
                min_distance = CalculateDistanceFromEnemyToTower(e);
                nearest_Transform = e;
                continue;
            }

            // for other towers
            float distance = CalculateDistanceFromEnemyToTower(e);
            if(distance < min_distance)
            {
                min_distance = distance;
                nearest_Transform = e;
            }
        }

        return nearest_Transform;
    }

        float CalculateDistanceFromEnemyToTower(Transform towerTransform)
    {
        Vector3 enemyPosition = transform.position;
        Vector3 towerPosition = towerTransform.position;
        float distance = Mathf.Sqrt(Mathf.Pow((towerPosition.x - enemyPosition.x),2) + Mathf.Pow((towerPosition.y - enemyPosition.y),2));
        return distance;
    }

    
}

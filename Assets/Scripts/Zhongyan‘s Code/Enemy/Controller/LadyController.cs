using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LadyController : EnemyController
{
    public GameObject arrowPrefab; // reference to the arrow prefab
    public Transform arrowSpawnPoint; // reference to the point where the arrow will be instantiated
    public Transform Player; // reference to the player
    public float fireRate; // rate at which the archer fires arrows
    public float range = 10f; // the range at which the archer will fire arrows
    private float nextFire; // the time at which the archer can fire again
    private float nextMove; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire
    private bool canMove = true;



    public float health = 100f;
    public float speed = 1.5f;
    public LayerMask wallLayer;
    private Rigidbody2D rb;
    private float nextJump;
    private bool canJump = true;


    




    void Start()
    {
        arrowSpawnPoint = transform;
        rb = gameObject.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        Player = player.transform;
        wallLayer = LayerMask.GetMask("ground");
        towerContainer = FindObjectOfType<TowerContainer>();

    }

    void Update()
    {
        EnemyLoop();
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



    protected override void EnemyLoop()
    {
        // Escape from the tower
        //

        // Approaches and escapes from the player

        Vector2 direction = (Player.position - transform.position);
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
            if (Vector2.Distance(transform.position, Player.position) < 3f)
            {
                transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, -speed * Time.deltaTime);
            }
            if (Vector2.Distance(transform.position, Player.position) > 3f && Vector2.Distance(transform.position, Player.position) < 10f)
            {
                transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, speed * Time.deltaTime);
            }
            if (Vector2.Distance(transform.position, Player.position) >= 10f)
            {
                // idle state
            }
        }
        



        if (Vector2.Distance(transform.position, Player.position) <= range)
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

        if (this.IsTowerSensed()){
            transform.position = Vector2.MoveTowards(transform.position, NearestTowerTransform.transform.position, -speed * Time.deltaTime);

        }
    }
}

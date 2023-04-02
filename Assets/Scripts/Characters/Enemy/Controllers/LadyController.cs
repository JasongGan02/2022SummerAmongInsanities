using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LadyController : EnemyController
{

    private GameObject arrow;
    public Animator animator;
    
    private Transform arrowSpawnPoint; // reference to the point where the arrow will be instantiated
    private float nextFire; // the time at which the archer can fire again
    private float nextMove; // the time at which the archer can fire again
    private bool canFire = true; // flag to check if the archer can fire
    private bool canMove = true;
    private GameObject arrowPrefab; // reference to the arrow prefab


    private LayerMask wallLayer;
    private Rigidbody2D rb;
    private float nextJump;
    private bool canJump = true;



    void Start()
    {
        arrowSpawnPoint = transform;
        animator = GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        wallLayer = LayerMask.GetMask("ground");
        towerContainer = FindObjectOfType<TowerContainer>();
    }



    void FireArrow(){
     //Instantiate the arrow
        arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
    }



    protected override void EnemyLoop()
    {
        // Escape from the tower
        //

        // Approaches and escapes from the player

        Vector2 direction = (player.transform.position - transform.position);
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
            if (Vector2.Distance(transform.position, player.transform.position) < 3f)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -MovingSpeed * Time.deltaTime);
            }
            if (Vector2.Distance(transform.position, player.transform.position) > 3f && Vector2.Distance(transform.position, player.transform.position) < 10f)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, MovingSpeed * Time.deltaTime);
            }
            if (Vector2.Distance(transform.position, player.transform.position) >= 10f)
            {
                // idle state
            }
        }


        // Player Taken Damage
        if (arrow != null){
            if (Vector2.Distance(arrow.transform.position, player.transform.position)< 0.3){
                player.GetComponent<PlayerController>().takenDamage(AtkDamage);
                Destroy(arrow);
            }
        }
        
        



        if (Vector2.Distance(transform.position, player.transform.position) <= AtkRange)
        {
            // Check if the archer can fire
            if (canFire)
            {
                // Fire an arrow
                FireArrow();

                canMove = false;

                // Set the next fire time
                nextFire = Time.time + AtkInterval;
                nextMove = Time.time + 1f;

                // Set the canFire flag to false
                canFire = false;
                animator.SetBool("canFire", false);
            }

            // Check if the fire rate has passed
            if (Time.time > nextFire)
            {
                // Set the canFire flag to true
                canFire = true;
                animator.SetBool("canFire", true);

            }
            if (Time.time > nextMove)
            {
                canMove = true;
            }
        }

        if (IsTowerSensed()){
            transform.position = Vector2.MoveTowards(transform.position, NearestTowerTransform.transform.position, -MovingSpeed * Time.deltaTime);

        }
    }
}

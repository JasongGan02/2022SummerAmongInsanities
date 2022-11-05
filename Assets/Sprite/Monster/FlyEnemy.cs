using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyEnemy : MonoBehaviour
{

    public float speed;
    public float sprint;
    private float waitTime;
    public float startWaitTime;
    public bool facingRight = true;

    public Transform moveTo;
    public Transform player;
    public float min_x;
    public float max_x;
    public float min_y;
    public float max_y;


    // Start is called before the first frame update
    void Start()
    {
        waitTime = startWaitTime;

        moveTo.position = new Vector2(Random.Range(min_x, max_x), Random.Range(min_y, max_y));
    }

    // Update is called once per frame
    void Update()
    {   

        if (Vector2.Distance(transform.position, player.position) < 4)
        {
            ChasePlayer();
            Stab();
        }
        else
        {
            Patrol();
            // If patrol around, it faces toward the destination. Otherwise, it faces the player
            if (moveTo.position.x < transform.position.x && facingRight || moveTo.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
        }
    }

    // stab attack
    void Stab()
    {
        if (Vector2.Distance(transform.position, player.position) < 1f)
        {
            Vector2 startingPoint = transform.position;
            transform.position = Vector2.MoveTowards(transform.position, player.position, sprint * Time.deltaTime);
            if (Vector2.Distance(transform.position, player.position) < 0.2f)
            {
                transform.position = Vector2.MoveTowards(transform.position, startingPoint, sprint * Time.deltaTime);
            }
        }
    }


    // flip the enemy
    void Flip()
    {
        facingRight = !facingRight;

        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }

    // enemy follows player
    void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, sprint * Time.deltaTime);
        if (player.position.x > transform.position.x && !facingRight || player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    // patrol around
    void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, moveTo.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, moveTo.position) < 0.2f)
        {
            if (waitTime <= 0)
            {
                moveTo.position = new Vector2(Random.Range(min_x, max_x), Random.Range(min_y, max_y));
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

}
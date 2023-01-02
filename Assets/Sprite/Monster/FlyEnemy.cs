using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlyEnemy : MonoBehaviour
{
    public float speed;
    public float sprint;
    private float waitTime;
    public float startWaitTime;
    public bool facingRight = true;

    private Transform moveTo;
    private Transform player;
    public float min_x;
    public float max_x;
    public float min_y;
    public float max_y;

    private bool planned;
    private bool prepare_dash;
    private bool is_dashing;
    private Vector2 dash_start;
    private Vector2 dash_end;
    private Vector2 stop_point;
    private Animator animator;

    [SerializeField] int atk_damage;
    [SerializeField] float atk_interval;
    float timer;
    private bool attacked;

    [SerializeField] TrailRenderer Tr;
    [SerializeField] ParticleSystem Ps;

    // Start is called before the first frame update
    void Start()
    {
        GameObject destination = new GameObject("newObject");
        moveTo = destination.transform;
          // initial moveTo
        player = GameObject.Find("Player").transform;   // set the player as the target
        waitTime = startWaitTime;

        moveTo.position = new Vector2(Random.Range(min_x, max_x), Random.Range(min_y, max_y));

        planned = false;
        prepare_dash = true;
        is_dashing = false;
        timer = 0;
        attacked = false;
        stop_point = player.position;
        Ps.Stop();
    }


    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        animator.SetBool("isAttack", Vector2.Distance(transform.position, player.position) < 5f || planned);
        if (Vector2.Distance(transform.position, player.position) < 5f || planned)
        {
            DashAttack();
            
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

    // dash to player and cause damage when contact
    void DashAttack()
    {
        if (prepare_dash)           // go to the left or right start-dash point of the player
        {
            if (!planned) {         // set attack route
                PlanRoute();
                planned = true;
            }                   
            transform.position = Vector2.MoveTowards(transform.position, dash_start, speed * 2 * Time.deltaTime);
            if (CloseEnough(transform.position, dash_start))
            {
                Ps.Play();
                timer += Time.deltaTime;
                if (timer < 0.5f) { }
                else {
                    prepare_dash = false;
                    is_dashing = true;
                }
            }
        }
        else if (is_dashing)        // dash through the player and attack
        {
            Ps.Stop();
            transform.position = Vector2.MoveTowards(transform.position, dash_end, sprint * 3 * Time.deltaTime);
            Tr.emitting = true;
            if(Vector2.Distance(transform.position, player.position) < 0.4f && !attacked)
            {
                player.GetComponent<PlayerAttributes>().DecreaseHealth(atk_damage);
                attacked = true;
            }
            if (CloseEnough(transform.position, dash_end))
            {
                Tr.emitting = false;
                is_dashing = false;
            }
        }
        else                        // return to the higher point and attack again
        {
            if (CloseEnough(transform.position, stop_point))
            {
                timer = 0;
                prepare_dash = true;
                attacked = false;
                planned = false;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, stop_point, speed * Time.deltaTime);
            }
        }
        if (player.position.x < transform.position.x && facingRight || player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        
    }
    // make attack plan (dash_start -> dash_End -> stop_point -> dash_start...)
    void PlanRoute()
    {
        dash_start = player.position;
        dash_end = player.position;
        stop_point = player.position;
        if (player.position.x > transform.position.x)           // player is on the right side
        {
            dash_start.x -= 4;
            dash_start.y += 0.3f;
            dash_end.x += 3;
            dash_end.y -= 0.3f;
            stop_point.x += 1;
            stop_point.y += 3.5f;
        }
        else                                                    // player is on the left side
        {
            dash_start.x += 4;
            dash_start.y += 0.3f;
            dash_end.x -= 3;
            dash_end.y -= 0.3f;
            stop_point.x -= 1;
            stop_point.y += 3.5f;
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


    // check for collision with player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("contacted");
        }
    }

    // check if two points are close enough
    bool CloseEnough(Vector2 first, Vector2 second)
    {
        if (Vector2.Distance(first,second) < 0.2f) { return true; }
        return false;
    }

    


}
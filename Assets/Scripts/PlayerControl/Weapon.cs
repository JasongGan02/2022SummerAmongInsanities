using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float speed;


    public bool facingRight = true;
    
    private Transform player;

 
 

    [SerializeField] int atk_damage;
    [SerializeField] float atk_interval;
    
    private bool attacked;

    [SerializeField] TrailRenderer Tr;



    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.Find("Player").transform;
       
        
        attacked = false;
      
        
    }

    

    // Update is called once per frame
    void Update()
    {

        Patrol();
        // If patrol around, it faces toward the destination. Otherwise, it faces the player
        if (player.position.x < transform.position.x && facingRight || player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }

        if (Input.GetMouseButton(0))
        {
            attack();
            Tr.emitting = true;

        }
        else
        {
            Tr.emitting = false;
        }
       

    }

    // dash to player and cause damage when contact
    void attack()
    {
        if (facingRight)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position + new Vector3(0, 1, 0), speed * 2 * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position - new Vector3(0, 1, 0), speed * 2 * Time.deltaTime);
        }
    }
   
    // make attack plan (dash_start -> dash_End -> stop_point -> dash_start...)


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
        if (Vector2.Distance(transform.position, player.position) > 0.8)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }


        if (Vector2.Distance(transform.position, player.position) < 0.2)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2,transform.position.z);
        }
    }


    //check for collision with player
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
        if (Vector2.Distance(first, second) < 0.2f) { return true; }
        return false;
    }




}
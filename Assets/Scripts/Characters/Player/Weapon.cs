using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{

    Playermovement playermovement;

    public float speed;
    [SerializeField] public GameObject player;



    public float magnitude = 1f;
    public float frequency = 10f;
    public float offset = 0f;
    

    void Awake()
    {
        playermovement = GameObject.Find("Player").GetComponent<Playermovement>();
        
    }
 

    // Start is called before the first frame update
   

    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            attack();
            playermovement.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;


        }
        else
        {
            Patrol();
            
        }
       

    }

    // dash to player and cause damage when contact
    void attack()
    {
        
        if (playermovement.facingRight)
        {

            transform.position = player.transform.position + new Vector3(1f, 0, 0) + transform.up * Mathf.Sin(Time.time * frequency + offset) * magnitude;


        }
        else
        {

            transform.position = player.transform.position - new Vector3(1f, 0, 0) -  transform.up * Mathf.Sin(Time.time * frequency + offset) * magnitude;

        }                       
    }
   
    // make attack plan (dash_start -> dash_End -> stop_point -> dash_start...)


    // flip the enemy
    void Flip()
    {
        

        Vector3 transformScale = transform.localScale;
        transformScale.y *= -1;
        transform.localScale = transformScale;
    }

    // enemy follows player


    // patrol around
    void Patrol()
    {
        if (Vector2.Distance(transform.position, player.transform.position) > 0.8)
        {
            transform.position = Vector2.MoveTowards(transform.position,player.transform.position, speed * Time.deltaTime);
        }


        if (Vector2.Distance(transform.position, player.transform.position) < 0.2)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2,transform.position.z);
        }

        if (playermovement.facingRight && (transform.localScale.y < 0) || !playermovement.facingRight && (transform.localScale.y > 0))
        {
            Flip();
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
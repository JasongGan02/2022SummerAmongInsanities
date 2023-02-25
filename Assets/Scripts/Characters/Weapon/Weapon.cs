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
    public float maxSpeed = 10f; // Set the maximum speed of the object
    public float slowDownDistance = 2f; // Set the distance from the player where the object should start slowing down

    void Awake()
    {
        playermovement = GameObject.Find("Player").GetComponent<Playermovement>();
        
    }
 

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

    public virtual void attack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float speed = maxSpeed; // Set the default speed to the maximum speed

        if (playermovement.facingRight)
        {
            if (transform.position.x > player.transform.position.x + slowDownDistance)
            {
                // Gradually slow down as the object approaches the slow down distance
                float percentSlowDown = Mathf.Clamp01((transform.position.x - (player.transform.position.x + slowDownDistance)) / slowDownDistance);
                speed = maxSpeed * (1 - percentSlowDown);
            }

            transform.position = player.transform.position + new Vector3(1f, 0, 0) + transform.up * Mathf.Sin(Time.time * frequency) * magnitude * speed;
        }
        else
        {
            transform.position = player.transform.position - new Vector3(1f, 0, 0) - transform.up * Mathf.Sin(Time.time * frequency) * magnitude * speed;
        }
    }


    // flip the enemy
    void Flip()
    {
        

        Vector3 transformScale = transform.localScale;
        transformScale.y *= -1;
        transform.localScale = transformScale;
    }



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

   




}
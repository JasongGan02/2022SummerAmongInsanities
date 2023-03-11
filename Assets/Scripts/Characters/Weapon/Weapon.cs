using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    [SerializeField] public GameObject player; 

    public Playermovement playermovement;
    public PlayerInteraction playerinteraction;
    public Animator animator;

    private float speed;

    public float magnitude = 0.1f;
    public float frequency = 10f;
    public float maxSpeed = 10f; // Set the maximum speed of the object
    public float slowDownDistance = 1f; // Set the distance from the player where the object should start slowing down

    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        animator = player.GetComponent<Animator>();
        playerinteraction = player.GetComponent<PlayerInteraction>();
        
    }


    public virtual void Update()
    {
        
        Flip();


        if (Input.GetMouseButton(0))
        {
            attack();
           


        }
        else
        {
            Patrol();

        }



    }

    public virtual void attack()
    {
        playerinteraction.weaponAnim = false;

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
    public virtual void Flip()
    {
        if (playermovement.facingRight && (transform.localScale.y < 0) || !playermovement.facingRight && (transform.localScale.y > 0))
        {
            Vector3 transformScale = transform.localScale;
            transformScale.y *= -1;
            transform.localScale = transformScale;
        }
    }



    // patrol around
    public virtual void Patrol()
    {

        transform.position = player.transform.position;
        
       
    }

    


}
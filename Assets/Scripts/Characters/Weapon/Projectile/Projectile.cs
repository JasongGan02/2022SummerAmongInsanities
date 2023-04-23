using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public GameObject player;

    public Playermovement playermovement;
    

    private float speed = 18f;
    private int damage = 1;

    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        Launch();

    }

    public virtual void Update()
    {
        Flip();


    }
     




    // Called when the projectile collides with another object
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Destroy(gameObject);
        }
        
    }



    // Launches the projectile in the specified direction
    public virtual void Launch()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (playermovement.facingRight)
        {
            rb.AddForce(Vector3.right * speed, ForceMode2D.Impulse);
        }
        else
        {
            
            rb.AddForce(Vector3.left * speed, ForceMode2D.Impulse);
        }
            
            
    }

    public virtual void Flip()
    {
        if (playermovement.facingRight)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 315));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 135));
        }
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public GameObject player;

    public Playermovement playermovement;
    private Inventory inventory;

    private float speed = 18f;
    private int damage = 1;

    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        Launch();
        inventory = FindObjectOfType<Inventory>();
    }

    public virtual void Update()
    {
        Flip();


    }
     




    // Called when the projectile collides with another object
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            Debug.Log("arrow da dao le");
            VillagerController villager = collision.gameObject.GetComponent<VillagerController>();
            villager.takenDamage(2);
            Destroy(gameObject);
        }
        else
        {
            Destroy(this);
            gameObject.layer = LayerMask.NameToLayer("resource");
            var controller = gameObject.AddComponent<DroppedObjectController>();
            controller.Initialize(inventory.findSlot("Arrow").item, 1);


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


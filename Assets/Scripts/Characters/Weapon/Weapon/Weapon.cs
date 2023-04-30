using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Threading;
using static UnityEngine.EventSystems.EventTrigger;

public class Weapon : MonoBehaviour
{


    

    [SerializeField] public GameObject player; 

    public Playermovement playermovement;
    public PlayerInteraction playerinteraction;
    public Inventory inventory;
    

    protected WeaponObject weaponStats;
    protected float AtkInterval = 1f;
    protected float farm;
    protected float Attack;

    public float speed;
    public float magnitude = 0.1f;
    public float maxSpeed = 10f; // Set the maximum speed of the object
    public float slowDownDistance = 1f; // Set the distance from the player where the object should start slowing down
    public float frequency = 10f;

    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        playerinteraction = player.GetComponent<PlayerInteraction>();
        inventory = FindObjectOfType<Inventory>();
        
  
    }


    public virtual void Update()
    {
        
        Flip();


        if (Input.GetMouseButton(0))
        {
            //InvokeRepeating("attack", 0.5f, AtkInterval);
            attack();
        }
        else
        {
            Patrol();

        }



    }





    public virtual void Initialize(WeaponObject weaponObject)
    {
        this.weaponStats = weaponObject;
        Type controllerType = GetType();
        Type objectType = weaponObject.GetType();

        // Get all the fields of the controller type
        FieldInfo[] controllerFields = controllerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        // Iterate over the fields and set their values from the object
        foreach (FieldInfo controllerField in controllerFields)
        {
            // Try to find a matching field in the object
            FieldInfo objectField = objectType.GetField(controllerField.Name);
            // If there's a matching field, set the value
            if (objectField != null && objectField.FieldType == controllerField.FieldType)
            {
                controllerField.SetValue(this, objectField.GetValue(weaponObject));
            }
        }

        // Get all the properties of the controller type
        PropertyInfo[] controllerProperties = controllerType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);

        // Iterate over the properties and set their values from the object
        foreach (PropertyInfo controllerProperty in controllerProperties)
        {
            // Try to find a matching property in the object
            PropertyInfo objectProperty = objectType.GetProperty(controllerProperty.Name);

            // If there's a matching property, set the value
            if (objectProperty != null && objectProperty.PropertyType == controllerProperty.PropertyType && objectProperty.CanRead)
            {
                controllerProperty.SetValue(this, objectProperty.GetValue(weaponObject));
            }
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

            transform.position = player.transform.position + new Vector3(1f, 0, 0) + Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;
        }
        else
        {
            transform.position = player.transform.position - new Vector3(1f, 0, 0) - Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;
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


    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            Debug.Log("da dao le");
            VillagerController villager = collision.gameObject.GetComponent<VillagerController>();
            villager.takenDamage(1);
        }
           
    }


}
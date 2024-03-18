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
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Rendering;

public class Weapon : MonoBehaviour, IDamageSource
{
    
    protected CharacterController characterController;
    protected WeaponObject weaponStats;
    protected float finalDamage;

    protected GameObject player;
    protected Playermovement playermovement;
    protected PlayerInteraction playerinteraction;
    protected Inventory inventory;
    

    protected float speed;
    protected float magnitude = 0.1f;
    protected float maxSpeed = 10f; // Set the maximum speed of the object
    protected float frequency = 10f; // spining frequency

    protected bool isAttacking = false;
    protected float attackCooldown = 0.6f; // Duration for which the attack will continue
    protected float lastAttackTime;


    protected audioManager am;
    protected GameObject floatingTextPrefab;
    public float DamageAmount => finalDamage;

    public float CriticalChance => characterController.CriticalChance;

    public float CriticalMultiplier => characterController.CriticalMultiplier;

    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        playerinteraction = player.GetComponent<PlayerInteraction>();
        inventory = FindObjectOfType<Inventory>();
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
        floatingTextPrefab = GameObject.Find("FloatingDamage");
    }


    public virtual void Update()
    {
        Flip();

        // Check if the mouse button is pressed or if the attack is already in progress
        if (Input.GetMouseButtonDown(0) || (isAttacking && Time.time - lastAttackTime < attackCooldown))
        {
            if (!isAttacking)
            {
                lastAttackTime = Time.time;
                isAttacking = true;
                InvokeRepeating("PlayAttack", 0.1f, 1f); 
            }
            attack();
        }
        else
        {
            if (isAttacking && Time.time - lastAttackTime >= attackCooldown)
            {
                isAttacking = false;
                CancelInvoke("PlayAttack"); 
            }
            Patrol();
        }
    }


    public virtual void Initialize(WeaponObject weaponObject, CharacterController characterController)
    {
        this.characterController = characterController;
        this.weaponStats = weaponObject;
        finalDamage = characterController.AtkDamage * weaponObject.DamageCoef;

    }


    protected void PlayAttack()
    {
        am.playWeaponAudio(am.attack);
    }
    public virtual void attack()
    {
        if (!isAttacking) return;
        
        float speed = maxSpeed; // Set the default speed to the maximum speed

        if (playermovement.facingRight)
        {
            transform.position = player.transform.position + new Vector3(1f, 0, 0) + Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;
        }
        else
        {
            transform.position = player.transform.position - new Vector3(1f, 0, 0) - Vector3.right * Mathf.Sin(Time.time * frequency) * magnitude * speed;
        }

    } 



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
        if (collision.gameObject.tag == "enemy")
        {
            ApplyDamage(collision.gameObject.GetComponent<CharacterController>());
            Debug.Log("damaging"); 

        }
           
    }

    public void ApplyDamage(IDamageable target)
    {
        float damageDealt = target.CalculateDamage(DamageAmount, CriticalChance, CriticalMultiplier);
        target.TakeDamage(damageDealt, this);
    }

}
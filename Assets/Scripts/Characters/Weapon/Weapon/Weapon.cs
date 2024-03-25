using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;


public class Weapon : MonoBehaviour, IDamageSource
{
    
    protected CharacterController characterController;
    protected WeaponObject weaponStats;
    protected float finalDamage;

    protected GameObject player;
    protected Playermovement playermovement;

    protected Animator weaponAnmator;
    protected audioManager am;

    protected float Delay = 1f;
    protected bool attackBlocked;
    protected Inventory inventory;

    public float DamageAmount => finalDamage;

    public float CriticalChance => characterController.CriticalChance;
    public float CriticalMultiplier => characterController.CriticalMultiplier;



    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
        weaponAnmator = this.GetComponent<Animator>();
        inventory = FindObjectOfType<Inventory>();
    }


    public virtual void Update()
    {
        Flip();
        if (Input.GetMouseButtonDown(0))
        {
            attack();
        }
        else
        {
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
        if (attackBlocked)
            return;

        if(playermovement.facingRight)
        weaponAnmator.SetTrigger("Attack");
        else
        weaponAnmator.SetTrigger("AttackLeft");

        PlayAttack();
        attackBlocked= true;
        StartCoroutine(DelayAttack());
        
    }

    protected IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(Delay);
        attackBlocked = false;
    }


    public virtual void Flip()
    {
 
        if ((playermovement.facingRight && transform.localScale.x < 0) ||
            (!playermovement.facingRight && transform.localScale.x > 0))
        {
            
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Constants;
using static UnityEngine.Rendering.DebugUI;
using Animator = UnityEngine.Animator;

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
    protected bool isAttacking = false;
    public float DamageAmount => finalDamage;

    public float CriticalChance => characterController.CriticalChance;
    public float CriticalMultiplier => characterController.CriticalMultiplier;

    protected LayerMask enemyLayer;
    protected float detectionRadius = 1.5f;

    public virtual void Start()
    {
        anim();
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
        inventory = FindObjectOfType<Inventory>();
    }

    protected virtual void anim()
    {
    }


    public virtual void Update()
    {
        DetectAndAttackEnemy();
        Patrol();
        Flip();
        
    }


    public virtual void Initialize(WeaponObject weaponObject, CharacterController characterController)
    {
        this.characterController = characterController;
        this.weaponStats = weaponObject;
        finalDamage = characterController.AtkDamage * weaponObject.DamageCoef;

    }
    protected virtual void DetectAndAttackEnemy()
    {
        // Detect enemies within a certain radius around the weapon
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach(Collider2D hit in hits)
        {
            if(hit.TryGetComponent<EnemyController>(out EnemyController enemyController))
            {
                EnsureFacingEnemy(hit.gameObject.transform.position);

                // Trigger attack
                if (!attackBlocked)
                {
                    attack();
                }
            }
        }
    }

    protected void PlayAttack()
    {
        am.playWeaponAudio(am.attack);
    }
    public virtual void attack()
    {
        if (attackBlocked)
            return;

        if (playermovement.facingRight)
            Attack();
        else
            AttackLeft();

        PlayAttack();
        attackBlocked= true;
        StartCoroutine(DelayAttack());
        
    }
    protected void Attack()
    {
        weaponAnmator.SetTrigger("Attack");
        isAttacking = true;
    }

    protected void AttackLeft()
    {
        weaponAnmator.SetTrigger("AttackLeft");
        isAttacking = true;
    }

    protected IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(Delay);
        attackBlocked = false;
    }


    protected void EnsureFacingEnemy(Vector3 enemyPosition)
    {
        bool shouldFaceRight = enemyPosition.x > transform.position.x;
        if (shouldFaceRight != playermovement.facingRight)
        {
            // Flip player and weapon if they're not facing towards the enemy
            Flip();
        }
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
        if (collision.gameObject.tag == "enemy" && isAttacking)
        {
            ApplyDamage(collision.gameObject.GetComponent<CharacterController>());
            Debug.Log("damaging");

            isAttacking = false;

        }
           
    }

    public void ApplyDamage(IDamageable target)
    {
        float damageDealt = target.CalculateDamage(DamageAmount, CriticalChance, CriticalMultiplier);
        target.TakeDamage(damageDealt, this);
    }


}
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
    protected float detectionRadius;
    protected GameObject player;
    protected Playermovement playermovement;
    protected audioManager am;
    protected float Delay = 1f;
    protected bool attackBlocked;
    protected Inventory inventory;
    protected bool isAttacking = false;
    protected Vector3 offset = new Vector3(1.0f, 0, 0);
    public float DamageAmount => finalDamage;
    public float CriticalChance => characterController.CriticalChance;
    public float CriticalMultiplier => characterController.CriticalMultiplier;

    protected LayerMask enemyLayer;

    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playermovement = player.GetComponent<Playermovement>();
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
        inventory = FindObjectOfType<Inventory>();
    }

    public virtual void Initialize(WeaponObject weaponObject, CharacterController characterController)
    {
        this.characterController = characterController;
        this.weaponStats = weaponObject;
        finalDamage = characterController.AtkDamage * weaponObject.DamageCoef;
        detectionRadius = characterController.AtkRange; 

    }

    public virtual void Update()
    {
        if (!isAttacking)
        {
            FollowPlayer();
        }
        DetectAndAttackEnemy();
    }

    void FollowPlayer()
    {
        transform.position = player.transform.position + offset;
    }
    protected virtual void DetectAndAttackEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, detectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("enemy") && !isAttacking)
            {
                StartCoroutine(Attack(hit.transform));
            }
        }
    }
    protected void PlayAttack()
    {
        am.playWeaponAudio(am.attack);
    }

    IEnumerator Attack(Transform enemy)
    {
        isAttacking = true;

   
        Vector2 direction = (enemy.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

     
        PlayAttack();
        Vector3 originalPosition = transform.position;
        Vector3 attackPosition = player.transform.position + (Vector3)direction * 1.0f; // Adjust 1.0f to control how far the spear pokes
        while (Vector3.Distance(transform.position, attackPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, attackPosition, Time.deltaTime * 2);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        while (Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, Time.deltaTime * 2);
            yield return null;
        }

        isAttacking = false;
    }


    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy") && isAttacking)
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                ApplyDamage(damageable);
            }
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Constants;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;
using Animator = UnityEngine.Animator;

public class Weapon : MonoBehaviour, IDamageSource
{
    protected AudioEmitter _audioEmitter;
    protected CharacterController characterController;
    protected WeaponObject weaponStats;
    protected float finalDamage;
    protected float attackRange;
    protected float knockbackForce;


    protected GameObject player;
    protected PlayerMovement playerMovement;
    protected Transform targetEnemy;
    

    protected Inventory inventory;
    protected bool isAttacking = false;
    protected float attackDelay = 1f;
    protected Vector2 targetDirection;
    protected float speed = 15f;

    public GameObject SourceGameObject => gameObject;
    public float DamageAmount => finalDamage;
    public float CriticalChance => characterController.CriticalChance;
    public float CriticalMultiplier => characterController.CriticalMultiplier;



    public virtual void Start()
    {
        player = GameObject.Find("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        _audioEmitter = GetComponent<AudioEmitter>();
        inventory = FindObjectOfType<Inventory>();
    }

    public virtual void Initialize(WeaponObject weaponObject, CharacterController characterController)
    {
        this.characterController = characterController;
        this.weaponStats = weaponObject;
        finalDamage =  weaponObject.BaseDamage + characterController.CurrentStats.attackDamage * weaponObject.DamageCoef ;
        attackRange = (weaponObject.BaseRange + characterController.CurrentStats.attackRange * weaponObject.RangeCoef)/100;
        knockbackForce = weaponObject.KnockBack;

    }

    
    public virtual void Update()
    {

        if (!isAttacking)
        {
            transform.position = player.transform.position;
            DetectAndAttackEnemy();  
        }
    }


    protected virtual void DetectAndAttackEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out EnemyController enemy))
            {

                targetEnemy = hit.transform;
                
                StartCoroutine(PrepareAttack(hit.transform.position));
                break;
            }
        }
        if(targetEnemy == null)
        {
            transform.rotation = Quaternion.Euler(0, 0, 270);
        }
        
    }


    IEnumerator PrepareAttack(Vector2 targetPosition)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

        float rotationDuration = 0.2f; // Duration for rotation
        float rotateTime = 0.0f;

        while (rotateTime < 1.0f)
        {
            // Update target position in case it moves
            targetDirection = (targetPosition - (Vector2)player.transform.position).normalized;
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90;

            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);

            rotateTime += Time.deltaTime / rotationDuration;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, rotateTime);

            // Update weapon position to follow the player's movement
            transform.position = player.transform.position;

            yield return null; // Wait for the next frame
        }

        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        _audioEmitter.PlayClipFromCategory("WeaponAttack");
        Debug.Log(attackRange);
        float startTime = Time.time;
        float journeyLength = attackRange;
        float fracJourney = 0f;

        Vector2 startPosition = player.transform.position; 
        Vector2 endPosition = startPosition + targetDirection * attackRange; 

       
        while (fracJourney < 1.0f)
        {
            float distCovered = (Time.time - startTime) * speed * 2;
            fracJourney = distCovered / journeyLength;
            transform.position = Vector2.Lerp(startPosition, endPosition, fracJourney);
            yield return null;
        }

     
        yield return new WaitForSeconds(0.1f);

     
        startTime = Time.time; 
        fracJourney = 0f;
        while (fracJourney < 1.0f)
        {
            float distCovered = (Time.time - startTime) * speed;
            fracJourney = distCovered / journeyLength;
            transform.position = Vector2.Lerp(endPosition, startPosition, fracJourney);
            yield return null;
        }

        isAttacking = false; 
        targetEnemy = null;
    }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy") && isAttacking)
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                ApplyDamage(damageable);
                KnockbackEnemy(collision);
            }
        }
    }

    public void ApplyDamage(IDamageable target)
    {
        float damageDealt = target.CalculateDamage(DamageAmount, CriticalChance, CriticalMultiplier);
        target.TakeDamage(damageDealt, this);
    }

    protected virtual void KnockbackEnemy(Collider2D enemy)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 knockbackDirection = (enemy.transform.position - player.transform.position).normalized;
            enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

}
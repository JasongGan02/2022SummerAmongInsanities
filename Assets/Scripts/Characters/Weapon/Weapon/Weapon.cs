using System.Collections;
using UnityEngine;

public enum WeaponState
{
    Idle,
    Attacking,
    Returning
}

public class Weapon : MonoBehaviour, IDamageSource, IEffectableController
{
    protected AudioEmitter _audioEmitter;
    protected CharacterController characterController;
    protected WeaponObject weaponStats;
    protected GameObject player;
    protected PlayerMovement playerMovement;

    protected Transform targetEnemy;
    protected Inventory inventory;

    protected WeaponState currentState = WeaponState.Idle;
    protected bool isAttacking = false;

    public float attackCycleTime => 2 / characterController.AttackInterval;
    protected float windupRatio = 0.2f; 
    protected float followThroughRatio = 0.2f;
    protected float minWindupTime = 0.2f; 
    protected float maxWindupTime = 0.7f; 
    protected float minFollowThroughTime = 0.2f; 
    protected float maxFollowThroughTime = 0.7f; 

    protected float finalDamage;
    protected float attackRange;    
    protected float knockbackForce;
    protected float speed = 15f;

    protected Vector3 idleOffset = new Vector3(0, 0f, 0);
    protected float idleBobSpeed = 2f;
    protected float idleBobAmount = 0.5f;
    protected float idleBobTime;
    protected bool hasDealtDamage = false;

    protected Vector2 targetDirection;
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
        characterController.OnWeaponStatsChanged += UpdateWeaponStats;
    }
    public virtual void Initialize(WeaponObject weaponObject, CharacterController characterController)
    {
        this.characterController = characterController;
        this.weaponStats = weaponObject;
        finalDamage = weaponObject.BaseDamage + characterController.CurrentStats.attackDamage * weaponObject.DamageCoef;
        attackRange = (weaponObject.BaseRange + characterController.CurrentStats.attackRange * weaponObject.RangeCoef) / 100;
        knockbackForce = weaponObject.KnockBack;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 12;
        }
        foreach (var effect in weaponStats.onInitializeEffects)
        {
            effect.ExecuteEffect(this);
        }
    }
    private void UpdateWeaponStats()
    {
        finalDamage = weaponStats.BaseDamage + characterController.CurrentStats.attackDamage * weaponStats.DamageCoef;
        attackRange = (weaponStats.BaseRange + characterController.CurrentStats.attackRange * weaponStats.RangeCoef) / 100;
        knockbackForce = weaponStats.KnockBack;
    }

    public virtual void Update()
    {
        switch (currentState)
        {
            case WeaponState.Idle:
                FollowPlayerWithFloat();
                DetectAndAttackEnemy();
                break;

            case WeaponState.Attacking:
                break;

            case WeaponState.Returning:
                break;
        }
    }



    protected virtual void FollowPlayerWithFloat()
    {
        idleBobTime += Time.deltaTime * idleBobSpeed;
        float bobOffset = Mathf.Sin(idleBobTime) * idleBobAmount;
        Vector3 targetPosition = player.transform.position + idleOffset + new Vector3(0, bobOffset, 0);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

        // ������ҵĳ���
        if (playerMovement != null)
        {
            float angle = playerMovement.facingRight ? 270 : 90;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    protected virtual void DetectAndAttackEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange*0.8f);
        float minDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out EnemyController enemy))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = hit.transform;
                }
            }
        }

        if (closestEnemy != null && !isAttacking)
        {
            targetEnemy = closestEnemy;
            targetDirection = (targetEnemy.position - transform.position).normalized;
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        currentState = WeaponState.Attacking;
        isAttacking = true;
        hasDealtDamage = false;

        float calculatedWindupTime = attackCycleTime * windupRatio;
        float calculatedFollowThroughTime = attackCycleTime * followThroughRatio;

        float adjustedWindupTime = Mathf.Clamp(calculatedWindupTime, minWindupTime, maxWindupTime);
        float adjustedFollowThroughTime = Mathf.Clamp(calculatedFollowThroughTime, minFollowThroughTime, maxFollowThroughTime);
        float adjustedIntervalTime = attackCycleTime - (adjustedWindupTime + adjustedFollowThroughTime);

        float startTime = Time.time;
        Vector2 startPosition = player.transform.position + idleOffset;
        Vector2 targetPosition =  startPosition + targetDirection * attackRange;

        while (Time.time - startTime < adjustedWindupTime)
        {
            float remainingTime = adjustedWindupTime - (Time.time - startTime);
            float distance = Vector2.Distance(transform.position, targetPosition);
            float dynamicSpeed = distance / remainingTime;

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, dynamicSpeed * Time.deltaTime);

            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        yield return StartCoroutine(ReturnToPlayer(adjustedFollowThroughTime, adjustedIntervalTime));
    }

    IEnumerator ReturnToPlayer(float followThroughTime, float intervalTime)
    {
        currentState = WeaponState.Returning;
        float startTime = Time.time;
        Vector2 currentTargetPosition = player.transform.position + idleOffset;

        while (Time.time - startTime < followThroughTime)
        {
            currentTargetPosition = player.transform.position + idleOffset;

            float remainingTime = followThroughTime - (Time.time - startTime);
            float distance = Vector2.Distance(transform.position, currentTargetPosition);
            float dynamicSpeed = distance / remainingTime;

            transform.position = Vector2.MoveTowards(transform.position, currentTargetPosition, dynamicSpeed * Time.deltaTime);

            Vector2 direction = ((Vector2)transform.position - currentTargetPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        float intervalStartTime = Time.time;

        while (Time.time - intervalStartTime < intervalTime)
        {
            FollowPlayerWithFloat();
            yield return null; 
        }

        isAttacking = false;
        currentState = WeaponState.Idle;
    }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy") && isAttacking)
        {

            if (hasDealtDamage) return;

            IDamageable damageable = collision.GetComponent<IDamageable>();
            
            if (damageable != null)
            {
                ApplyDamage(damageable);
                ApplyEffects(collision.GetComponent<IEffectableController>());
                KnockbackEnemy(collision);
                hasDealtDamage = true;
            }

        }
    }

    public void ApplyDamage(IDamageable target)
    {
        float damageDealt = target.CalculateDamage(DamageAmount, CriticalChance, CriticalMultiplier);
        target.TakeDamage(damageDealt, this);
    }
    
    private void ApplyEffects(IEffectableController target)
    {
        var detonateChargeEffect = GetComponent<DetonateChargeEffectController>();
        if (detonateChargeEffect != null && detonateChargeEffect.enabled)
        {
            detonateChargeEffect.ApplyDetonateHit(target);
        }
        
        foreach (var effect in weaponStats.onHitEffects)
        {
            effect.ExecuteEffect(target);
        }
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

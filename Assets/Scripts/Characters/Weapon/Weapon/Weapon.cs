using System.Collections;
using UnityEngine;

public enum WeaponState
{
    Idle,
    Attacking,
    Returning
}

public class Weapon : MonoBehaviour, IDamageSource
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
    protected float attackCycleTime = 4f;
    protected float windupRatio = 0.2f; 
    protected float followThroughRatio = 0.2f;
    protected float intervalRatio => 1f - windupRatio - followThroughRatio; 

    protected float finalDamage;
    protected float attackRange;
    protected float knockbackForce;
    protected float speed = 15f;

    protected Vector3 idleOffset = new Vector3(0, 0f, 0);
    protected float idleBobSpeed = 2f;
    protected float idleBobAmount = 0.5f;
    protected float idleBobTime;

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
                // 在协程中执行攻击逻辑，无需在Update重复
                break;

            case WeaponState.Returning:
                // 在协程中执行回程逻辑
                break;
        }
    }



    protected virtual void FollowPlayerWithFloat()
    {
        idleBobTime += Time.deltaTime * idleBobSpeed;
        float bobOffset = Mathf.Sin(idleBobTime) * idleBobAmount;
        Vector3 targetPosition = player.transform.position + idleOffset + new Vector3(0, bobOffset, 0);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

        // 朝向玩家的朝向
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

        
        float attackWindupTime = attackCycleTime * windupRatio;

        _audioEmitter.PlayClipFromCategory("WeaponAttack");

        float startTime = Time.time;
        Vector2 startPosition = player.transform.position + idleOffset;
        Vector2 targetPosition = startPosition + targetDirection * attackRange;

        
        while (Time.time - startTime < attackWindupTime)
        {
            float remainingTime = attackWindupTime - (Time.time - startTime);
            float distance = Vector2.Distance(transform.position, targetPosition);
            float dynamicSpeed = distance / remainingTime;

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, dynamicSpeed * Time.deltaTime);

            
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        
        yield return StartCoroutine(ReturnToPlayer());
    }



    IEnumerator ReturnToPlayer()
    {
        currentState = WeaponState.Returning;

        
        float returnTime = attackCycleTime * followThroughRatio;
        float startTime = Time.time;

        Vector2 currentTargetPosition = player.transform.position + idleOffset;

        while (Time.time - startTime < returnTime)
        {
            currentTargetPosition = player.transform.position + idleOffset;

            float remainingTime = returnTime - (Time.time - startTime);
            float distance = Vector2.Distance(transform.position, currentTargetPosition);
            float dynamicSpeed = distance / remainingTime;

            transform.position = Vector2.MoveTowards(transform.position, currentTargetPosition, dynamicSpeed * Time.deltaTime);

            Vector2 direction = ((Vector2)transform.position - currentTargetPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }


        
        float intervalTime = attackCycleTime * intervalRatio;
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
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                ApplyDamage(damageable);
                ApplyEffects(collision.GetComponent<IEffectableController>());
                KnockbackEnemy(collision);
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
        Debug.Log("applied effect");
        foreach (var effect in weaponStats.onHitEffects)
        {
            Debug.Log(effect);
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

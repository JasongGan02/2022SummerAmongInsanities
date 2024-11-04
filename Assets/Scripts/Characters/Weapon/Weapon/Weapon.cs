using System.Collections;
using UnityEngine;

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

    protected Vector3 idleOffset = new Vector3(0, 1.5f, 0); 
    protected float idleBobSpeed = 2f; 
    protected float idleBobAmount = 0.5f; 
    private float idleBobTime;

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
        finalDamage = weaponObject.BaseDamage + characterController.CurrentStats.attackDamage * weaponObject.DamageCoef;
        attackRange = (weaponObject.BaseRange + characterController.CurrentStats.attackRange * weaponObject.RangeCoef) / 100;
        knockbackForce = weaponObject.KnockBack;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 12;
        }
    }

    public virtual void Update()
    {
        if (!isAttacking)
        {
            FollowPlayerWithFloat();
            DetectAndAttackEnemy();
        }
    }

    protected void FollowPlayerWithFloat()
    {
        // 计算idle状态下的上下摆动
        idleBobTime += Time.deltaTime * idleBobSpeed;
        float bobOffset = Mathf.Sin(idleBobTime) * idleBobAmount;

        // 计算武器应该跟随的位置
        Vector3 targetPosition = player.transform.position + idleOffset + new Vector3(0, bobOffset, 0);

        // 平滑移动到该位置
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
    }

    protected virtual void DetectAndAttackEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyController>(out EnemyController enemy))
            {
                targetEnemy = hit.transform;
                StartCoroutine(PrepareAttack(hit.transform.position));
                break;
            }
        }

        if (targetEnemy == null)
        {
            if (player.GetComponent<PlayerMovement>().facingRight)
            {
                // 玩家朝右，武器也朝右
                transform.rotation = Quaternion.Euler(0, 0, 270); // 正常朝向
            }
            else
            {
                // 玩家朝左，武器也朝左
                transform.rotation = Quaternion.Euler(0, 0, 90); // 水平翻转
            }
            
        }
    }

    IEnumerator PrepareAttack(Vector2 targetPosition)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

        float rotationDuration = 0.2f; // 旋转时间
        float rotateTime = 0.0f;

        while (rotateTime < 1.0f)
        {
            // 更新目标方向
            targetDirection = (targetPosition - (Vector2)transform.position).normalized;
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90;

            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);

            rotateTime += Time.deltaTime / rotationDuration;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, rotateTime);

            yield return null; // 等待下一帧
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

        Vector2 startPosition = player.transform.position + idleOffset;
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

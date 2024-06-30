using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System;
public class Projectile : MonoBehaviour, IDamageSource
{
    protected ProjectileObject projectileObject;
    protected CharacterObject characterObject;
    protected CharacterController firingCharacter;
    protected float finalDamage;
    protected float lifespanInSeconds = 8.0f; // Maximum lifespan of the projectile in seconds
    protected float timeOfLaunch;
    protected Rigidbody2D rb;
    protected List<TextAsset> hatredList;
    public float firingAngle = 45.0f; // Angle of firing
    public float speed = 20f; // Speed of the projectile
    public float gravityScale = 0.1f; // Scaled down gravity effect

    public GameObject SourceGameObject => gameObject;
    public float DamageAmount => finalDamage;

    public float CriticalChance => firingCharacter.CriticalChance;

    public float CriticalMultiplier => firingCharacter.CriticalMultiplier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    protected virtual void Update()
    {
        HasReachedLifespan();
        // Constantly align the projectile to its velocity vector
        AlignToVelocity();
    }

    public void Initialize(CharacterController firingCharacter, ProjectileObject projectileObject)
    {
        this.projectileObject = projectileObject;
        this.firingCharacter = firingCharacter;
        hatredList = firingCharacter.CharacterObject.hatred;
        finalDamage = firingCharacter.CurrentStats.attackDamage * projectileObject.DamageCoef;
        timeOfLaunch = Time.time;
    }


    public virtual void Launch(GameObject target, Transform startPosition)
    {
        if (target == null)
        {
            Debug.LogError("Launch target is null.");
            return;
        }

        // Calculate the direction to the target
        Vector2 directionToTarget = (target.transform.position - startPosition.position).normalized;

        // Set the projectile's velocity towards the target
        rb.velocity = directionToTarget * speed;

        // Adjust the gravity scale for minor gravity effect
        rb.gravityScale = gravityScale;

    }


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsInHatredList(collider))
        {
            CharacterController character = collider.GetComponent<CharacterController>();
            if (character != null)
            {
                ApplyDamage(character);
            }

            // Return the projectile to the pool
            PoolManager.Instance.Return(gameObject, projectileObject);
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            PoolManager.Instance.Return(gameObject, projectileObject);
        }
    }
    protected virtual void HasReachedLifespan()
    {
        if (Time.time - timeOfLaunch > lifespanInSeconds)
        {
            PoolManager.Instance.Return(gameObject, projectileObject);
        }
            
    }

    protected void AlignToVelocity()
    {
        if (rb.velocity != Vector2.zero)
        {
            // Calculate the angle from velocity
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;

            // Align the arrow to 45 degrees from its velocity vector
            transform.rotation = Quaternion.AngleAxis(angle - 45, Vector3.forward);
        }
    }
    
    public virtual void ApplyDamage(IDamageable target)
    {
        float damageDealt = target.CalculateDamage(DamageAmount, CriticalChance, CriticalMultiplier);
        target.TakeDamage(damageDealt, this);
        Debug.Log("damaging!");
    }

    protected virtual bool IsInHatredList(Collider2D collider)
    {
        if (hatredList == null)
            return false;
        foreach (var hatedType in hatredList)   
        {
            Type type = Type.GetType(hatedType.name);
            if (type == null) continue;

            var target = collider.GetComponent(type) as CharacterController;
            if (target != null)
            {
                return true; // Target is in hatred list
            }
        }

        return false; // Target not found in hatred list
    }

}
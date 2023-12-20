using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System;
public class Projectile : MonoBehaviour
{
    protected WeaponObject weaponObject;
    protected CharacterObject characterObject;
    protected CharacterController firingCharacter;
    protected int finalDamage;
    protected float lifespanInSeconds = 8.0f; // Maximum lifespan of the projectile in seconds
    protected float timeOfLaunch;
    protected Rigidbody2D rb;
    protected List<TextAsset> hatredList;
    public float firingAngle = 45.0f; // Angle of firing
    public float speed = 20f; // Speed of the projectile
    public float gravityScale = 0.1f; // Scaled down gravity effect


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    protected void Update()
    {
        if (HasReachedLifespan())
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject, weaponObject.getPrefab());
        }

        // Constantly align the projectile to its velocity vector
        AlignToVelocity();
    }

    public void Initialize(CharacterController firingCharacter, WeaponObject weaponObject)
    {
        this.weaponObject = weaponObject;
        this.firingCharacter = firingCharacter;
        hatredList = firingCharacter.GetCharacterObject().Hatred;
        finalDamage = Mathf.RoundToInt(firingCharacter.AtkDamage * weaponObject.getAttack());
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
        // Check if the collided object is in the hatred list
        foreach (var hatedType in hatredList)
        {
            Type type = Type.GetType(hatedType.name);
            if (type == null) continue;

            var target = collider.GetComponent(type) as CharacterController;
            if (target != null)
            {
            
                target.takenDamage(finalDamage);
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject, weaponObject.getPrefab());

                return; // Exit the method after dealing damage
            }
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject, weaponObject.getPrefab());
        }
    }
    private bool HasReachedLifespan()
    {
        return Time.time - timeOfLaunch > lifespanInSeconds;
    }

    private void AlignToVelocity()
    {
        if (rb.velocity != Vector2.zero)
        {
            // Calculate the angle from velocity
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;

            // Align the arrow to 45 degrees from its velocity vector
            transform.rotation = Quaternion.AngleAxis(angle - 45, Vector3.forward);
        }
    }
}
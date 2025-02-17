using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeaponController : Weapon, IRangedAttacker
{
    protected ProjectileObject projectileObject;
    public float AttackRange => characterController.CurrentStats.attackRange;

    public ProjectileObject ProjectileObject => projectileObject;

    protected GameObject target;
    protected Transform startPosition;

    public override void Start()
    {
        base.Start();
        projectileObject = ((RangedWeaponObject)weaponStats).projectileObject;
    }
    protected void Flip()
    {
        if (targetEnemy != null)
        {
           
            Vector2 directionToEnemy = (targetEnemy.position - player.transform.position).normalized;

          
            float angle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg;

         
            float offset = -45f;
            transform.rotation = Quaternion.Euler(0, 0, angle + offset);

           
            float radius = 1f; 
            Vector2 bowPosition = (Vector2)player.transform.position + directionToEnemy * radius;
            transform.position = bowPosition;
        }
        else
        {
            float defaultAngle = playerMovement.facingRight ? -45f : 135f;
            transform.rotation = Quaternion.Euler(0, 0, defaultAngle);

            transform.position = player.transform.position + new Vector3(playerMovement.facingRight ? 1f : -1f, 0, 0);
        }

        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public override void FixedUpdate()
    {
        Flip();
        startPosition = transform;

        if (!isAttacking && inventory.FindItemCount(ProjectileObject) >= 1)
        {
            DetectAndAttackEnemy();
        }

    }

    




    public virtual void FireProjectiles(GameObject target)
    {
        if (ProjectileObject != null)
        {
            GameObject projectileObject = PoolManager.Instance.Get(ProjectileObject);
            projectileObject.transform.position = startPosition.position;
            projectileObject.transform.SetParent(transform, true);
            ProjectileController projectileControllerComponent = projectileObject.GetComponent<ProjectileController>();
            projectileControllerComponent.Initialize(characterController, ProjectileObject);
            projectileControllerComponent.Launch(target, startPosition);
        }
    }
}

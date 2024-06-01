using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeaponController : Weapon, IRangedAttacker
{
    protected ProjectileObject projectileObject;
    public float AttackRange => characterController.AtkRange;

    public ProjectileObject ProjectileObject => projectileObject;

    protected GameObject target;
    protected Transform startPosition;

    public override void Start()
    {
        base.Start();
        projectileObject = weaponStats.projectileObject;
    }

    public virtual void FireProjectile(GameObject target)
    {
        if (ProjectileObject != null)
        {
            GameObject projectileObject = PoolManager.Instance.Get(ProjectileObject);
            projectileObject.transform.position = startPosition.position;
            projectileObject.transform.SetParent(transform, true);
            Projectile projectileComponent = projectileObject.GetComponent<Projectile>();
            projectileComponent.Initialize(characterController, ProjectileObject);
            projectileComponent.Launch(target, startPosition);
        }
    }
}

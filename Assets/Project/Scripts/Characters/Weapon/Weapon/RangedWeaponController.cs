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

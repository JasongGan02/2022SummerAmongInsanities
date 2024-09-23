using UnityEngine;

public abstract class RangedTowerController : AttackTowerController, IRangedAttacker
{
    public float AttackRange => currentStats.attackRange;
    public ProjectileObject ProjectileObject => ((TowerObject)characterObject).projectileObject;

    protected GameObject target;
    protected Transform startPosition;

    protected virtual void Start()
    {
        // Common start logic for all ranged towers
        isEnemySpotted = false;
        startPosition = this.transform;
        InvokeRepeating("Attack", 0f, currentStats.attackInterval);
    }

    protected abstract void Attack();

    public virtual void FireProjectile(GameObject target)
    {
        if (ProjectileObject != null)
        {
            GameObject projectileGameObject = PoolManager.Instance.Get(ProjectileObject);
            projectileGameObject.transform.position = startPosition.position;
            projectileGameObject.transform.SetParent(transform, true);
            Projectile projectileComponent = projectileGameObject.GetComponent<Projectile>();
            projectileComponent.Initialize(this, ProjectileObject);
            projectileComponent.Launch(target, startPosition);
        }
    }

 
}

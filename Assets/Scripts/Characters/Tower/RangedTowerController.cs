using UnityEngine;

public abstract class RangedTowerController : AttackTowerController, IRangedAttacker
{
    [SerializeField] protected WeaponObject weaponObject;

    public float AttackRange => _atkRange;
    public WeaponObject WeaponObject => weaponObject;

    protected GameObject target;
    protected Transform startPosition;

    protected virtual void Start()
    {
        // Common start logic for all ranged towers
        enemyContainer = FindObjectOfType<EnemyContainer>();
        isEnemySpotted = false;
        startPosition = this.transform;
        InvokeRepeating("Attack", 0f, _atkSpeed);
    }

    protected abstract void Attack();

    public virtual void FireProjectile(GameObject target)
    {
        if (weaponObject != null)
        {
            GameObject projectileObject = ProjectilePoolManager.Instance.GetProjectile(weaponObject.getPrefab());
            projectileObject.transform.position = startPosition.position;
            projectileObject.transform.SetParent(transform, true);
            Projectile projectileComponent = projectileObject.GetComponent<Projectile>();
            projectileComponent.Initialize(this, WeaponObject);
            projectileComponent.Launch(target, startPosition);
        }
    }

 
}

using UnityEngine;

public abstract class RangedTowerController : AttackTowerController, IRangedAttacker
{
    protected ProjectileObject projectileObject;

    public float AttackRange => _atkRange;
    public ProjectileObject ProjectileObject => projectileObject;

    protected GameObject target;
    protected Transform startPosition;

    protected virtual void Start()
    {
        // Common start logic for all ranged towers
        enemyContainer = FindObjectOfType<EnemyContainer>();
        isEnemySpotted = false;
        startPosition = this.transform;
        InvokeRepeating("Attack", 0f, _atkSpeed);
        am = GameObject.FindGameObjectWithTag("audio").GetComponent<audioManager>();
    }

    protected abstract void Attack();

    public virtual void FireProjectile(GameObject target)
    {
        if (projectileObject != null)
        {
            GameObject projectileGameObject = PoolManager.Instance.Get(projectileObject);
            projectileGameObject.transform.position = startPosition.position;
            projectileGameObject.transform.SetParent(transform, true);
            Projectile projectileComponent = projectileGameObject.GetComponent<Projectile>();
            projectileComponent.Initialize(this, ProjectileObject);
            projectileComponent.Launch(target, startPosition);
        }
    }

 
}

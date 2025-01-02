using System.Collections;
using UnityEngine;

public abstract class RangedTowerController : AttackTowerController, IRangedAttacker
{
    public float AttackRange => currentStats.attackRange;
    public ProjectileObject ProjectileObject => ((RangedTowerObject)characterObject).projectileObject;
    
    private RangedTowerStats rangedTowerStats => ((RangedTowerObject)characterObject).rangedTowerStats;

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

    public virtual void FireProjectiles(GameObject target)
    {
        if (ProjectileObject != null)
        {
            StartCoroutine(FireProjectilesWithDelay(target));
        }
    }

    private IEnumerator FireProjectilesWithDelay(GameObject target)
    {
        int projectilesPerShot = rangedTowerStats.projectilesPerShot;
        float delayBetweenProjectiles = 0.1f; // Adjust the delay duration as needed
        Debug.Log(projectilesPerShot);
        for (int i = 0; i < projectilesPerShot; i++)
        {
            GameObject projectileGameObject = PoolManager.Instance.Get(ProjectileObject);
            projectileGameObject.transform.position = startPosition.position;
            projectileGameObject.transform.SetParent(transform, true);
            ProjectileController projectileControllerComponent = projectileGameObject.GetComponent<ProjectileController>();
            projectileControllerComponent.Initialize(this, ProjectileObject);
            projectileControllerComponent.Launch(target, startPosition);
            
            if (projectilesPerShot > 0)
                yield return new WaitForSeconds(delayBetweenProjectiles); // Wait before firing the next projectile
        }
    }


 
}

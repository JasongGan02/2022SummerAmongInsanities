using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    [System.Serializable]
    public class ProjectilePool
    {
        public ProjectileController ProjectileControllerPrefab;
        public int initialPoolSize = 20;
    }

    [SerializeField] private List<ProjectilePool> ProjectilePools;
    private Dictionary<ProjectileController, Queue<ProjectileController>> pools = new Dictionary<ProjectileController, Queue<ProjectileController>>();

    void Start()
    {
        foreach (var pool in ProjectilePools)
        {
            CreatePool(pool.ProjectileControllerPrefab, pool.initialPoolSize);
        }
    }

    private void CreatePool(ProjectileController prefab, int initialPoolSize)
    {
        Queue<ProjectileController> newPool = new Queue<ProjectileController>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            ProjectileController newProjectileController = Instantiate(prefab);
            newProjectileController.gameObject.SetActive(false);
            newPool.Enqueue(newProjectileController);
        }
        pools[prefab] = newPool;
    }

    public ProjectileController GetProjectileController(ProjectileController prefab)
    {
        if (pools.ContainsKey(prefab) && pools[prefab].Count > 0)
        {
            ProjectileController ProjectileController = pools[prefab].Dequeue();
            ProjectileController.gameObject.SetActive(true);
            return ProjectileController;
        }
        else
        {
            return Instantiate(prefab); // Optionally expand the pool
        }
    }

    public void ReturnProjectileController(ProjectileController ProjectileController)
    {
        ProjectileController.gameObject.SetActive(false);
        pools[ProjectileController.GetPrefab()].Enqueue(ProjectileController);
    }
}

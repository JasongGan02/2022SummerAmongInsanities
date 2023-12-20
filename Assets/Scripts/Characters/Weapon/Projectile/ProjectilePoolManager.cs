using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Optionally: Initialize pools here instead of Start if needed
    }


    [System.Serializable]
    public class ProjectilePool
    {
        public GameObject prefab;
        public int initialPoolSize;
    }

    [SerializeField] private List<ProjectilePool> projectilePools;
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Start()
    {
        foreach (var pool in projectilePools)
        {
            CreatePool(pool.prefab, pool.initialPoolSize);
        }
    }

    private void CreatePool(GameObject prefab, int size)
    {
        Queue<GameObject> newPool = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.SetParent(transform, false);
            obj.SetActive(false);
            newPool.Enqueue(obj);
        }
        pools[prefab] = newPool;
    }

    public GameObject GetProjectile(GameObject prefab)
    {
        if (pools.ContainsKey(prefab) && pools[prefab].Count > 0)
        {
            GameObject projectile = pools[prefab].Dequeue();
            projectile.SetActive(true);
            return projectile;
        }
        else
        {
            return Instantiate(prefab); // Optionally expand the pool here
        }
    }

    public void ReturnProjectile(GameObject projectile, GameObject prefab)
    {
        projectile.SetActive(false);
        pools[prefab].Enqueue(projectile);
    }
}

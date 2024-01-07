using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optionally: Initialize pools here instead of Start if needed
    }



    [System.Serializable]
    public class PoolableObjectPool
    {
        [Tooltip("Must use an IPoolableObject here")]
        public BaseObject poolableObject;
        public int initialPoolSize;
    }


    [SerializeField] private List<PoolableObjectPool> IPoolableObjectPools;
    private Dictionary<BaseObject, Queue<GameObject>> pools = new Dictionary<BaseObject, Queue<GameObject>>();

    private void Start()
    {
        foreach (var pool in IPoolableObjectPools)
        {
            CreatePool(pool.poolableObject, pool.initialPoolSize);
        }
    }

    private void CreatePool(BaseObject poolableObject, int size)
    {
        Queue<GameObject> newPool = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = ((IPoolableObject)poolableObject).GetPoolGameObject();
            obj.SetActive(false);
            obj.transform.SetParent(transform, true);
            newPool.Enqueue(obj);
        }
        pools[poolableObject] = newPool;
    }

    public GameObject Get(BaseObject poolableObject)
    {
        if (pools.ContainsKey(poolableObject) && pools[poolableObject].Count > 0)
        {
            GameObject obj = pools[poolableObject].Dequeue();
            obj.SetActive(true);
            var controller = obj.GetComponent<IPoolableObjectController>();
            if (controller != null)
                controller.Reinitialize();
            return obj;
        }
        else
        {
            return ((IPoolableObject)poolableObject).GetPoolGameObject(); // Optionally expand the pool here
        }
    }

    public void Return(GameObject poolableGameObject, BaseObject poolableObject)
    {
        poolableGameObject.transform.SetParent(transform, true);
        poolableGameObject.SetActive(false);
        pools[poolableObject].Enqueue(poolableGameObject);
    }
}

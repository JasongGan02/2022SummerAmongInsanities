using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool
{
    private Queue<GameObject> pool;
    private GameObject prefab;

    public ObjectPool(GameObject prefab, int initialSize)
    {
        this.prefab = prefab;
        pool = new Queue<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject instance = GameObject.Instantiate(prefab);
            pool.Enqueue(instance);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else
        {
            return GameObject.Instantiate(prefab); // Optionally expand the pool here
        }
    }

    public void Return(GameObject instance)
    {
        pool.Enqueue(instance);
    }
}

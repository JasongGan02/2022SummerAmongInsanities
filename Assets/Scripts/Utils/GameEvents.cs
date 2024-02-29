using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            // If a duplicate exists, destroy it
            Destroy(gameObject);
        }
        else
        {
            current = this;
            DontDestroyOnLoad(gameObject); // Make this object persistent
        }
    }

    public event Action<int> OnEnemyLevelIncreased;

    public void IncreaseEnemyLevel(int enemyLevel)
    {
        OnEnemyLevelIncreased?.Invoke(enemyLevel);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;


public class EnemyObject : CharacterObject, ISpawnable
{
    [SerializeField] private EnemyStats enemyStats;
   
    protected override void OnEnable()
    {
        baseStats = enemyStats;  // Ensure the baseStats is set
        base.OnEnable();
    }
    
    public void LevelUp()
    {
        maxStats.hp *= 1.1f;
        maxStats.attackDamage *= 1.1f;
    }

    [Header("ISpawnable Fields")] 
    [SerializeField] float minLightLevel;
    [SerializeField] float maxLightLevel;
    [SerializeField] float spawnWeight;
    [SerializeField] int groupSize;
    [SerializeField] string biome;
    [SerializeField] Vector2Int colliderSize;
    
    public float MinLightLevel => minLightLevel;
    public float MaxLightLevel => maxLightLevel;
    public float SpawnWeight => spawnWeight;
    public int PackSize => groupSize;
    public string Biome => biome;
    public Vector2Int ColliderSize => colliderSize;
}

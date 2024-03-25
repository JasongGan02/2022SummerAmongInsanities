using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class EnemyObject : CharacterObject, ISpawnable
{
    public float SensingRange;
    //public int 
    public void LevelUp()
    {
        _HP *= 1.1f;
        _atkDamage *= 1.1f;
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

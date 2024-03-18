using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<EnemyObject> enemyObjects = new List<EnemyObject>(); // Assign your enemy prefab in the inspector
    public float spawnInterval = 5f; // Time in seconds between spawn checks
    public int maxEnemies = 50; // Max number of enemies allowed at once
    private float nextSpawnTime;
    private float totalWeight = 0f;
    private void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        
        foreach (var enemy in enemyObjects)
        {
            if (enemy != null)
            {
                totalWeight += (enemy as ISpawnable).SpawnWeight;
            }
        }
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime && transform.childCount < maxEnemies)
        {
            Debug.Log("Tried Spwaned");
            TrySpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void TrySpawnEnemy()
    {
        
        var chunksToSpawn = new List<int>(WorldGenerator.ActiveChunks.Keys);
        int randomChunkIndex = Random.Range(0, chunksToSpawn.Count);
        Vector2Int chunkCoord = new Vector2Int(chunksToSpawn[randomChunkIndex], 0);
        TileObject[,,] tiles = WorldGenerator.WorldData[chunkCoord];
        float[,] lightData = WorldGenerator.WorldLightData[chunkCoord];

        ISpawnable enemyObject = SelectEnemyObjectBasedOnWeight();
        // Example of a simple spawn logic based on light and terrain
        foreach (var chunk in chunksToSpawn)
        {
            for (int x = 0; x < WorldGenerator.ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldGenerator.ChunkSize.y; y++)
                {
                    
                    if (ShouldSpawnHere(tiles, lightData, x, y, enemyObject.ColliderSize, enemyObject.MinLightLevel, enemyObject.MaxLightLevel))
                    {
                        GameObject spawnedGameObject = PoolManager.Instance.Get(enemyObject as EnemyObject);
                        Debug.Log("Success!");
                        spawnedGameObject.transform.position = new Vector3(x + chunkCoord.x * WorldGenerator.ChunkSize.x, y+1, 0);
                        spawnedGameObject.transform.SetParent(transform, true);
                        return; // Spawn one enemy and exit
                    }
                }
            }
        }
    }

    private bool ShouldSpawnHere(TileObject[,,] tiles, float[,] lightData, int x, int y, Vector2Int colliderSize, float minLightLevel, float maxLightLevel)
    {
        // Check if the bottom-left tile is suitable based on terrain and light
        bool isSuitableGroundAndLight = IsGroundAndLightSuitable(tiles, lightData, x, y, minLightLevel, maxLightLevel);
        if (!isSuitableGroundAndLight)
        {
            return false; // Early exit if bottom-left tile isn't suitable
        }
    
        int startX = x - (colliderSize.x / 2);
        int endX = startX + colliderSize.x - 1;
        int startY = y + 1;
        int endY = startY + colliderSize.y - 1; // if y = 0, start y is 1, collider size is 2, then check endy to 3

        for (int checkX = startX; checkX <= endX; checkX++)
        {
            for (int checkY = startY; checkY <= endY; checkY++)
            {
                // Ensure the check is within bounds (assuming y+2 is within bounds since y is the base)
                if (checkX < 0 || checkX >= WorldGenerator.ChunkSize.x || checkY < 0 || checkY >= WorldGenerator.ChunkSize.y)
                {
                    return false; // Out of bounds, cannot spawn here
                }
                
                if (tiles[checkX, checkY, 1] != null)
                {
                    return false; // Entity present at the base level
                }
            }
        }

        return true; // Suitable location found
    }

    // Helper method to check if a specific tile is suitable based on ground and light conditions
    private bool IsGroundAndLightSuitable(TileObject[,,] tiles, float[,] lightData, int x, int y, float minLightLevel, float maxLightLevel)
    {
        bool isGround = tiles[x, y, 1] != null; // Assuming layer 1 is the entity layer, such as ground
        float lightLevel = lightData[x, y];
        bool isLightSuitable = lightLevel >= minLightLevel && lightLevel <= maxLightLevel;
    
        return isGround && isLightSuitable;
    }

    
    private ISpawnable SelectEnemyObjectBasedOnWeight()
    {
        float randomWeight = Random.Range(0, totalWeight);
        float currentWeight = 0f;
        
        foreach (var enemy in enemyObjects)
        {
            if (enemy != null)
            {
                currentWeight += (enemy as ISpawnable).SpawnWeight;
                if (currentWeight >= randomWeight)
                {
                    return (enemy as ISpawnable);
                }
            }
        }
        
        return null; 
    }
}

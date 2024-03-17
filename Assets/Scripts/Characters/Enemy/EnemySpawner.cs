using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Assign your enemy prefab in the inspector
    public float spawnInterval = 5f; // Time in seconds between spawn checks
    public int maxEnemies = 50; // Max number of enemies allowed at once
    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime && FindObjectsOfType(typeof(Enemy)).Length < maxEnemies)
        {
            TrySpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void TrySpawnEnemy()
    {
        
        var chunksToSpawn = new List<int>(WorldGenerator.ActiveChunks.Keys);
        // Example of a simple spawn logic based on light and terrain
        foreach (var chunk in chunksToSpawn)
        {
            Vector2Int chunkCoord = new Vector2Int(chunk, 0);
            TileObject[,,] tiles = WorldGenerator.WorldData[chunkCoord];
            float[,] lightData = WorldGenerator.WorldLightData[chunkCoord];

            for (int x = 0; x < WorldGenerator.ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldGenerator.ChunkSize.y; y++)
                {
                    if (ShouldSpawnHere(tiles, lightData, x, y))
                    {
                        Instantiate(enemyPrefab, new Vector3(x + chunkCoord.x * WorldGenerator.ChunkSize.x, y, 0), Quaternion.identity);
                        return; // Spawn one enemy and exit
                    }
                }
            }
        }
    }

    private bool ShouldSpawnHere(TileObject[,,] tiles, float[,] lightData, int x, int y)
    {
        // Define your own conditions based on light and terrain
        // For example, don't spawn on empty tiles or fully lit tiles
        bool isGround = tiles[x, y, 1] != null; // Assuming layer 1 is the entity layer, such as ground
        bool isDarkEnough = lightData[x, y] < 0.5f; // Example threshold, adjust based on your game

        return isGround && isDarkEnough;
    }
}

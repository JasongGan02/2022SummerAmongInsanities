using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] private List<MobCategory> mobCategories;
    public List<EnemyObject> enemyObjects = new List<EnemyObject>(); // Assign your enemy prefab in the inspector
    [SerializeField] private float spawnRate;
    //public static Dictionary<int, GameObject> Global
    private float totalWeight = 0f;
    private GameObject EnemyContainer;
    private List<int> currentTickActiveChunk = new List<int>();
    private List<GameObject> enemyList = new List<GameObject>(); //TODO: get a dic for all lists;
    private void Awake()
    {
        EnemyContainer = transform.Find("EnemyContainer").gameObject;
    }

    private void Start()
    {
        spawnRate = Time.fixedDeltaTime;
        InitializeTotalWeights();
        StartCoroutine(SpawnCycleCoroutine());
    }

    private void InitializeTotalWeights()
    {
        foreach (MobCategory mc in mobCategories)
        {
            foreach (var o in mc.mobScriptableObjects)
            {
                var so = (ISpawnable)o;
                mc.totalWeight += so.SpawnWeight;
            }
        }
    }

    private IEnumerator SpawnCycleCoroutine()
    {
        while (true)
        {
            currentTickActiveChunk =  new List<int>(WorldGenerator.ActiveChunks.Keys);
            CollectMobCapData();
            RunSpawningAttempts();
            DespawnEntities();
            
            yield return new WaitForSeconds(spawnRate);
        }
    }

    private void CalculateGlobalCap() //for multiplayer dont need to care now global = mobcap-value * totalSpawnableCap / a pre-defined default spawnable chunks number
    {
        foreach (MobCategory mc in mobCategories)
        {
            
        }
    }
    private void CollectMobCapData()
    {
        // Calculate current mob counts per category and update mobsInCategory dictionary
        foreach (var category in mobCategories)
        {
            if (category.categoryName == "enemy")
                category.currentMob = enemyList.Count;
        }
        
    }

    private void RunSpawningAttempts()
    {
        // Iterate through each mob category
 
        foreach (var category in mobCategories)
        {
            // Check if the global mob cap allows spawning in this category
            if (CanSpawnCategory(category))
            {
                AttemptSpawnMobs(category);
            }
        }
        
    }

    private bool CanSpawnCategory(MobCategory category)
    {
        // Implement logic to check against global and local mob caps
        return category.currentMob < category.baseMobCap; // Placeholder change baseMob to global mob for multiple players
    }

    private void AttemptSpawnMobs(MobCategory category)
    {
        int randomChunkIndex = Random.Range(0, currentTickActiveChunk.Count);
        Vector2Int chunkCoord = new Vector2Int(currentTickActiveChunk[randomChunkIndex], 0);
        TileObject[,,] terrainData = WorldGenerator.WorldData[chunkCoord];
        float[,] lightData = WorldGenerator.WorldLightData[chunkCoord];
        int chosenX = Random.Range(0, WorldGenerator.ChunkSize.x);
        int maxY = FindHighestBlock(terrainData, chosenX); if (maxY == int.MinValue) return;
        
        int chosenY = Random.Range(0, maxY);
        
        //if (terrainData[chosenX, chosenY, 1] == null) return; TODO: instead of == null this should check if the block is able to spawn thing
        ISpawnable spawnableMob = SelectEnemyObjectBasedOnWeight();
        if (spawnableMob == null) return;
        for (int packIndex = 0; packIndex < spawnableMob.PackSize; packIndex++)
        {
            // Add random offsets for pack spawning, ensuring we stay within chunk bounds
            Vector2Int offset = GenerateTriangularDistributedOffset(8);
            int finalX = Mathf.Clamp(chosenX + offset.x, 0, WorldGenerator.ChunkSize.x - 1);
            int finalY = Mathf.Clamp(chosenY + offset.y, 0, maxY);
            if (ShouldSpawnHere(terrainData, lightData, finalX, finalY, spawnableMob.ColliderSize, spawnableMob.MinLightLevel, spawnableMob.MaxLightLevel))
            {
                GameObject spawnedGameObject = PoolManager.Instance.Get(spawnableMob as EnemyObject);
                spawnedGameObject.transform.position = new Vector3(finalX + chunkCoord.x * WorldGenerator.ChunkSize.x, finalY + 1, 0);
                spawnedGameObject.transform.SetParent(EnemyContainer.transform, true);
                RegisterMob(spawnedGameObject);

                // Assuming one pack per spawn attempt per category, break after successfully spawning a pack
                break;
            }
        }
        
        
        
    }

    private int FindHighestBlock(TileObject[,,] terrainData, int chosenX)
    {
        for (int y = WorldGenerator.ChunkSize.y - 1; y >= 0; y--)
        {
            if (terrainData[chosenX, y, 1] != null)
                return y;
        }

        return int.MinValue;
    }
    
    private Vector2Int GenerateTriangularDistributedOffset(int range)
    {
        // Generate two random numbers in the range [0, 1)
        float u1 = Random.Range(0f, 1f);
        float u2 = Random.Range(0f, 1f);

        // Use the square root of the inverse triangular distribution function
        float sign = u1 < 0.5f ? -1f : 1f; // Decide if we're in the lower or upper segment
        float factor = Mathf.Sqrt(2 * u2);

        // Calculate offsets based on the triangular distribution
        int offsetX = (int)(sign * range * (1 - factor));
        int offsetY = (int)(sign * range * (1 - factor));

        return new Vector2Int(offsetX, offsetY);
    }


    private void DespawnEntities()
    {
        // Implement despawning logic, based on proximity to players, mob age, etc.
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
                    Debug.Log("Chunk " + chunkCoord + ": ");
                    if (ShouldSpawnHere(tiles, lightData, x, y, enemyObject.ColliderSize, enemyObject.MinLightLevel, enemyObject.MaxLightLevel))
                    {
                        GameObject spawnedGameObject = PoolManager.Instance.Get(enemyObject as EnemyObject);
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

        if (!IsGroundAndLightSuitable(tiles, lightData, x, y, minLightLevel, maxLightLevel)) return false;

    
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
        float lightLevel = 15 * (1 - lightData[x, y]);
        bool isLightSuitable = lightLevel >= minLightLevel && lightLevel <= maxLightLevel;
        Debug.Log($"X: {x}, Y: {y}, Light Level: {lightLevel}");
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
    
    public void RegisterMob(GameObject mob)
    {
        enemyList.Add(mob);
        // Also add to a specific chunk's container based on types
        //AddEnemyToGrid(enemy, enemy.transform.position);
    }

    public static List<EnemyController> FindEnemyNearby(Vector3 worldPosition)
    {
        int chunkCoord = WorldGenerator.GetChunkCoordsFromPosition(worldPosition);
        List<EnemyController> nearbyEnemies = new List<EnemyController>();

        // Check the specified chunk and its immediate neighbors
        for (int offset = -1; offset <= 1; offset++)
        {
            int currentChunk = chunkCoord + offset;
            if (WorldGenerator.ActiveChunks.ContainsKey(currentChunk))
            {
                nearbyEnemies.AddRange(WorldGenerator.ActiveChunks[currentChunk].transform.Find("MobContainer").Find("EnemyContainer").GetComponentsInChildren<EnemyController>());
            }
        }
        return nearbyEnemies;
    }
}

[System.Serializable]
public class MobCategory
{
    public string categoryName;
    public int baseMobCap;
    internal int currentMob;
    internal float totalWeight;
    public List<CharacterObject> mobScriptableObjects; 
}

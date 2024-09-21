using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] private List<MobCategory> mobCategories;
    [SerializeField] private float safeZoneRadius = 20f;
    [SerializeField] private float spawnRate;
    [SerializeField] private float despawnDistance = 80f;
    [SerializeField] public static int waveNumber = 5;
    
    //public static Dictionary<int, GameObject> Global
    private float totalWeight = 0f;
    private List<int> currentTickActiveChunk = new List<int>();
    public static List<GameObject> enemyList = new List<GameObject>(); //TODO: get a dic for all lists;
    private GameObject player;

    private void Start()
    {
        //spawnRate = Time.fixedDeltaTime;
        player = GameObject.FindGameObjectWithTag("Player");
        InitializeTotalWeights();
        StartCoroutine(SpawnCycleCoroutine()); 
        GameEvents.current.OnNightStarted += HandleNightStart;
    }

    private void OnDestroy()
    {
        GameEvents.current.OnNightStarted -= HandleNightStart;
    }

    public static void UpdateSpawnDiff(float coeff)
    {
        Debug.Log("coeff: " + 1 + 0.4 * coeff);
        waveNumber = (int) (waveNumber * (1 + 0.4 * coeff));
        Debug.Log(waveNumber);
    }
    
    private void HandleNightStart(bool isRedMoon)
    {
        StartWaveSpawning(isRedMoon ? 2f : 1f); // Normal intensity for regular nights
    }
    
    private void StartWaveSpawning(float intensityMultiplier)
    {
        if (CoreArchitectureController.Instance == null)
            return;
        float energyZoneRadius = CoreArchitectureController.Instance.GetConstructableDistance();
        float spawnDistance = energyZoneRadius + 20f;
        Vector3 corePosition = CoreArchitectureController.Instance.transform.position;

        if (CanSpawnCategory(mobCategories[1]))
        {
            Debug.Log("CanSpawn");
            StartCoroutine(SpawnWaveAtPoint(corePosition, (int)(waveNumber * intensityMultiplier), spawnDistance, intensityMultiplier));
        }
    }
    private IEnumerator SpawnWaveAtPoint(Vector3 corePosition, int waveNumber, float spawnDistance, float intensityMultiplier)
    {
        // Assuming a constant spacing to spread out the wave along a line parallel to the core
        for (int i = 0; i < waveNumber; i++)
        {
            if (!CanSpawnCategory(mobCategories[1])) yield break;
            
            // Generate offset to create a line of enemies along the x-axis from both sides
            Vector2Int offset = GenerateTriangularDistributedOffset(8);
            Vector3 spawnPointLeft = new Vector3(corePosition.x - spawnDistance + offset.x, corePosition.y, 0);
            Vector3 spawnPointRight = new Vector3(corePosition.x + spawnDistance + offset.x, corePosition.y, 0);

            // Calculate the actual spawn points based on terrain data for both sides
            SpawnEnemyAtGround(spawnPointLeft);
            SpawnEnemyAtGround(spawnPointRight);

            // Delay next spawn in the wave for staggered appearance
            float randomDelay = Random.Range(1f, 5f) / intensityMultiplier;
            yield return new WaitForSeconds(randomDelay); // Faster spawning for more intense nights
        }
    }
    
    private void SpawnEnemyAtGround(Vector3 spawnPoint)
    {
        RaycastHit2D hit = Physics2D.Raycast(spawnPoint + Vector3.up * 50, Vector3.down, 100);
        if (hit.collider != null)
        {
            // Adjust to spawn on the topmost solid ground
            spawnPoint.y = hit.point.y + 1; // Ensure the enemy is slightly above the ground
            
            // Select the enemy type and spawn
            GameObject enemy = PoolManager.Instance.Get(SelectEnemyObjectBasedOnWeight(mobCategories[1]) as EnemyObject);
            enemy.transform.position = spawnPoint;
            RegisterMob(enemy, mobCategories[1]);
            
        }
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
            category.currentMob = category.mobList.Count;
        }
        
    }

    private void RunSpawningAttempts()
    {
        // Iterate through each mob category
        foreach (var category in mobCategories)
        {
            // Check if the global mob cap allows spawning in this category
            if (category.categoryName != "WaveEnemy" && CanSpawnCategory(category))
            {
                AttemptSpawnMobs(category);
            }
        }
        
    }

    private bool CanSpawnCategory(MobCategory category)
    {
        if (category.categoryName == "WaveEnemy")
        {
            /*Debug.Log($"Checking if can spawn category: {category.categoryName}");
            Debug.Log($"Current mob count: {category.currentMob}");
            Debug.Log($"Base mob cap: {category.baseMobCap}");
            Debug.Log($"Can spawn? {category.currentMob < category.baseMobCap}");*/
        }
        return category.currentMob < category.baseMobCap; // Placeholder change baseMob to global mob for multiple players
    }

    private void AttemptSpawnMobs(MobCategory category)
    {
        currentTickActiveChunk = new List<int>(WorldGenerator.ActiveChunks.Keys.Where(chunkId => WorldGenerator.ReadyChunks.Contains(chunkId)));
        if (currentTickActiveChunk.Count == 0)
            return;
        int randomChunkIndex = Random.Range(0, currentTickActiveChunk.Count);
        Vector2Int chunkCoord = new Vector2Int(currentTickActiveChunk[randomChunkIndex], 0);
        float[,] lightData = WorldGenerator.WorldLightData[chunkCoord];
        TileObject[,,] terrainData = WorldGenerator.WorldData[chunkCoord];
        int chosenX = Random.Range(0, WorldGenerator.ChunkSize.x);
        int maxY = FindHighestBlock(terrainData, chosenX); if (maxY == int.MinValue) return;
        
        int chosenY = Random.Range(0, maxY);
        
        //if (terrainData[chosenX, chosenY, 1] == null) return; TODO: instead of == null this should check if the block is able to spawn thing
        ISpawnable spawnableMob = SelectEnemyObjectBasedOnWeight(category);
        if (spawnableMob == null) return;

        for (int packIndex = 0; packIndex < spawnableMob.PackSize; packIndex++)
        {
            Vector2Int offset = GenerateTriangularDistributedOffset(8);
            int finalX = Mathf.Clamp(chosenX + offset.x, 0, WorldGenerator.ChunkSize.x - 1);
            int finalY = Mathf.Clamp(chosenY + offset.y, 0, maxY);
            
            if (CheckSafeZone(finalX, chunkCoord, finalY)) continue; // Skip spawning this mob due to being within the safe zone

            if (ShouldSpawnHere(terrainData, lightData, finalX, finalY, spawnableMob.ColliderSize, spawnableMob.MinLightLevel, spawnableMob.MaxLightLevel))
            {
                if (WorldGenerator.ActiveChunks.ContainsKey(chunkCoord.x))
                {
                    GameObject spawnedGameObject = PoolManager.Instance.Get(spawnableMob as EnemyObject);
                    spawnedGameObject.transform.position = new Vector3(finalX + chunkCoord.x * WorldGenerator.ChunkSize.x, finalY + 1, 0);
                    Transform enemyContainer = WorldGenerator.ActiveChunks[chunkCoord.x].transform.Find("MobContainer").Find("EnemyContainer");
                    spawnedGameObject.transform.SetParent(enemyContainer, true);
                    RegisterMob(spawnedGameObject, category);
                }
            }
        }
    }

    private bool CheckSafeZone(int finalX, Vector2Int chunkCoord, int finalY)
    {
        if (player == null){
            player =  GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return true;
        }   

        Vector3 playerPosition = player.transform.position;
        Vector3 potentialSpawnPosition = new Vector3(finalX + chunkCoord.x * WorldGenerator.ChunkSize.x, finalY + 1, 0);
        // Check if potential spawn position is within the safe zone radius
        if (Vector3.Distance(playerPosition, potentialSpawnPosition) < safeZoneRadius)
        {
            return true;
        }
        
        // Now check against the core architecture's energy zone
        float energyZoneRadius = CoreArchitectureController.Instance.GetConstructableDistance() * 1.2f;
        Vector3 corePosition = CoreArchitectureController.Instance.transform.position;
        if (Vector3.Distance(corePosition, potentialSpawnPosition) < energyZoneRadius) {
            return true; // Too close to the core architecture's energy zone
        }

        return false; // Not within the safe zones, spawn is allowed
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
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Assuming the player has a tag "Player"
        if (player == null) return;

        Vector3 playerPosition = player.transform.position;

        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            float distance = Vector3.Distance(playerPosition, enemyList[i].transform.position);
            if (distance > despawnDistance)
            {
                GameObject toDespawn = enemyList[i];
                enemyList.RemoveAt(i);
                var category = mobCategories.FirstOrDefault(c => c.mobList.Contains(toDespawn));
                if (category != null)
                {
                    category.mobList.Remove(toDespawn);
                }
                PoolManager.Instance.Return(toDespawn, toDespawn.GetComponent<IPoolableObjectController>().PoolableObject); // Assuming you have a method to return it to the pool or destroy it
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
       
        return isGround && isLightSuitable;
    }

    
    private ISpawnable SelectEnemyObjectBasedOnWeight(MobCategory category)
    {
        float randomWeight = Random.Range(0, category.totalWeight);
        float currentWeight = 0f;
        
        foreach (var enemy in category.mobScriptableObjects)
        {
            if (enemy != null)
            {
                currentWeight += (enemy as ISpawnable).SpawnWeight;
                if (currentWeight >= randomWeight)
                {
                    return enemy as ISpawnable;
                }
            }
        }
        
        return null; 
    }
    
    public void RegisterMob(GameObject mob, MobCategory mobCategory)
    {
        enemyList.Add(mob);
        mobCategory.mobList.Add(mob);
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
    public List<GameObject> mobList = new List<GameObject>();
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    private List<int> currentTickActiveChunk = new();
    public static List<GameObject> enemyList = new(); //TODO: get a dic for all lists;
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
        waveNumber = (int)(waveNumber * (1 + 0.4 * coeff));
        Debug.Log(waveNumber);
    }

    private void HandleNightStart(bool isRedMoon)
    {
        StartWaveSpawning(isRedMoon ? 2f : 1f); // Normal intensity for regular nights
    }

    private void StartWaveSpawning(float intensityMultiplier)
    {
        if (CoreArchitectureController.Instance == null) return;
        var energyZoneRadius = CoreArchitectureController.Instance.GetConstructableDistance();
        var spawnDistance = energyZoneRadius + 20f;
        var corePosition = CoreArchitectureController.Instance.transform.position;
        if (CanSpawnCategory(mobCategories[1]))
            StartCoroutine(SpawnWaveAtPoint(corePosition, (int)(waveNumber * intensityMultiplier), spawnDistance,
                intensityMultiplier));
    }

    private IEnumerator SpawnWaveAtPoint(Vector3 corePosition, int waveNumber, float spawnDistance,
        float intensityMultiplier)
    {
        // Assuming a constant spacing to spread out the wave along a line parallel to the core
        for (var i = 0; i < waveNumber; i++)
        {
            if (!CanSpawnCategory(mobCategories[1])) yield break;

            // Generate offset to create a line of enemies along the x-axis from both sides
            var offset = GenerateTriangularDistributedOffset(8);
            var spawnPointLeft = new Vector3(corePosition.x - spawnDistance + offset.x, corePosition.y, 0);
            var spawnPointRight = new Vector3(corePosition.x + spawnDistance + offset.x, corePosition.y, 0);

            // Calculate the actual spawn points based on terrain data for both sides
            SpawnEnemyAtGround(spawnPointLeft);
            SpawnEnemyAtGround(spawnPointRight);

            // Delay next spawn in the wave for staggered appearance
            var randomDelay = Random.Range(1f, 5f) / intensityMultiplier;
            yield return new WaitForSeconds(randomDelay); // Faster spawning for more intense nights
        }
    }

    private void SpawnEnemyAtGround(Vector3 spawnPoint)
    {
        var hit = Physics2D.Raycast(spawnPoint + Vector3.up * 50, Vector3.down, 100);
        if (hit.collider != null)
        {
            // Adjust to spawn on the topmost solid ground
            spawnPoint.y = hit.point.y + 1; // Ensure the enemy is slightly above the ground

            // Select the enemy type and spawn
            var enemy =
                PoolManager.Instance.Get(SelectEnemyObjectBasedOnWeight(mobCategories[1]) as EnemyObject);
            enemy.transform.position = spawnPoint;
            RegisterMob(enemy, mobCategories[1]);
        }
    }

    private void InitializeTotalWeights()
    {
        foreach (var mc in mobCategories)
        foreach (var o in mc.mobScriptableObjects)
        {
            var so = (ISpawnable)o;
            mc.totalWeight += so.SpawnWeight;
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

    private void
        CalculateGlobalCap() //for multiplayer dont need to care now global = mobcap-value * totalSpawnableCap / a pre-defined default spawnable chunks number
    {
        foreach (var mc in mobCategories)
        {
        }
    }

    private void CollectMobCapData()
    {
        // Calculate current mob counts per category and update mobsInCategory dictionary
        foreach (var category in mobCategories) category.currentMob = category.mobList.Count;
    }

    private void RunSpawningAttempts()
    {
        // Iterate through each mob category
        foreach (var category in mobCategories)
            // Check if the global mob cap allows spawning in this category
            if (category.categoryName != "WaveEnemy" && CanSpawnCategory(category))
                AttemptSpawnMobs(category);
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

        return
            category.currentMob < category.baseMobCap; // Placeholder change baseMob to global mob for multiple players
    }

    private void AttemptSpawnMobs(MobCategory category)
    {
        currentTickActiveChunk =
            new List<int>(
                WorldGenerator.ActiveChunks.Keys.Where(chunkId => WorldGenerator.ReadyChunks.Contains(chunkId)));
        if (currentTickActiveChunk.Count == 0) return;
        var randomChunkIndex = Random.Range(0, currentTickActiveChunk.Count);
        var chunkCoord = new Vector2Int(currentTickActiveChunk[randomChunkIndex], 0);
        var lightData = WorldGenerator.WorldLightData[chunkCoord];
        var terrainData = WorldGenerator.WorldData[chunkCoord];
        var chosenX = Random.Range(0, WorldGenerator.ChunkSize.x);
        var maxY = FindHighestBlock(terrainData, chosenX);
        if (maxY == int.MinValue) return;
        var chosenY = Random.Range(0, maxY);

        //if (terrainData[chosenX, chosenY, 1] == null) return; TODO: instead of == null this should check if the block is able to spawn thing
        var spawnableMob = SelectEnemyObjectBasedOnWeight(category);
        if (spawnableMob == null) return;
        for (var packIndex = 0; packIndex < spawnableMob.PackSize; packIndex++)
        {
            var offset = GenerateTriangularDistributedOffset(8);
            var finalX = Mathf.Clamp(chosenX + offset.x, 0, WorldGenerator.ChunkSize.x - 1);
            var finalY = Mathf.Clamp(chosenY + offset.y, 0, maxY);
            if (CheckSafeZone(finalX, chunkCoord, finalY))
                continue; // Skip spawning this mob due to being within the safe zone
            if (ShouldSpawnHere(terrainData, lightData, finalX, finalY, spawnableMob.ColliderSize,
                    spawnableMob.MinLightLevel, spawnableMob.MaxLightLevel))
                if (WorldGenerator.ActiveChunks.ContainsKey(chunkCoord.x))
                {
                    var spawnedGameObject = PoolManager.Instance.Get(spawnableMob as EnemyObject);
                    spawnedGameObject.transform.position =
                        new Vector3(finalX + chunkCoord.x * WorldGenerator.ChunkSize.x, finalY + 1, 0);
                    var enemyContainer = WorldGenerator.ActiveChunks[chunkCoord.x].transform.Find("MobContainer")
                        .Find("EnemyContainer");
                    spawnedGameObject.transform.SetParent(enemyContainer, true);
                    RegisterMob(spawnedGameObject, category);
                }
        }
    }

    private bool CheckSafeZone(int finalX, Vector2Int chunkCoord, int finalY)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return true;
        }

        var playerPosition = player.transform.position;
        var potentialSpawnPosition = new Vector3(finalX + chunkCoord.x * WorldGenerator.ChunkSize.x, finalY + 1, 0);
        // Check if potential spawn position is within the safe zone radius
        if (Vector3.Distance(playerPosition, potentialSpawnPosition) < safeZoneRadius) return true;

        // Now check against the core architecture's energy zone
        var energyZoneRadius = CoreArchitectureController.Instance.GetConstructableDistance() * 1.2f;
        var corePosition = CoreArchitectureController.Instance.transform.position;
        if (Vector3.Distance(corePosition, potentialSpawnPosition) <
            energyZoneRadius) return true; // Too close to the core architecture's energy zone

        return false; // Not within the safe zones, spawn is allowed
    }

    private int FindHighestBlock(TileObject[,,] terrainData, int chosenX)
    {
        for (var y = WorldGenerator.ChunkSize.y - 1; y >= 0; y--)
            if (terrainData[chosenX, y, 1] != null)
                return y;

        return int.MinValue;
    }

    private Vector2Int GenerateTriangularDistributedOffset(int range)
    {
        // Generate two random numbers in the range [0, 1)
        var u1 = Random.Range(0f, 1f);
        var u2 = Random.Range(0f, 1f);

        // Use the square root of the inverse triangular distribution function
        var sign = u1 < 0.5f ? -1f : 1f; // Decide if we're in the lower or upper segment
        var factor = Mathf.Sqrt(2 * u2);

        // Calculate offsets based on the triangular distribution
        var offsetX = (int)(sign * range * (1 - factor));
        var offsetY = (int)(sign * range * (1 - factor));
        return new Vector2Int(offsetX, offsetY);
    }

    private void DespawnEntities()
    {
        var player = GameObject.FindGameObjectWithTag("Player"); // Assuming the player has a tag "Player"
        if (player == null) return;
        var playerPosition = player.transform.position;
        for (var i = enemyList.Count - 1; i >= 0; i--)
        {
            var distance = Vector3.Distance(playerPosition, enemyList[i].transform.position);
            if (distance > despawnDistance)
            {
                var toDespawn = enemyList[i];
                enemyList.RemoveAt(i);
                var category = mobCategories.FirstOrDefault(c => c.mobList.Contains(toDespawn));
                if (category != null) category.mobList.Remove(toDespawn);

                PoolManager.Instance.Return(toDespawn,
                    toDespawn.GetComponent<IPoolableObjectController>()
                        .PoolableObject); // Assuming you have a method to return it to the pool or destroy it
            }
        }
    }

    private bool ShouldSpawnHere(TileObject[,,] tiles, float[,] lightData, int x, int y, Vector2Int colliderSize,
        float minLightLevel, float maxLightLevel)
    {
        if (!IsGroundAndLightSuitable(tiles, lightData, x, y, minLightLevel, maxLightLevel)) return false;
        var startX = x - colliderSize.x / 2;
        var endX = startX + colliderSize.x - 1;
        var startY = y + 1;
        var endY = startY + colliderSize.y - 1; // if y = 0, start y is 1, collider size is 2, then check endy to 3
        for (var checkX = startX; checkX <= endX; checkX++)
        for (var checkY = startY; checkY <= endY; checkY++)
        {
            // Ensure the check is within bounds (assuming y+2 is within bounds since y is the base)
            if (checkX < 0 || checkX >= WorldGenerator.ChunkSize.x || checkY < 0 ||
                checkY >= WorldGenerator.ChunkSize.y)
                return false; // Out of bounds, cannot spawn here

            if (tiles[checkX, checkY, 1] != null) return false; // Entity present at the base level
        }

        return true; // Suitable location found
    }

    // Helper method to check if a specific tile is suitable based on ground and light conditions
    private bool IsGroundAndLightSuitable(TileObject[,,] tiles, float[,] lightData, int x, int y, float minLightLevel,
        float maxLightLevel)
    {
        var isGround = tiles[x, y, 1] != null; // Assuming layer 1 is the entity layer, such as ground
        var lightLevel = 15 * (1 - lightData[x, y]);
        var isLightSuitable = lightLevel >= minLightLevel && lightLevel <= maxLightLevel;
        return isGround && isLightSuitable;
    }

    private ISpawnable SelectEnemyObjectBasedOnWeight(MobCategory category)
    {
        var randomWeight = Random.Range(0, category.totalWeight);
        var currentWeight = 0f;
        foreach (var enemy in category.mobScriptableObjects)
            if (enemy != null)
            {
                currentWeight += (enemy as ISpawnable).SpawnWeight;
                if (currentWeight >= randomWeight) return enemy as ISpawnable;
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
        var chunkCoord = WorldGenerator.GetChunkCoordsFromPosition(worldPosition);
        var nearbyEnemies = new List<EnemyController>();

        // Check the specified chunk and its immediate neighbors
        for (var offset = -1; offset <= 1; offset++)
        {
            var currentChunk = chunkCoord + offset;
            if (WorldGenerator.ActiveChunks.ContainsKey(currentChunk))
                nearbyEnemies.AddRange(WorldGenerator.ActiveChunks[currentChunk].transform.Find("MobContainer")
                    .Find("EnemyContainer").GetComponentsInChildren<EnemyController>());
        }

        return nearbyEnemies;
    }
}

[Serializable]
public class MobCategory
{
    public string categoryName;
    public int baseMobCap;
    internal int currentMob;
    internal float totalWeight;
    public List<CharacterObject> mobScriptableObjects;
    public List<GameObject> mobList = new();
}
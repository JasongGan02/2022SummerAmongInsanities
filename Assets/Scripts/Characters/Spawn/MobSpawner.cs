using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MobSpawner : MonoBehaviour
{
    #region Fields

    [SerializeField] private List<MobCategory> mobCategories;
    [SerializeField] private float safeZoneRadius = 20f;
    [SerializeField] private float spawnRate;
    [SerializeField] private float despawnDistance = 80f;
    [SerializeField] public static int waveNumber = 5;
    private LayerMask groundLayerMask;

    private float totalWeight = 0f;
    private List<int> currentTickActiveChunk = new();
    public static List<GameObject> enemyList = new(); // TODO: get a dic for all lists;
    private GameObject player;
    private EnemyRedMoonEffectObject enemyRedMoonEffectObject;

    #endregion

    #region Unity Callbacks

    private async void Start()
    {
        // spawnRate = Time.fixedDeltaTime;
        player = GameObject.FindGameObjectWithTag("Player");
        groundLayerMask = LayerMask.GetMask("ground");
        enemyRedMoonEffectObject = await AddressablesManager.Instance.LoadAssetAsync<EnemyRedMoonEffectObject>("Assets/Scripts/Effects/SO/EnemyRedMoonEffectObject.asset");
        InitializeTotalWeights();
        StartCoroutine(SpawnCycleCoroutine());
        GameEvents.current.OnNightStarted += HandleNightStart;
    }

    private void OnDestroy()
    {
        GameEvents.current.OnNightStarted -= HandleNightStart;
    }

    #endregion

    #region Public Methods

    public static void UpdateSpawnDiff(float coeff)
    {
        Debug.Log("coeff: " + (1 + 0.4 * coeff));
        waveNumber = (int)(waveNumber * (1 + 0.4 * coeff));
        Debug.Log(waveNumber);
    }

    public static List<EnemyController> FindEnemyNearby(Vector3 worldPosition)
    {
        var chunkCoord = WorldGenerator.GetChunkCoordsFromPosition(worldPosition);
        var nearbyEnemies = new List<EnemyController>();

        // Check the specified chunk and its immediate neighbors.
        for (var offset = -1; offset <= 1; offset++)
        {
            var currentChunk = chunkCoord + offset;
            if (WorldGenerator.ActiveChunks.ContainsKey(currentChunk))
            {
                nearbyEnemies.AddRange(
                    WorldGenerator.ActiveChunks[currentChunk]
                        .transform.Find("MobContainer")
                        .Find("EnemyContainer")
                        .GetComponentsInChildren<EnemyController>()
                );
            }
        }

        return nearbyEnemies;
    }

    #endregion

    #region Event Handlers

    private void HandleNightStart(bool isRedMoon)
    {
        // Normal intensity for regular nights; red moon nights spawn with double intensity.
        StartWaveSpawning(isRedMoon ? 2f : 1f);
        enemyRedMoonEffectObject.InitializeEffectObject();
    }

    #endregion

    #region Wave Spawning

    private void StartWaveSpawning(float intensityMultiplier)
    {
        if (CoreArchitectureController.Instance == null)
            return;

        var energyZoneRadius = CoreArchitectureController.Instance.GetConstructableDistance();
        var spawnDistance = energyZoneRadius + 20f;
        var corePosition = CoreArchitectureController.Instance.transform.position;

        // Assuming WaveEnemy is at index 1 in the mobCategories list.
        if (CanSpawnCategory(mobCategories[1]))
        {
            StartCoroutine(
                SpawnWaveAtPoint(corePosition, (int)(waveNumber * intensityMultiplier), spawnDistance, intensityMultiplier)
            );
        }
    }

    private IEnumerator SpawnWaveAtPoint(Vector3 corePosition, int waveNumber, float spawnDistance, float intensityMultiplier)
    {
        yield return new WaitUntil(() => CoreArchitectureController.Instance != null);

        // Spawn a wave of enemies along a line parallel to the core position.
        for (var i = 0; i < waveNumber; i++)
        {
            if (!CanSpawnCategory(mobCategories[1]))
                yield break;

            // Generate an offset to position enemies along the x-axis.
            var offset = GenerateTriangularDistributedOffset(8);
            var spawnPointLeft = new Vector3(corePosition.x - spawnDistance + offset.x, corePosition.y, 0);
            var spawnPointRight = new Vector3(corePosition.x + spawnDistance + offset.x, corePosition.y, 0);

            // Select enemy prefab from the WaveEnemy category.
            var enemyPrefab = SelectEnemyObjectBasedOnWeight(mobCategories[1]) as EnemyObject;

            // Randomly choose how to spawn:
            // 0: spawn only left, 1: spawn only right.
            int spawnChoice = Random.Range(0, 2);
            if (spawnChoice == 0)
            {
                SpawnEnemy(enemyPrefab, spawnPointLeft, mobCategories[1], null, "WaveSpawnLeft");
            }
            else if (spawnChoice == 1)
            {
                SpawnEnemy(enemyPrefab, spawnPointRight, mobCategories[1], null, "WaveSpawnRight");
            }

            // Delay the next spawn for a staggered appearance.
            var randomDelay = Random.Range(1f, 5f) / intensityMultiplier;
            yield return new WaitForSeconds(randomDelay);
        }
    }

    #endregion

    #region Spawning Cycle

    private IEnumerator SpawnCycleCoroutine()
    {
        yield return new WaitUntil(() => CoreArchitectureController.Instance != null);

        while (CoreArchitectureController.Instance != null)
        {
            CollectMobCapData();
            RunSpawningAttempts();
            DespawnEntities();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    private void CollectMobCapData()
    {
        // Update current mob counts for each category.
        foreach (var category in mobCategories)
            category.currentMob = category.mobList.Count;
    }

    private void RunSpawningAttempts()
    {
        // Iterate through each mob category (except "WaveEnemy") and attempt to spawn mobs.
        foreach (var category in mobCategories)
        {
            if (category.categoryName != "WaveEnemy" && CanSpawnCategory(category))
                AttemptSpawnMobs(category);
        }
    }

    private bool CanSpawnCategory(MobCategory category)
    {
        // Additional checks for "WaveEnemy" can be implemented here if needed.
        return category.currentMob < category.baseMobCap;
    }

    private void AttemptSpawnMobs(MobCategory category)
    {
        currentTickActiveChunk = new List<int>(
            WorldGenerator.ActiveChunks.Keys.Where(chunkId => WorldGenerator.ReadyChunks.Contains(chunkId))
        );
        if (currentTickActiveChunk.Count == 0)
            return;

        var randomChunkIndex = Random.Range(0, currentTickActiveChunk.Count);
        var chunkCoord = new Vector2Int(currentTickActiveChunk[randomChunkIndex], 0);
        var lightData = WorldGenerator.WorldLightData[chunkCoord];
        var terrainData = WorldGenerator.WorldData[chunkCoord];

        var chosenX = Random.Range(0, WorldGenerator.ChunkSize.x);
        var maxY = FindHighestBlock(terrainData, chosenX);
        if (maxY == int.MinValue)
            return;

        var chosenY = Random.Range(0, maxY);
        var spawnableMob = SelectEnemyObjectBasedOnWeight(category);
        if (spawnableMob == null)
            return;

        for (var packIndex = 0; packIndex < spawnableMob.PackSize; packIndex++)
        {
            var offset = GenerateTriangularDistributedOffset(8);
            var finalX = Mathf.Clamp(chosenX + offset.x, 0, WorldGenerator.ChunkSize.x - 1);
            var finalY = Mathf.Clamp(chosenY + offset.y, 0, maxY);


            if (ShouldSpawnHere(terrainData, lightData, finalX, finalY, spawnableMob.ColliderSize,
                    spawnableMob.MinLightLevel, spawnableMob.MaxLightLevel))
            {
                var spawnPosition = new Vector3(finalX + chunkCoord.x * WorldGenerator.ChunkSize.x, finalY, 0);
                if (!CheckSafeZone(spawnPosition))
                    SpawnEnemy(spawnableMob as EnemyObject, spawnPosition, category, null, "CycleSpawn");
            }
        }
    }

    private void DespawnEntities()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player"); // Assuming the player has a tag "Player".
        if (playerObj == null)
            return;
        var playerPosition = playerObj.transform.position;

        for (var i = enemyList.Count - 1; i >= 0; i--)
        {
            var enemy = enemyList[i];
            var distance = Vector3.Distance(playerPosition, enemy.transform.position);
            if (distance > despawnDistance)
            {
                // Remove enemy from the global list.
                enemyList.RemoveAt(i);
                // Remove enemy from its category.
                var category = mobCategories.FirstOrDefault(c => c.mobList.Contains(enemy));
                if (category != null)
                    category.mobList.Remove(enemy);

                // Return the enemy to the pool.
                PoolManager.Instance.Return(
                    enemy,
                    enemy.GetComponent<IPoolableObjectController>().PoolableObject
                );
            }
        }
    }

    #endregion

    #region Unified Spawning Function

    /// <summary>
    /// Unified spawning function to handle all enemy spawns.
    /// </summary>
    /// <param name="enemyPrefab">The enemy prefab to spawn.</param>
    /// <param name="spawnPoint">The initial spawn position.</param>
    /// <param name="category">The mob category to register the enemy with.</param>
    /// <param name="parent">Optional parent transform for the spawned enemy.</param>
    /// <param name="useGroundAdjustment">
    /// If true, perform a raycast from above the spawn point downward to adjust the spawn position
    /// so that the enemy sits on the ground.
    /// </param>
    private void SpawnEnemy(EnemyObject enemyPrefab, Vector3 spawnPoint, MobCategory category, Transform parent = null, string debugSource = "Unknown")
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"[SpawnEnemy] Called from {debugSource}: enemyPrefab is null, spawn aborted.");
            return;
        }

        // Always perform a ground check using the provided ground layer mask.
        RaycastHit2D hit = Physics2D.Raycast(spawnPoint + Vector3.up, Vector3.down, 2, groundLayerMask);
        if (hit.collider == null)
        {
            Debug.LogWarning($"[SpawnEnemy] Called from {debugSource}: no valid ground found at spawnPoint {spawnPoint}");
            return; // No valid ground found.
        }

        // Adjust spawn point so the enemy sits slightly above the ground.
        spawnPoint.y = hit.point.y + 1;

        // If no parent was provided, determine it based on the spawn point.
        if (parent == null)
        {
            parent = GetEnemyContainerForSpawnPoint(spawnPoint);
        }

        // Get the enemy from your pool and set its position.
        var enemy = PoolManager.Instance.Get(enemyPrefab);
        enemy.transform.position = spawnPoint;

        // Set the parent if it was found.
        if (parent != null)
        {
            enemy.transform.SetParent(parent, true);
        }

        if (TimeSystemManager.Instance.IsRedMoon)
        {
            enemyRedMoonEffectObject.ExecuteEffect(enemy.GetComponent<IEffectableController>());
        }

        RegisterMob(enemy, category);

        // Draw debug lines to indicate distances to the player and core.
        if (player != null)
        {
            // Draw a line from the enemy to the player (magenta).
            Debug.DrawLine(enemy.transform.position, player.transform.position, Color.magenta, 2f);
            Debug.Log($"[SpawnEnemy] {enemyPrefab.name} spawned at {spawnPoint} from {debugSource}. Distance to Player: {Vector3.Distance(enemy.transform.position, player.transform.position):F2}");
        }
        else
        {
            Debug.LogWarning("[SpawnEnemy]: Player reference is missing.");
        }

        if (CoreArchitectureController.Instance != null)
        {
            // Draw a line from the enemy to the core (cyan).
            Debug.DrawLine(enemy.transform.position, CoreArchitectureController.Instance.transform.position, Color.cyan, 2f);
            Debug.Log($"[SpawnEnemy] Distance to Core: {Vector3.Distance(enemy.transform.position, CoreArchitectureController.Instance.transform.position):F2}");
        }
        else
        {
            Debug.LogWarning("[SpawnEnemy]: CoreArchitectureController instance is missing.");
        }

        // Debug output indicating a successful spawn.
        Debug.Log($"[SpawnEnemy] {enemyPrefab.name} spawned at {spawnPoint} from {debugSource} and registered under category '{category.categoryName}'.");
    }

    #endregion

    #region Utility Methods

    private void InitializeTotalWeights()
    {
        foreach (var mc in mobCategories)
        {
            foreach (var o in mc.mobScriptableObjects)
            {
                var so = (ISpawnable)o;
                mc.totalWeight += so.SpawnWeight;
            }
        }
    }

    private bool CheckSafeZone(Vector3 spawnPosition)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[CheckSafeZone]: No player found.");
                return true; // Prevent spawning if no player is found.
            }
        }

        // Log player position and safe zone radius.
        Debug.Log($"[CheckSafeZone] Player position: {player.transform.position}, SafeZoneRadius: {safeZoneRadius}");

        float distanceToPlayer = Vector3.Distance(player.transform.position, spawnPosition);
        Debug.Log($"[CheckSafeZone] Spawn position: {spawnPosition}, Computed distance to Player: {distanceToPlayer}");

        if (distanceToPlayer < safeZoneRadius)
        {
            Debug.Log($"[CheckSafeZone]: Spawn position {spawnPosition} is within the player's safe zone (distance: {distanceToPlayer}).");
            return true;
        }

        if (CoreArchitectureController.Instance != null)
        {
            Vector3 corePosition = CoreArchitectureController.Instance.transform.position;
            float energyRadius = CoreArchitectureController.Instance.GetConstructableDistance() * 1.2f;
            float distanceToCore = Vector3.Distance(corePosition, spawnPosition);
            Debug.Log($"[CheckSafeZone] Core position: {corePosition}, EnergyRadius: {energyRadius}, Computed distance to Core: {distanceToCore}");

            if (distanceToCore < energyRadius)
            {
                Debug.Log($"[CheckSafeZone]: Spawn position {spawnPosition} is within the core's safe zone (distance: {distanceToCore}).");
                return true;
            }
        }

        Debug.Log($"[CheckSafeZone]: Spawn position {spawnPosition} passed safe zone check.");
        return false;
    }


    private int FindHighestBlock(TileObject[,,] terrainData, int chosenX)
    {
        for (var y = WorldGenerator.ChunkSize.y - 1; y >= 0; y--)
        {
            if (terrainData[chosenX, y, 1] != null)
                return y;
        }

        return int.MinValue;
    }

    private Vector2Int GenerateTriangularDistributedOffset(int range)
    {
        // Generate two random numbers in [0, 1).
        var u1 = Random.Range(0f, 1f);
        var u2 = Random.Range(0f, 1f);

        // Decide the sign based on u1 and calculate the factor.
        var sign = u1 < 0.5f ? -1f : 1f;
        var factor = Mathf.Sqrt(2 * u2);

        // Calculate the offset using the triangular distribution.
        var offsetX = (int)(sign * range * (1 - factor));
        var offsetY = (int)(sign * range * (1 - factor));
        return new Vector2Int(offsetX, offsetY);
    }

    private bool ShouldSpawnHere(TileObject[,,] tiles, float[,] lightData, int x, int y, Vector2Int colliderSize, float minLightLevel, float maxLightLevel)
    {
        if (!IsGroundAndLightSuitable(tiles, lightData, x, y, minLightLevel, maxLightLevel))
            return false;

        var startX = x - colliderSize.x / 2;
        var endX = startX + colliderSize.x - 1;
        var startY = y + 1;
        var endY = startY + colliderSize.y - 1;

        for (var checkX = startX; checkX <= endX; checkX++)
        {
            for (var checkY = startY; checkY <= endY; checkY++)
            {
                // Ensure the check is within bounds.
                if (checkX < 0 || checkX >= WorldGenerator.ChunkSize.x ||
                    checkY < 0 || checkY >= WorldGenerator.ChunkSize.y)
                    return false;

                if (tiles[checkX, checkY, 1] != null)
                    return false;
            }
        }

        return true;
    }

    private bool IsGroundAndLightSuitable(TileObject[,,] tiles, float[,] lightData, int x, int y, float minLightLevel, float maxLightLevel)
    {
        var isGround = tiles[x, y, 1] != null; // Assuming layer 1 represents ground.
        var lightLevel = 15 * (1 - lightData[x, y]);
        var isLightSuitable = lightLevel >= minLightLevel && lightLevel <= maxLightLevel;
        return isGround && isLightSuitable;
    }

    private ISpawnable SelectEnemyObjectBasedOnWeight(MobCategory category)
    {
        var randomWeight = Random.Range(0, category.totalWeight);
        var currentWeight = 0f;
        foreach (var enemy in category.mobScriptableObjects)
        {
            if (enemy != null)
            {
                currentWeight += (enemy as ISpawnable).SpawnWeight;
                if (currentWeight >= randomWeight)
                    return enemy as ISpawnable;
            }
        }

        return null;
    }

    public void RegisterMob(GameObject mob, MobCategory mobCategory)
    {
        enemyList.Add(mob);
        mobCategory.mobList.Add(mob);
    }

    private Transform GetEnemyContainerForSpawnPoint(Vector3 spawnPoint)
    {
        // Get the chunk coordinate for the spawnPoint.
        int chunkCoord = WorldGenerator.GetChunkCoordsFromPosition(spawnPoint);
        // Optionally, if you want the local coordinates within the chunk:
        // Vector2Int localCoords = WorldGenerator.WorldToLocalCoords(new Vector2Int((int)spawnPoint.x, (int)spawnPoint.y), chunkCoord);

        // Look for the chunk in the ActiveChunks dictionary.
        if (WorldGenerator.ActiveChunks.TryGetValue(chunkCoord, out GameObject chunk))
        {
            // Assuming that the chunk GameObject has a child "MobContainer"
            // and under that a child "EnemyContainer".
            Transform mobContainer = chunk.transform.Find("MobContainer");
            if (mobContainer != null)
            {
                Transform enemyContainer = mobContainer.Find("EnemyContainer");
                return enemyContainer;
            }
        }

        return null;
    }

    #endregion

    #region Unused / Placeholder Methods

    private void CalculateGlobalCap()
    {
        // For multiplayer: global = mobcap-value * totalSpawnableCap / a pre-defined default spawnable chunks number.
        foreach (var mc in mobCategories)
        {
            // No implementation provided.
        }
    }

    #endregion
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
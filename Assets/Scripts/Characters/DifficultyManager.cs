using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DifficultyManager : MonoBehaviour
{
    public bool stopLevelingUp = false;
    public int playerCount = 1; // Number of players in the game
    public float difficultyValue = 1; // 1 for Easy, 2 for Medium, 3 for Hard

    private List<EnemyObject> enemyObjects = new List<EnemyObject>(); // Dynamically loaded enemy objects
    private int daysCompleted = 0; // Days completed in the game
    private float timeInMinutes = 0; // Time in minutes since the game started

    private int enemyLevel;
    private float playerFactor;
    private float _coeff;

    private async void Start()
    {
        GameEvents.current.OnDayUpdated += OnDayPassed;
        GameEvents.current.OnNightStarted += UpdateSpawnDiff;

        // Load enemy objects dynamically
        LoadEnemyObjects();
    }

    private async void LoadEnemyObjects()
    {
        try
        {
            var loadedEnemies = await AddressablesManager.Instance.LoadMultipleAssetsExactTypeAsync<EnemyObject>("EnemyObject");
            if (loadedEnemies != null && loadedEnemies.Count > 0)
            {
                enemyObjects.AddRange(loadedEnemies);
            }
            else
            {
                Debug.LogWarning("No enemy objects loaded. Ensure assets are tagged correctly.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load enemy objects: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        GameEvents.current.OnDayUpdated -= OnDayPassed;
        GameEvents.current.OnNightStarted -= UpdateSpawnDiff;
    }

    private void Update()
    {
        if (stopLevelingUp) return;

        timeInMinutes += Time.deltaTime / 60; // Convert seconds to minutes
        UpdateDifficultyCoefficient();
    }

    private void UpdateDifficultyCoefficient()
    {
        // Cache player factor and time factor to avoid recalculating every frame
        playerFactor = 1 + 0.3f * (playerCount - 1);
        float timeFactor = 0.0506f * difficultyValue * Mathf.Pow(playerCount, 0.2f);
        float daysFactor = Mathf.Pow(1.13f, daysCompleted);

        _coeff = (playerFactor + timeInMinutes * timeFactor) * daysFactor;

        // Update game systems based on the new coefficient
        UpdateEnemyLevels(_coeff);
        UpdateInteractableCosts(_coeff);
        UpdateEnemyRewards(_coeff);
    }

    private void OnDayPassed(int day)
    {
        daysCompleted++;
    }


    private void UpdateEnemyLevels(float coeff)
    {
        int newEnemyLevel = (int)(-2 + (coeff - playerFactor) / 0.33f);
        if (newEnemyLevel > enemyLevel)
        {
            Debug.Log($"Enemy level updated to: {newEnemyLevel}");
            enemyLevel = newEnemyLevel;
            UpdateAllEnemies();
        }
    }

    private void UpdateAllEnemies()
    {
        if (enemyObjects == null || enemyObjects.Count == 0)
        {
            Debug.LogWarning("No enemy objects available to update.");
            return;
        }

        // Update dynamically loaded enemy objects
        foreach (var enemyObject in enemyObjects)
        {
            enemyObject.LevelUp();
        }

        // Update spawned enemies in the scene
        if (MobSpawner.enemyList != null)
        {
            foreach (var enemy in MobSpawner.enemyList)
            {
                if (enemy != null)
                {
                    var controller = enemy.GetComponent<EnemyController>();
                    if (controller != null)
                    {
                        controller.LevelUp();
                    }
                }
            }
        }
    }

    public void UpdateSpawnDiff(bool isRedMoon)
    {
        if (MobSpawner.waveNumber <= 0)
        {
            Debug.LogWarning("Invalid wave number. Cannot update spawn difficulty.");
            return;
        }

        MobSpawner.waveNumber = (int)(MobSpawner.waveNumber * (1 + 0.2f * _coeff));
    }

    private void UpdateInteractableCosts(float coeff)
    {
        // Example: Adjust interactable costs based on difficulty
        // moneyCost = baseCost * Mathf.Pow(coeff, 1.25f);
    }

    private void UpdateEnemyRewards(float coeff)
    {
        // Example: Adjust enemy rewards based on difficulty
        // enemyXPReward = coeff * monsterValue * rewardMultiplier;
        // enemyGoldReward = 2 * coeff * monsterValue * rewardMultiplier;
    }
}
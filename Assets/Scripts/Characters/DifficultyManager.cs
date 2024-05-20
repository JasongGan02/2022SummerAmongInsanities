using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;

public class DifficultyManager : MonoBehaviour
{
    public bool stopLevelingUp = false;
    public int playerCount = 1; // Number of players in the game
    public float difficultyValue = 1; // 1 for Easy, 2 for Medium, 3 for Hard
    public EnemyObject[] enemyObjects;
    private int daysCompleted = 0; // Days completed in the game
    private bool isRedMoon = false;
    private float timeInMinutes = 0; // Time in minutes since the game started
    
    private int enemyLevel;
    private float playerFactor;
    private TimeSystemManager timeSystemManager;
    private float _coeff;

    private void Start()
    {
        timeSystemManager = FindObjectOfType<TimeSystemManager>();
        timeSystemManager.onDayUpdated += OnDayPassed;
        TimeSystemManager.onNightStarted += OnRedMoonNight;
        TimeSystemManager.onNightStarted += UpdateSpawnDiff;
    }

    private void OnDestroy()
    {
        timeSystemManager.onDayUpdated -= OnDayPassed;
        TimeSystemManager.onNightStarted -= OnRedMoonNight;
    }

    private void Update()
    {
        timeInMinutes += Time.deltaTime / 60; // Convert seconds to minutes
        if (!stopLevelingUp)
            UpdateDifficultyCoefficient();
    }

    

    private void UpdateDifficultyCoefficient()
    {
        float playerFactor = 1 + 0.3f * (playerCount - 1);
        float timeFactor = 0.0506f * difficultyValue * Mathf.Pow(playerCount, 0.2f);
        float daysFactor = Mathf.Pow(1.13f, daysCompleted);

        _coeff = (playerFactor + timeInMinutes * timeFactor) * daysFactor;
        
        // Use coeff to adjust enemy levels, money costs, and rewards as necessary
        // For example: 
        UpdateEnemyLevels(_coeff);
        UpdateInteractableCosts(_coeff);
        UpdateEnemyRewards(_coeff);
    }

    private void OnDayPassed(int day)
    {
        daysCompleted++;
    }

    private void OnRedMoonNight(bool isRedMoon)
    {
        daysCompleted++;
    }

    private void UpdateEnemyLevels(float coeff)
    {
        int newEnemyLevel = (int)(-2 + (coeff - playerFactor) / 0.33f);
        if (newEnemyLevel > enemyLevel)
        {
            Debug.Log(newEnemyLevel);
            enemyLevel = newEnemyLevel;
            UpdateAllEnemies();
        }

    }

    private void UpdateAllEnemies()
    {
        foreach (var enemyObject in enemyObjects)
        {
            enemyObject.LevelUp();
        }
        // Find all EnemyController instances and update them
        foreach (var enemy in MobSpawner.enemyList)
        {
            enemy.GetComponent<EnemyController>().LevelUp();
        }
    }
    
    public void UpdateSpawnDiff(bool isRedMoon)
    {
        MobSpawner.waveNumber *= (int)(MobSpawner.waveNumber * (1 + 0.2 * _coeff)) ;
    }

    private void UpdateInteractableCosts(float coeff)
    {
        // moneyCost = baseCost * Mathf.Pow(coeff, 1.25f);
        // Adjust the cost of interactables in the game environment
    }

    private void UpdateEnemyRewards(float coeff)
    {
        // enemyXPReward = coeff * monsterValue * rewardMultiplier;
        // enemyGoldReward = 2 * coeff * monsterValue * rewardMultiplier;
        // Adjust the rewards given by enemies
    }
}
